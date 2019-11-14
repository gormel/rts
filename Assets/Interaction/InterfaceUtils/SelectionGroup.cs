using System.Collections.Generic;
using System.Linq;
using Assets.Views.Base;
using UnityEngine;

namespace Assets.Interaction.Selection
{
    class SelectionGroup
    {
        private readonly SelectionManager mSelectionManager;
        private readonly Root mRoot;
        public List<SelectableView> Members { get; } = new List<SelectableView>();
        private int mLastCameraOn;

        public SelectionGroup(SelectionManager selectionManager, Root root)
        {
            mSelectionManager = selectionManager;
            mRoot = root;
        }

        public void Add()
        {
            foreach (var view in mSelectionManager.Selected)
            {
                if (!Members.Contains(view))
                    Members.Add(view);
            }

            mLastCameraOn = 0;
        }

        public void Set()
        {
            Members.Clear();
            Add();
        }

        public void Select()
        {
            var selected = new HashSet<SelectableView>(mSelectionManager.Selected);
            var members = new HashSet<SelectableView>(Members.Where(v => v != null));
            if (selected.SetEquals(members) && mSelectionManager.Selected.Any())
            {
                mRoot.PlaseCamera(Members[mLastCameraOn].FlatBounds.center);
                mLastCameraOn = (mLastCameraOn + Members.Count + 1) % Members.Count;
                return;
            }

            mSelectionManager.Select(Members);
        }
    }
}