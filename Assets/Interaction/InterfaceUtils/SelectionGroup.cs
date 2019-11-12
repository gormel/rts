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
        }

        public void Set()
        {
            Members.Clear();
            Add();
        }

        public void Select()
        {
            if (!mSelectionManager.Selected.Except(Members.Where(v => v != null)).Any() &&
                !Members.Where(v => v != null).Except(mSelectionManager.Selected).Any()
                && mSelectionManager.Selected.Any())
            {
                var posX = mSelectionManager.Selected.Average(v => v.FlatBounds.center.x);
                var posY = mSelectionManager.Selected.Average(v => v.FlatBounds.center.y);
                mRoot.PlaseCamera(new Vector2(posX, posY));
                return;
            }

            mSelectionManager.Select(Members);
        }
    }
}