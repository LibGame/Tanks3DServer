using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class CreateLobbyResponse
    {
        public bool result;
        public LobbySession lobbySession;
        public string lobbyName;
        public List<LobbySessionParticipant> participants;
        public int maxPlayerCount;
        public int time;
        public string password;

        public CreateLobbyResponse(LobbySession lobbySession, string lobbyName,int maxPlayerCount, string password)
        {
            this.lobbySession = lobbySession;
            this.lobbyName = lobbyName;
            this.maxPlayerCount = maxPlayerCount;
            this.password = password;
        }
    }
}
