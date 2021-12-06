using DiscordBot.DiceBot.Game.RAF.Actions;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.DiceBot.Game.RAF
{
    public class RAFActionBook
    {
        protected Dictionary<RAFPlayer, AbstractAction> actions;
        protected RAFController _rafController;

        public RAFActionBook(RAFController rafController)
        {
            actions = new Dictionary<RAFPlayer, AbstractAction>();
            _rafController = rafController;
        }

        public void AddAction(RAFPlayer user, AbstractAction action)
        {
            if (actions.ContainsKey(user))
            {
                actions[user] = action;
            } else
            {
                actions.Add(user, action);
            }
        }

        public void ResolveBook()
        {
            foreach (AbstractAction action in actions.Values.OrderBy(x => x.ActionOrder).ToList())
            {
                action.PerformAction();
            }
            actions.Clear();
        }

        public void Clear()
        {
            actions.Clear();
        }

        public bool IsFull()
        {
            return _rafController.ActivePlayers.All(actions.Keys.Contains);
        }
    }
}
