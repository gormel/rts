using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Map;
using UnityEngine;

namespace Assets.Utils
{
    static class GameUtils
    {
        public static Vector3 GetPosition(Vector2 flatPosition, Map map)
        {
            return new Vector3(flatPosition.x, map.GetHeightAt(flatPosition), flatPosition.y);
        }

        public static Vector2 GetFlatPosition(Vector3 position)
        {
            return new Vector2(position.x, position.z);
        }
    }
}
