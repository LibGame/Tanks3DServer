using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class ConnectToLobbyResponse
    {
        public LobbyMode lobbyMode;
        public LobbySession lobbySession;
        public string lobbyName;
        public int maxPlayerCount;
        public List<LobbySessionParticipant> participants;
        public int time;
        public bool result;
        public string error;
    }
}
