using System.Collections.Generic;
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
            void Apply();
        }

        private class Activator<T> : IActivator where T : SelectableView
        {
            private readonly GameObject mTarget;
            private readonly IDictionary<GameObject, bool> mActiveStates;

            public Activator(GameObject target, IDictionary<GameObject, bool> activeStates)
            {
                mTarget = target;
                mActiveStates = activeStates;
            }

            public void UpdateActive(UserInterface userInterface)
            {
                var active = false;

                if (userInterface.Selected.Count > 0 && userInterface.Selected[0].OwnershipRelation == ObjectOwnershipRelation.My)
                    active = userInterface.Selected[0] is T;

                if (mActiveStates.TryGetValue(mTarget, out bool isActive) && isActive)
                    return;

                mActiveStates[mTarget] = active;
            }

            public void Apply()
            {
                var toSet = mActiveStates[mTarget];
                if (mTarget.activeSelf != toSet)
                    mTarget.SetActive(toSet);
            }
        }

        public GameObject WorkerActions;
        public GameObject RangedWarriorActions;
        public GameObject MeeleeWarriorActions;
        public GameObject CentralBuildingActions;
        public GameObject BuildingTemplateActions;
        public GameObject BarrakActions;
        public GameObject MiningCampActions;
        public GameObject TurretActions;
        public GameObject BuildersLabActions;
        public GameObject WarriorsLabActions;

        public UserInterface Interface;

        private IActivator[] mActivators;

        private readonly Dictionary<GameObject, bool> mActiveStates = new Dictionary<GameObject, bool>();

        void Start()
        {
            mActivators = new IActivator[]
            {
                new Activator<WorkerView>(WorkerActions, mActiveStates), 
                new Activator<RangedWarriorView>(RangedWarriorActions, mActiveStates), 
                new Activator<MeeleeWarriorView>(MeeleeWarriorActions, mActiveStates), 
                new Activator<CentralBuildingView>(CentralBuildingActions, mActiveStates), 
                new Activator<BuildingTemplateView>(BuildingTemplateActions, mActiveStates), 
                new Activator<BarrakView>(BarrakActions, mActiveStates),
                new Activator<MiningCampView>(MiningCampActions, mActiveStates),
                new Activator<TurretView>(TurretActions, mActiveStates),
                new Activator<BuildersLabView>(BuildersLabActions, mActiveStates),
                new Activator<WarriorsLabView>(WarriorsLabActions, mActiveStates),
            };
        }

        void Update()
        {
            mActiveStates.Clear();
            for (int i = 0; i < mActivators.Length; i++) 
                mActivators[i].UpdateActive(Interface);
            for (int i = 0; i < mActivators.Length; i++) 
                mActivators[i].Apply();
        }
    }
}
