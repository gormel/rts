using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Networking
{
    interface IPackageProcessor
    {
        void Process(JObject package);
        void SendState(JsonTextWriter writer);
    }
}