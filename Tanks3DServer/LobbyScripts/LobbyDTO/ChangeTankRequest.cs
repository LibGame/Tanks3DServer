using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class ChangeTankRequest
    {
        public short tankID;
        public string lobbyParticipantID;
        public short ordinal;
        public string lobbySession;
    }
}
