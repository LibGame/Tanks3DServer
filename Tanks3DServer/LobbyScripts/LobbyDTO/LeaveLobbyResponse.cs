﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class LeaveLobbyResponse
    {
        public LobbySession lobbySession;
        public string pariticipant;
        public bool result;
    }
}
