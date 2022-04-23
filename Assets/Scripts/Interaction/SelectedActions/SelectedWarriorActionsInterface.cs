using System;
using System.Linq;
using Assets.Core.GameObjects.Base;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedWarriorActionsInterface : SelectedUnitsActionsInterface
    {
        public GameObject IdleHighliter;
        public GameObject AggressiveHighliter;
        public GameObject DefenciveHighliter;

        private void Update()
        {
            var warrior = Interface.FetchSelectedInfo<IWarriorInfo>().FirstOrDefault();
            if (warrior != null)
            {
                var strategy = warrior.Strategy;
                IdleHighliter.SetActive(strategy == Strategy.Idle);
                AggressiveHighliter.SetActive(strategy == Strategy.Aggressive);
                DefenciveHighliter.SetActive(strategy == Strategy.Defencive);
            }
        }

        public void BeginAttack()
        {
            Interface.BeginAttack(Interface.FetchSelectedOrders<IWarriorOrders>());
        }

        public void SetIdle()
        {
            foreach (var unitView in Interface.FetchSelectedOrders<IWarriorOrders>())
                unitView.SetStrategy(Strategy.Idle);
        }

        public void SetAgressive()
        {
            foreach (var unitView in Interface.FetchSelectedOrders<IWarriorOrders>())
                unitView.SetStrategy(Strategy.Aggressive);
        }

        public void SetDefencive()
        {
            foreach (var unitView in Interface.FetchSelectedOrders<IWarriorOrders>())
                unitView.SetStrategy(Strategy.Defencive);
        }
    }
}