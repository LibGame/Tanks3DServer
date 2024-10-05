using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class CreateLobbyRequest
    {
        public string owner;
        public LobbyMode lobbyMode;
        public string password;
        public string lobbyName;
        public short maxPlayers;
        public string participantCreatorID;
        public int minBid;
    }
}
