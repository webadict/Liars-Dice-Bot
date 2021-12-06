using DiscordBot.DiceBot.Game.Abstracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.TexasHoldem.Abstracts
{
    public class DeterminedHand : IComparable<DeterminedHand>
    {
        public TexasHoldemPlayer Player { get; }

        public Ranking Ranking { get; set; } = Ranking.HIGH_CARD;

        public Rank FirstScore { get; set; } = Rank.NONE;
        public Rank SecondScore { get; set; } = Rank.NONE;
        public Rank ThirdScore { get; set; } = Rank.NONE;
        public Rank FourthScore { get; set; } = Rank.NONE;
        public Rank FifthScore { get; set; } = Rank.NONE;

        List<Card> Cards { get; } = new List<Card>();

        public DeterminedHand(TexasHoldemPlayer player)
        {
            Player = player;
        }

        public void AddCard(Card card)
        {
            if (Cards.Count < 5)
            {
                Cards.Add(card);
            }
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            Cards.AddRange(cards.Take(5 - Cards.Count));
        }

        public int CompareTo(DeterminedHand otherHand)
        {
            if (Ranking != otherHand.Ranking)
            {
                return Ranking > otherHand.Ranking ? 1 : -1;
            }
            if (FirstScore != otherHand.FirstScore)
            {
                return FirstScore > otherHand.FirstScore ? 1 : -1;
            }
            if (SecondScore != otherHand.SecondScore)
            {
                return SecondScore > otherHand.SecondScore ? 1 : -1;
            }
            if (ThirdScore != otherHand.ThirdScore)
            {
                return ThirdScore > otherHand.ThirdScore ? 1 : -1;
            }
            if (FourthScore != otherHand.FourthScore)
            {
                return FourthScore > otherHand.FourthScore ? 1 : -1;
            }
            if (FifthScore != otherHand.FifthScore)
            {
                return FifthScore > otherHand.FifthScore ? 1 : -1;
            }
            return 0;
        }

        public string RankString()
        {
            switch (Ranking)
            {
                case Ranking.FIVE_OF_A_KIND:
                    return $"Five of a kind, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Plural}";
                case Ranking.ROYAL_FLUSH:
                    return "Royal Flush";
                case Ranking.STRAIGHT_FLUSH:
                    return $"Straight Flush, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Singular}-high";
                case Ranking.FOUR_OF_A_KIND:
                    return $"Four of a kind, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Plural}";
                case Ranking.FULL_HOUSE:
                    return $"Full house, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Plural} over {EnumHelper.GetAttributeOfType<PluralityAttribute>(SecondScore).Plural}";
                case Ranking.FLUSH:
                    return $"Flush, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Singular}-high";
                case Ranking.STRAIGHT:
                    return $"Straight, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Singular}-high";
                case Ranking.THREE_OF_A_KIND:
                    return $"Three of a kind, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Plural}";
                case Ranking.TWO_PAIR:
                    return $"Two Pair, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Plural} over {EnumHelper.GetAttributeOfType<PluralityAttribute>(SecondScore).Plural}";
                case Ranking.ONE_PAIR:
                    return $"One Pair, {EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Plural}";
                case Ranking.HIGH_CARD:
                    return $"{EnumHelper.GetAttributeOfType<PluralityAttribute>(FirstScore).Singular}-high";
            }
            return "You somehow broke the bot.";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            
            if (obj.GetType() == typeof(DeterminedHand))
            {
                var hand = (DeterminedHand)obj;
                return (Player == hand.Player
                    && Ranking == hand.Ranking
                    && FirstScore == hand.FirstScore
                    && SecondScore == hand.SecondScore
                    && ThirdScore == hand.ThirdScore
                    && FourthScore == hand.FourthScore
                    && FifthScore == hand.FifthScore);
            }
            return false;
        }

        public static bool operator ==(DeterminedHand left, DeterminedHand right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(DeterminedHand left, DeterminedHand right)
        {
            return !(left == right);
        }

        public static bool operator <(DeterminedHand left, DeterminedHand right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(DeterminedHand left, DeterminedHand right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(DeterminedHand left, DeterminedHand right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(DeterminedHand left, DeterminedHand right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
