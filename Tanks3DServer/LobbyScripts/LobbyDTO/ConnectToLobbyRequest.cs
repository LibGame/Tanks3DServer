using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class ConnectToLobbyRequest
    {
        public LobbyMode lobbyMode;
        public string sessionID;
        public string playerID;
        public string password;
    }
}
