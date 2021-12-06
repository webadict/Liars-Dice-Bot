namespace DiscordBot.DiceBot.Game.RAF.Actions
{
    public class NoneAction : AbstractAction
    {
        public override ActionOrder ActionOrder => ActionOrder.NO_ORDER;

        public NoneAction() : base(null, null)
        {

        }

        public override void PerformAction()
        {
        }
    }
}
