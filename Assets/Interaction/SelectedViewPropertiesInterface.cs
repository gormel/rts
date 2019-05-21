using UnityEngine;

namespace Assets.Interaction
{
    sealed class SelectedViewPropertiesInterface : CollectionInterface
    {
        public UserInterface Interface;

        protected override int ElementCount
        {
            get
            {
                if (Interface.Selected.Count < 1)
                    return 0;

                return Interface.Selected[0].Properties.Count;
            }
        }

        protected override void UpdateChild(int index, GameObject child)
        {
            var prop = Interface.Selected[0].Properties[index];
            var comp = child.GetComponent<PropertyInterface>();
            comp.SetName(prop.Name);
            comp.SetText(prop.Value);
        }

        void Update()
        {
            OnUpdate();
        }
    }
}