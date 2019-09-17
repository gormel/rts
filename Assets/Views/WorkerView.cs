using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.GameObjects;
using Assets.Core.GameObjects.Final;
using Assets.Views.Base;
using Assets.Views.Utils;
using UnityEngine;

namespace Assets.Views
{
    sealed class WorkerView : UnitView<IWorkerOrders, IWorkerInfo>
    {
        public override string Name => "Рабочий";

        protected override void OnLoad()
        {
            RegisterProperty(new SelectableViewProperty("X position", () => Info.Position.x.ToString("##.##")));
            RegisterProperty(new SelectableViewProperty("Y position", () => Info.Position.y.ToString("##.##")));
        }

        public void AttachAsBuilder(Guid templateId)
        {
            Orders.AttachAsBuilder(templateId);
        }

        public Task<Guid> PlaceCentralBuilding(Vector2Int position)
        {
            return Orders.PlaceCentralBuildingTemplate(position);
        }

        public Task<Guid> PlaceBarrak(Vector2Int position)
        {
            return Orders.PlaceBarrakTemplate(position);
        }

        public Task<Guid> PlaceMiningCamp(Vector2Int position)
        {
            return Orders.PlaceMiningCampTemplate(position);
        }

        public override void OnRightClick(SelectableView view)
        {
            if (view is BuildingTemplateView)
                AttachAsBuilder(((BuildingTemplateView)view).Info.ID);
        }
    }
}
