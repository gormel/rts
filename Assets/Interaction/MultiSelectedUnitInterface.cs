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
        }

        void Update()
        {
            OnUpdate();
        }
    }
}