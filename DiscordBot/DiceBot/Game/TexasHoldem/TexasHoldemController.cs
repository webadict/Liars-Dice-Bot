using Discord;
using Discord.WebSocket;
using DiscordBot.DiceBot.Game.Abstracts;
using DiscordBot.DiceBot.Game.Interfaces;
using DiscordBot.DiceBot.Game.TexasHoldem.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DiscordBot.DiceBot.Game.TexasHoldem
{
    public class TexasHoldemController : BaseGameController<TexasHoldemPlayer>, IGameController
    {
        private readonly Random _random;

        private const int FLOP = 3;
        private const int TURN = 1;
        private const int RIVER = 1;

        private const int BIG_BLIND = 10;
        private const int LITTLE_BLIND = 5;

        private const int STARTING_CHIPS = 100;

        private const int NUMBER_OF_DECKS = 1;
        
        public override List<TexasHoldemPlayer> Players { get; }
        public List<TexasHoldemPlayer> SittingPlayers
        {
            get
            {
                return Players.Where(x => x.Active && !x.Folded).ToList();
            }
        }
        public List<TexasHoldemPlayer> BettingPlayers
        {
            get
            {
                return Players.Where(x => x.Active && !x.Folded && !x.AllIn).ToList();
            }
        }

        public List<Card> Cards { get; }
        public Deck Deck { get; }

        public TexasHoldemPlayer Dealer { get; private set; }
        public TexasHoldemPlayer CurrentPlayer { get; private set; }
        public TexasHoldemPlayer LastRaisePlayer { get; private set; }
        public TexasHoldemPhase Phase { get; private set; }
        public int CurrentBet { get; private set; }
        public int FullBet { get; private set; }
        public int CurrentPot { get; private set; }

        public TexasHoldemController(Random random) : base()
        {
            _random = random;
            Players = new List<TexasHoldemPlayer>();
            Deck = new Deck(random);
            Cards = new List<Card>();
        }

        public override void ResetGame()
        {
            Deck.Create(NUMBER_OF_DECKS);
            CurrentPlayer = null;
        }

        protected override void EndGame()
        {
            TexasHoldemPlayer winner = ActivePlayers.First();
            var message = $"{winner.User.Mention} has won the game!";
            ActiveChannel.SendMessageAsync(message).Wait();
            StopGame();
        }

        protected override string GetActivePlayerList()
        {
            return $"```Players:\n" + string.Join("\n", ActivePlayers.Select(x => x.GetStatus())) + "```";
        }

        protected override void StartGame()
        {
            foreach (SocketUser user in Users)
            {
                TexasHoldemPlayer player = new TexasHoldemPlayer(user)
                {
                    Chips = STARTING_CHIPS,
                };

                Players.Add(player);
            }
            AdvancePlayer();
            Phase = TexasHoldemPhase.PREFLOP;
            Dealer = GetRandomPlayer();
            StartRound();
        }

        public void StartRound()
        {
            CurrentPot = 0;
            Phase = TexasHoldemPhase.PREFLOP;
            Cards.Clear();
            Deck.Create();
            foreach (TexasHoldemPlayer player in ActivePlayers)
            {
                player.Folded = false;
                player.Pot = 0;
                // Draw two cards.
                player.GiveCard(Deck.DrawRandomCard());
                player.GiveCard(Deck.DrawRandomCard());
                SendStatus(player);
            }
            StartPhase();
            ActiveChannel.SendMessageAsync($"It is {CurrentPlayer.User.Mention}'s turn.").Wait();
        }

        public void StartPhase()
        {
            CurrentBet = 0;
            SetCurrentPlayer(Dealer);
            AdvancePlayer();
            LastRaisePlayer = CurrentPlayer;
            foreach (TexasHoldemPlayer player in SittingPlayers)
            {
                player.Bet = 0;
            }
            switch (Phase)
            {
                case TexasHoldemPhase.PREFLOP:
                    SetBlinds();
                    break;
                case TexasHoldemPhase.FLOP:
                    FlipCard(FLOP);
                    break;
                case TexasHoldemPhase.TURN:
                    FlipCard(TURN);
                    break;
                case TexasHoldemPhase.RIVER:
                    FlipCard(RIVER);
                    break;
            }
            var message = $"{CurrentPlayer.User.Mention} starts the bidding.";

        }

        public void SetBlinds()
        {
            CurrentPot += CurrentPlayer.RaiseBetTo(LITTLE_BLIND);
            AdvancePlayer();
            CurrentPot += CurrentPlayer.RaiseBetTo(BIG_BLIND);
            CurrentBet = BIG_BLIND;
            LastRaisePlayer = CurrentPlayer;
            AdvancePlayer();
        }

        public void EndPhase()
        {
            Phase = Phase.Next();
            if (Phase == TexasHoldemPhase.PREFLOP)
            {
                EndRound();
            }
        }

        public void EndRound()
        {
            DetermineWinner();
            if (ActivePlayers.Count <= 1)
            {
                EndGame();
            }
        }

        public void FlipCard(int numberToFlip)
        {
            List<Card> revealedCards = new List<Card>();
            for (int i = 0; i < numberToFlip; i++)
            {
                Card card = Deck.DrawRandomCard();
                Cards.Add(card);
                revealedCards.Add(card);
            }
            var message = $"{Phase.GetAttributeOfType<DescriptionAttribute>().Description} revealed: {string.Join(" ", revealedCards.Select(x => x.ToString()))}";
        }

        public void Check(TexasHoldemPlayer player)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            if (CurrentBet != 0)
            {
                return;
            }
            AdvancePlayer();
            var message = $"{player.User.Mention} checks.";
            message += $" It is now {CurrentPlayer.User.Mention}'s turn to bid.";
            ActiveChannel.SendMessageAsync(message);
            CalculatePhaseAdvance();
        }

        public void Call(TexasHoldemPlayer player)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            if (CurrentBet == 0)
            {
                Check(player);
                return;
            }
            AdvancePlayer();
            CurrentPot += player.RaiseBetTo(CurrentBet);
            var message = $"{player.User.Mention} ";
            if (player.AllIn)
            {
                message += $"is all in! (Bet: {player.Bet})";
            } else
            {
                message += $"calls for {player.Bet}.";
            }
            message += $" It is now {CurrentPlayer.User.Mention}'s turn to bid.";
            ActiveChannel.SendMessageAsync(message);
            CalculatePhaseAdvance();
        }

        public void Raise(TexasHoldemPlayer player, int raise)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            if (CurrentBet != 0)
            {
                return;
            }
            AdvancePlayer();
            var newBet = CurrentBet + raise;
            CurrentPot += player.RaiseBetTo(newBet);
            CalculateNewBet(player);
            var message = $"{player.User.Mention} ";
            if (player.AllIn)
            {
                message += $"is all in! (Bet: {player.Bet})";
            }
            else
            {
                message += $"raises to {player.Bet}.";
            }
            message += $" It is now {CurrentPlayer.User.Mention}'s turn to bid.";
            ActiveChannel.SendMessageAsync(message);
        }

        public void AllIn(TexasHoldemPlayer player)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            if (CurrentBet != 0)
            {
                return;
            }
            AdvancePlayer();
            var newBet = player.Bet + player.Chips;
            CurrentPot += player.RaiseBetTo(newBet);
            CalculateNewBet(player);
            var message = $"{player.User.Mention} is all in! (Bet: {player.Bet})";
            ActiveChannel.SendMessageAsync(message);
            CalculatePhaseAdvance();
        }

        public void Fold(TexasHoldemPlayer player)
        {
            if (CurrentPlayer != player)
            {
                return;
            }
            if (CurrentBet != 0)
            {
                return;
            }
            player.Folded = true;
            if (SittingPlayers.Count() == 1)
            {
                var message = "";
                ActiveChannel.SendMessageAsync(message);
                SittingPlayers.Single().Chips += CurrentPot;
                EndRound();
            }
            AdvancePlayer();
            CalculatePhaseAdvance();
        }

        public void CalculateNewBet(TexasHoldemPlayer player)
        {
            if (player.Bet > CurrentBet)
            {
                CurrentBet = player.Bet;
                LastRaisePlayer = player;
            }
        }

        public void CalculatePhaseAdvance()
        {
            if (LastRaisePlayer == CurrentPlayer)
            {
                EndPhase();
            } else
            {
                var message = "";
                message += $" It is now {CurrentPlayer.User.Mention}'s turn to bid.";
                ActiveChannel.SendMessageAsync(message);
            }
        }

        public void DetermineWinner()
        {
            List<DeterminedHand> determinedHands = new List<DeterminedHand>();
            foreach (TexasHoldemPlayer player in SittingPlayers)
            {
                determinedHands.Add(GetRanking(player, player.Cards.Concat(Cards).ToList()));
            }
            determinedHands.Sort();
            while (CurrentPot > 0)
            {
                var winningHands = determinedHands.Where(x => x.CompareTo(determinedHands.First()) == 0);
                var pot = winningHands.Select(x => x.Player.Pot).Min();
                int splitPot = pot / winningHands.Count();
                foreach (DeterminedHand hand in winningHands)
                {
                    hand.Player.Chips += splitPot;
                    hand.Player.Pot -= pot;
                    if (hand.Player.Pot == 0)
                    {
                        determinedHands.Remove(hand);
                    }
                }
                CurrentPot -= pot;
            }
        }

        public DeterminedHand GetRanking(TexasHoldemPlayer player, List<Card> cards)
        {
            DeterminedHand finalHand = new DeterminedHand(player);
            int wilds = cards.Count(x => x.Rank == Rank.JOKER);

            var countQuery = cards.Where(x => x.Rank != Rank.JOKER)
                .Select(x => x.Rank)
                .GroupBy(x => x)
                .Select(x => new { Rank = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.Rank);

            var firstItem = countQuery.FirstOrDefault();
            var secondItem = countQuery.ElementAtOrDefault(1);
            if (firstItem.Count + wilds >= 5)
            {
                finalHand.Ranking = Ranking.FIVE_OF_A_KIND;
                finalHand.FirstScore = firstItem.Rank;
                foreach (Card card in cards)
                {
                    if (card.Rank == firstItem.Rank ||
                        card.Rank == Rank.JOKER)
                    {
                        finalHand.AddCard(card);
                    }
                }
                return finalHand;
            }

            var flushQuery = cards.Where(x => x.Suit == Suit.WILDS)
                .Select(x => x.Suit)
                .GroupBy(x => x)
                .Select(x => new { Suit = x.Key, Count = x.Count() })
                .OrderBy(x => x.Count);
            bool flush = flushQuery
                .First()
                .Count + wilds >= 5;
            
            if (flush)
            {
                Suit flushSuit = flushQuery.First().Suit;
                if (ParseStraight(cards.Where(x => x.Suit == flushSuit).ToList(), wilds, out Rank straightFlushRank))
                {
                    finalHand.FirstScore = straightFlushRank;
                    if (straightFlushRank == Rank.ACE_HIGH)
                    {
                        finalHand.Ranking = Ranking.ROYAL_FLUSH;
                    } else
                    {
                        finalHand.Ranking = Ranking.STRAIGHT_FLUSH;
                    }
                    finalHand.AddCards(cards.Where(x => x.Rank == Rank.JOKER));
                    finalHand.AddCards(cards.Where(x => x.Suit == flushSuit && x.Rank <= straightFlushRank).OrderByDescending(x => x.Rank));
                    return finalHand;
                }
            }
            
            if (firstItem.Count + wilds == 4)
            {
                finalHand.Ranking = Ranking.FOUR_OF_A_KIND;
                finalHand.FirstScore = firstItem.Rank;
                finalHand.AddCards(cards.Where(x => x.Rank == firstItem.Rank || x.Rank == Rank.JOKER));
                Card card = cards.Where(x => x.Rank != Rank.JOKER && x.Rank != firstItem.Rank)
                    .OrderByDescending(x => x.Rank)
                    .Take(1)
                    .Single();
                finalHand.AddCard(card);
                finalHand.SecondScore = card.Rank;
            }
            if (firstItem.Count + wilds == 3 && secondItem.Count >= 2)
            {
                finalHand.Ranking = Ranking.FULL_HOUSE;
                finalHand.FirstScore = firstItem.Rank;
                finalHand.SecondScore = secondItem.Rank;
                finalHand.AddCards(cards.Where(x => x.Rank == firstItem.Rank || x.Rank == Rank.JOKER));
                finalHand.AddCards(cards.Where(x => x.Rank == secondItem.Rank).Take(2));
                return finalHand;
            }
            if (flush)
            {
                Suit flushSuit = flushQuery.First().Suit;
                finalHand.Ranking = Ranking.FLUSH;
                List<Card> extraCards = cards.Where(x => x.Suit == flushSuit)
                    .Concat(cards.Where(x => x.Rank == Rank.JOKER))
                    .OrderByDescending(x => x.Rank)
                    .ToList();
                finalHand.FirstScore = cards.ElementAt(0).Rank == Rank.JOKER ? Rank.ACE_HIGH : cards.ElementAt(0).Rank;
                finalHand.SecondScore = cards.ElementAt(1).Rank == Rank.JOKER ? Rank.ACE_HIGH : cards.ElementAt(1).Rank;
                finalHand.ThirdScore = cards.ElementAt(2).Rank == Rank.JOKER ? Rank.ACE_HIGH : cards.ElementAt(2).Rank;
                finalHand.FourthScore = cards.ElementAt(3).Rank == Rank.JOKER ? Rank.ACE_HIGH : cards.ElementAt(3).Rank;
                finalHand.FifthScore = cards.ElementAt(4).Rank == Rank.JOKER ? Rank.ACE_HIGH : cards.ElementAt(4).Rank;
                finalHand.AddCards(cards.Take(5));
            }
            if (ParseStraight(cards, wilds, out Rank straightRank))
            {
                finalHand.FirstScore = straightRank;
                finalHand.Ranking = Ranking.STRAIGHT;
                finalHand.AddCards(cards.Where(x => x.Rank == Rank.JOKER));
                finalHand.AddCards(cards.Where(x => x.Rank <= straightRank).GroupBy(x => x.Rank).OrderByDescending(x => x.Key).Select(x => x.First()));
                return finalHand;
            }
            if (firstItem.Count + wilds == 3)
            {
                finalHand.Ranking = Ranking.THREE_OF_A_KIND;
                finalHand.FirstScore = firstItem.Rank;
                finalHand.AddCards(cards.Where(x => x.Rank == firstItem.Rank || x.Rank == Rank.JOKER));
                List<Card> extraCards = cards.Where(x => x.Rank != firstItem.Rank && x.Rank != Rank.JOKER)
                    .Take(2)
                    .ToList();
                finalHand.SecondScore = extraCards.ElementAt(0).Rank;
                finalHand.ThirdScore = extraCards.ElementAt(1).Rank;
                return finalHand;
            }
            if (firstItem.Count == 2 && secondItem.Count + wilds == 2)
            {
                finalHand.Ranking = Ranking.TWO_PAIR;
                if (firstItem.Rank > secondItem.Rank)
                {
                    finalHand.FirstScore = firstItem.Rank;
                    finalHand.SecondScore = secondItem.Rank;
                } else
                {
                    finalHand.FirstScore = secondItem.Rank;
                    finalHand.SecondScore = firstItem.Rank;
                }
                finalHand.AddCards(cards.Where(x => x.Rank == firstItem.Rank || x.Rank == secondItem.Rank || x.Rank == Rank.JOKER));
                Card card = cards.Where(x => x.Rank != firstItem.Rank && x.Rank != secondItem.Rank && x.Rank != Rank.JOKER)
                    .Take(1)
                    .Single();
                finalHand.ThirdScore = card.Rank;
                finalHand.AddCard(card);
                return finalHand;
            }
            if (firstItem.Count + wilds == 2)
            {
                finalHand.Ranking = Ranking.ONE_PAIR;
                finalHand.FirstScore = firstItem.Rank;
                finalHand.AddCards(cards.Where(x => x.Rank == firstItem.Rank || x.Rank == Rank.JOKER));
                List<Card> extraCards = cards.Where(x => x.Rank != firstItem.Rank && x.Rank != Rank.JOKER)
                    .Take(3)
                    .ToList();
                finalHand.SecondScore = extraCards.ElementAt(0).Rank;
                finalHand.ThirdScore = extraCards.ElementAt(1).Rank;
                finalHand.FourthScore = extraCards.ElementAt(2).Rank;
                return finalHand;
            }
            finalHand.FirstScore = cards.ElementAt(0).Rank;
            finalHand.SecondScore = cards.ElementAt(1).Rank;
            finalHand.ThirdScore = cards.ElementAt(2).Rank;
            finalHand.FourthScore = cards.ElementAt(3).Rank;
            finalHand.FifthScore = cards.ElementAt(4).Rank;
            finalHand.AddCards(cards.Take(5));
            return finalHand;
        }

        public bool ParseStraight(List<Card> cards, int wilds, out Rank straightRank)
        {
            straightRank = Rank.NONE;
            var straightCount = 0;
            var cardsClone = cards.Where(x => x.Rank == Rank.ACE_HIGH)
                .Select(x => Rank.ACE_LOW)
                .Concat(cards.Select(x => x.Rank))
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderByDescending(x => x)
                .ToList();
            for (int i = 4 - wilds; i < cardsClone.Count; i++)
            {
                Rank lastRank = cardsClone.Skip(i - 4 + wilds).Take(1).Single();
                Rank firstRank = cardsClone.Skip(i).Take(1).Single();
                if ((int)lastRank - (int)firstRank <= 4 - wilds)
                {
                    straightRank = lastRank;
                    straightCount = (int)lastRank - (int)firstRank;
                    break;
                }
            }
            if (straightCount == 0)
            {
                return false;
            }
            if (straightCount < 4)
            {
                var rankInt = (int)straightRank + 4 - straightCount;
                if (rankInt > (int)Rank.ACE_HIGH)
                {
                    straightRank = Rank.ACE_HIGH;
                } else
                {
                    straightRank = (Rank)rankInt;
                }
            }
            return true;
        }

        public void SendStatus(TexasHoldemPlayer player)
        {
            player.User.SendMessageAsync(player.GetStatus(true));
        }

        public void AdvancePlayer()
        {
            var query = Players.Where(x => (x.Active && !x.Folded) || x == CurrentPlayer).ToList();
            int index = query.IndexOf(CurrentPlayer);
            index++;
            if (index >= query.Count)
            {
                index = 0;
            }
            SetCurrentPlayer(query[index]);
        }

        public TexasHoldemPlayer GetRandomPlayer()
        {
            if (ActivePlayers.Count == 0)
            {
                return null;
            }
            return ActivePlayers[_random.Next(ActivePlayers.Count)];
        }

        public void SetCurrentPlayer(TexasHoldemPlayer player)
        {
            if (ActivePlayers.Contains(player))
            {
                CurrentPlayer = player;
            }
        }
    }
}
