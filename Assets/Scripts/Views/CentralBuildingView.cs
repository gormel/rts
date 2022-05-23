using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Final;
using Assets.Core.GameObjects.Utils;
using Assets.Utils;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    sealed class CentralBuildingView : FactoryBuildingView<ICentralBuildingOrders, ICentralBuildingInfo>
    {
        public override string Name => "Главное здание";

        public Animation GatesAnimation;
        public AnimationClip OpenClip;
        public AnimationClip CloseClip;
        public GameObject DataWire;
        
        [ColorUsage(true, true)]
        public Color OpenColor;
        
        [ColorUsage(true, true)]
        public Color ClosedColor;
        public MeshRenderer[] GateIndicators;

        protected override void OnLoad()
        {
            base.OnLoad();
            WatchMore(() => Info.Progress, 0.6f, p => { GatesAnimation.Play(OpenClip.name); });
            WatchMore(() => Info.Progress, 0.99f, p => { GatesAnimation.Play(CloseClip.name); });
        }

        protected override void Update()
        {
            base.Update();
            
            DataWire.SetActive(Info.Progress is > 0.01f and < 0.99f);
            foreach (var indicator in GateIndicators)
                indicator.material.SetColor("_EmissionColor", Info.Progress is > 0.6f and < 0.99f ? OpenColor : ClosedColor);
        }

        public override void OnRightClick(Vector2 position)
        {
            Orders.SetWaypoint(position);
        }
    }
}
