using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class PlayerSendPositionResponse
    {
        public string playerID;
        public Vector tankPosition { get; set; }
        public Vector turretRotation { get; set; }
        public Vector tankRotation { get; set; }
    }
}
