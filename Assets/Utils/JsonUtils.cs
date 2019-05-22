using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Utils
{
    static class JsonUtils
    {
        public static JsonSerializer Serializer { get; }
        static JsonUtils()
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.TypeNameHandling = TypeNameHandling.All;
            Serializer = JsonSerializer.Create(serializerSettings);
        }
    }
}
