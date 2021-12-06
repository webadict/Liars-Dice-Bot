namespace DiscordBot.DiceBot.Game.RAF.Actions
{
    public class KillAction : AbstractAction
    {
        public override ActionOrder ActionOrder => ActionOrder.KILL;

        public KillAction(RAFPlayer user, RAFPlayer target) : base(user, target)
        {

        }

        public override void PerformAction()
        {
            if (Target.ActionOption == ActionOption.REFLECTING_KILLING)
            {
                Target.ActionOption = ActionOption.REFLECTING;
            }
            if (Target.ActionOption == ActionOption.REFLECTING)
            {
                User.TakeDamage();
            } else
            {
                Target.TakeDamage();
            }
        }
    }
}
