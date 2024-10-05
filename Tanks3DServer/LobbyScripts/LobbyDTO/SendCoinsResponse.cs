﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.Database;
using Tanks3DServer.MoiraiScripts;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class SendCoinsResponse
    {
        public User user;
        public List<Transaction> transactions;
        public bool result;
    }
}
