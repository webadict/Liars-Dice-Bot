namespace DiscordBot.DiceBot.Game.LiarsDice
{
    public class LiarsDiceBid
    {
        private bool _isRevolved = false;

        public LiarsDiceController _liarsDiceController;

        public LiarsDicePlayer CurrentBidder { get; private set; }
        public int Quantity { get; private set; } = 0;
        public int Rank { get; private set; } = 0;
        public int Total { get; private set; } = 0;
        
        public LiarsDiceBid(LiarsDiceController liarsDiceController)
        {
            _liarsDiceController = liarsDiceController;
        }

        public bool Bid(LiarsDicePlayer bidder, int quantity, int rank)
        {
            if (quantity <= 0 || rank <= 0)
            {
                return false;
            }
            if (quantity > Quantity || quantity == Quantity && (!_isRevolved && rank > Rank
                || _isRevolved && rank < Rank))
            {
                SetBid(bidder: bidder, quantity: quantity, rank: rank);
                if (_liarsDiceController.Mods.Revolution && quantity == rank)
                {
                    _isRevolved = !_isRevolved;
                    if (_isRevolved)
                    {
                        _liarsDiceController.ActiveChannel.SendMessageAsync("REVOLUTION! (1s are high)").Wait();
                    } else
                    {
                        _liarsDiceController.ActiveChannel.SendMessageAsync($"COUNTER-REVOLUTION! ({_liarsDiceController.Mods.NumberOfSides}s are high)").Wait();
                    }
                }
                return true;
            }
            return false;
        }

        public bool Bid(LiarsDicePlayer bidder, int total)
        {
            if (total <= Total)
            {
                return false;
            }
            SetBid(bidder: bidder, total: total);
            return true;
        }

        public void ResetBid()
        {
            CurrentBidder = null;
            Quantity = 0;
            Rank = 0;
            _isRevolved = false;
        }

        private void SetBid(LiarsDicePlayer bidder, int quantity, int rank)
        {
            CurrentBidder = bidder;
            Quantity = quantity;
            Rank = rank;
        }

        private void SetBid(LiarsDicePlayer bidder, int total)
        {
            CurrentBidder = bidder;
            Total = total;
        }
    }
}
