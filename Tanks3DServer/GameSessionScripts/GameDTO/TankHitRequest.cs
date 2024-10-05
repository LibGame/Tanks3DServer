using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    public class TankHitRequest
    {
        public string tankAttakerID;
        public string tankTargetID;
        public string gameSession;
    }
}
