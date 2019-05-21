using System;

namespace Assets.Views.Utils
{
    class SelectableViewProperty
    {
        private readonly Func<string> mGetValue;
        public string Name { get; }
        public string Value { get; private set; }

        public SelectableViewProperty(string name, Func<string> getValue)
        {
            mGetValue = getValue;
            Name = name;
        }

        public void Update()
        {
            Value = mGetValue();
        }
    }
}