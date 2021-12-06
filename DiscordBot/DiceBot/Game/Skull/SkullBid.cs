namespace DiscordBot.DiceBot.Game.Skull
{
    public class SkullBid
    {
        private bool _isRevolved = false;

        public SkullController _liarsDiceController;

        public SkullPlayer CurrentBidder { get; private set; }
        public int Quantity { get; private set; } = 0;
        
        public SkullBid(SkullController liarsDiceController)
        {
            _liarsDiceController = liarsDiceController;
        }

        public bool Bid(SkullPlayer bidder, int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }
            if (quantity > Quantity || quantity == Quantity)
            {
                SetBid(bidder: bidder, quantity: quantity);
                return true;
            }
            return false;
        }

        public void ResetBid()
        {
            CurrentBidder = null;
            Quantity = 0;
            _isRevolved = false;
        }

        private void SetBid(SkullPlayer bidder, int quantity)
        {
            CurrentBidder = bidder;
            Quantity = quantity;
        }
    }
}
