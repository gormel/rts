using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects.Base;
using UnityEngine;

namespace Assets.Views.Base
{
    abstract class BuildingView<TOrderer, TInfo> : ModelSelectableView<TOrderer, TInfo>
        where TOrderer : IBuildingOrders
        where TInfo : IBuildingInfo
    {
        protected override Vector2 Position => Info.Position + Info.Size / 2;

        private bool mScaled = false;
        private TaskCompletionSource<bool> mScaledTcs = new TaskCompletionSource<bool>();

        protected Task WaitScaledAsync()
        {
            if (mScaled)
                return Task.CompletedTask;

            return mScaledTcs.Task;
        }

        protected override void Update()
        {
            base.Update();
            
            transform.localScale = new Vector3(
                Info.Size.x,
                Mathf.Min(Info.Size.x, Info.Size.y),
                Info.Size.y);

            if (!mScaled)
            {
                mScaledTcs.SetResult(true);
                mScaled = true;
            }
        }
    }
}
