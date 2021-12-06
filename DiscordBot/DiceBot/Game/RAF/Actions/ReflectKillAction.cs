namespace DiscordBot.DiceBot.Game.RAF.Actions
{
    public class ReflectKillAction : AbstractAction
    {
        public override ActionOrder ActionOrder => ActionOrder.REFLECT;

        public ReflectKillAction(RAFPlayer user) : base(user, null)
        {

        }

        public override void PerformAction()
        {
            User.ActionOption = ActionOption.REFLECTING_KILLING;
        }
    }
}
