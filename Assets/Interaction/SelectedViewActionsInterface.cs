using Assets.Views;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction
{
    class SelectedViewActionsInterface : MonoBehaviour
    {
        interface IActivator
        {
            void UpdateActive(UserInterface userInterface);
        }

        private class Activator<T> : IActivator where T : SelectableView
        {
            private readonly GameObject mTarget;

            public Activator(GameObject target)
            {
                mTarget = target;
            }

            public void UpdateActive(UserInterface userInterface)
            {
                var active = false;

                if (userInterface.Selected.Count > 0 && userInterface.Selected[0].IsControlable)
                    active = userInterface.Selected[0] is T;

                mTarget.SetActive(active);
            }
        }

        public GameObject WorkerActions;
        public GameObject RangedWarriorActions;
        public GameObject MeeleeWarriorActions;
        public GameObject CentralBuildingActions;
        public GameObject BuildingTemplateActions;
        public GameObject BarrakActions;
        public GameObject MiningCampActions;

        public UserInterface Interface;

        private IActivator[] mActivators;

        void Start()
        {
            mActivators = new IActivator[]
            {
                new Activator<WorkerView>(WorkerActions), 
                new Activator<RangedWarriorView>(RangedWarriorActions), 
                new Activator<MeeleeWarriorView>(MeeleeWarriorActions), 
                new Activator<CentralBuildingView>(CentralBuildingActions), 
                new Activator<BuildingTemplateView>(BuildingTemplateActions), 
                new Activator<BarrakView>(BarrakActions),
                new Activator<MiningCampView>(MiningCampActions),
            };
        }

        void Update()
        {
            for (int i = 0; i < mActivators.Length; i++)
            {
                mActivators[i].UpdateActive(Interface);
            }
        }
    }
}
