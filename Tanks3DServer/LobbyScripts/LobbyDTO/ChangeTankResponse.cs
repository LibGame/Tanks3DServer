﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class ChangeTankResponse
    {
        public bool result;
        public List<LobbySessionParticipant> participants;
    }
}
