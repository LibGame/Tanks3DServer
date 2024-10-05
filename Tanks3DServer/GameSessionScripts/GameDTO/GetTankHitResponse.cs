using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class GetTankHitResponse
    { 
        public string tankID;
        public int hp;
        public int score;
        public string tankAttaker;
    }
}
