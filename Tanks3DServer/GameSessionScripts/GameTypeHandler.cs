using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts
{
    internal enum GameTypeHandler
    {
        ConnectToGameSessionRequest = 0,
        ConnectToGameSessionResponse = 1,
        PlayerSendPositionRequest = 2,
        PlayerSendPositionResponse = 3,
        ShootTankRequest = 4,
        ShootTankResponse = 5,
        TankDeathRequest = 6,
        TankDeathResponse = 7,
        TankRespawnRequest = 8,
        TankRespawnResponse = 9,
        TankActiveBonusRequest = 10,
        TankActiveBonusResponse = 11,
        GetTankHitRequest = 12,
        GetTankHitResponse = 13,
        WinGameResponse = 14,
    }
}
