using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Interaction
{
    sealed class MultiSelectedUnitInterface : CollectionInterface
    {
        public UserInterface Interface;
        protected override int ElementCount => Interface.Selected.Count;

        protected override void UpdateChild(int index, GameObject child)
        {
            var view = Interface.Selected[index];
            var instInterface = child.GetComponent<IconNameHealthInterface>();
            instInterface.SetIcon(view.Icon);
            instInterface.SetName(view.Name);
            instInterface.SetHealth(view.Health, view.MaxHealth);
            if (view.InfoBase is IQueueOrdersInfo orderSelected)
                instInterface.SetProgress(orderSelected.Progress);
            instInterface.Interface = Interface;
            instInterface.Owner = view;
        }

        void Update()
        {
            OnUpdate();
        }
    }
}