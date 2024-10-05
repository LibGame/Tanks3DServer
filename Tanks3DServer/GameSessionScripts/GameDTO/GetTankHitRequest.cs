using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class GetTankHitRequest
    {
        public string sessionID;
        public string tankID;
        public string tankAttakerID;
        public int hp;
        public bool handleScore;
    }
}
