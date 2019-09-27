using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Assets.Networking.Lobby
{
    class LobbyServiceImpl : LobbyService.LobbyServiceBase
    {
        public override Task ListenStart(Empty request, IServerStreamWriter<StartState> responseStream, ServerCallContext context)
        {
            return base.ListenStart(request, responseStream, context);
        }

        public override Task ListenUserState(Empty request, IServerStreamWriter<UserState> responseStream, ServerCallContext context)
        {
            return base.ListenUserState(request, responseStream, context);
        }
    }
}
