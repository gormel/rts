using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views.Base;

namespace Assets.Interaction
{
    class SelectedWarriorActionsInterface : SelectedUnitsActionsInterface
    {
        public void BeginAttack()
        {
            Interface.BeginAttack(FetchSelectedOrders<IWarriorOrders>());
        }

        public void SetIdle()
        {
            foreach (var unitView in FetchSelectedOrders<IWarriorOrders>())
                unitView.SetStrategy(Strategy.Idle);
        }

        public void SetAgressive()
        {
            foreach (var unitView in FetchSelectedOrders<IWarriorOrders>())
                unitView.SetStrategy(Strategy.Aggressive);
        }

        public void SetDefencive()
        {
            foreach (var unitView in FetchSelectedOrders<IWarriorOrders>())
                unitView.SetStrategy(Strategy.Defencive);
        }
    }
}