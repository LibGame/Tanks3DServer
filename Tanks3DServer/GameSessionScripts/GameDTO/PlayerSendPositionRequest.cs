using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class PlayerSendPositionRequest
    {
        public string playerID;
        public string gameSession;
        public Vector tankPosition { get; set; }
        public Vector turretRotation { get; set; }
        public Vector tankRotation { get; set; }
    }
}
