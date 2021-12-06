namespace DiscordBot.DiceBot.Game.RAF.Actions
{
    public abstract class AbstractAction
    {
        abstract public ActionOrder ActionOrder { get; }

        public RAFPlayer User { get; protected set; }
        public RAFPlayer Target { get; protected set; }

        protected AbstractAction(RAFPlayer user, RAFPlayer target)
        {
            User = user;
            Target = target;
        }

        public abstract void PerformAction();
    }
}
