using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class PlayerConnectToLobby
    {
        public List<LobbySessionParticipant> participants;
        public string lobbySession;
        public string lobbyName;
        public int maxPlayerCount;
        public int time;

        public PlayerConnectToLobby(int time,List<LobbySessionParticipant> participants, string lobbySession, string lobbyName, int maxPlayerCount)
        {
            this.time = time;
            this.participants = participants;
            this.lobbySession = lobbySession;
            this.lobbyName = lobbyName;
            this.maxPlayerCount = maxPlayerCount;
        }
    }
}
