﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class LobbyParticipantReadyRequest
    {
        public string participantID;
        public string lobbyID;
        public bool ready;
    }
}
