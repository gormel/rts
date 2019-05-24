using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Networking
{
    interface IPackageProcessor
    {
        void Process(JObject data);
        void SendState(JsonTextWriter writer);
    }
}