using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class LobbyParticipantReadyResponse
    {
        public bool result;
        public bool isStartedGame;
        public bool ready;
        public string participantID;
    }
}
