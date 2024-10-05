using Autofac;
using EnglishWordsServer.AutofacConfiig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.GameSessionScripts.GameDTO;
using Tanks3DServer.Handlers;
using Tanks3DServer.LobbyScripts.LobbyDTO;
using Tanks3DServer.ServerUDPScripts;

namespace Tanks3DServer.GameSessionScripts
{
    internal class GameHandler
    {
        private GameController _gameController;

        public void Init()
        {
            _gameController = AutofacProjectContext.Container.Resolve<GameController>();
        }

        public void Handle(string body, int handle, WebSocketUDP iPEndPoint)
        {
            GameTypeHandler lobbyHandlersType = (GameTypeHandler)handle;

            if (lobbyHandlersType == GameTypeHandler.ConnectToGameSessionRequest)
            {
                ConnectToGameSessionRequest data = JsonConvert.DeserializeObject<ConnectToGameSessionRequest>(body);
                if (data != null)
                    _gameController.ConnectToGameSession(data, iPEndPoint);
            }
            else if (lobbyHandlersType == GameTypeHandler.PlayerSendPositionRequest)
            {
                PlayerSendPositionRequest data = JsonConvert.DeserializeObject<PlayerSendPositionRequest>(body);
                if (data != null)
                    _gameController.PlayerSendingPosition(data, iPEndPoint);
            }
            else if (lobbyHandlersType == GameTypeHandler.ShootTankRequest)
            {
                ShootTankRequest data = JsonConvert.DeserializeObject<ShootTankRequest>(body);
                if (data != null)
                    _gameController.ShootTank(data, iPEndPoint);
            }
            else if (lobbyHandlersType == GameTypeHandler.TankDeathRequest)
            {
                TankDeathRequest data = JsonConvert.DeserializeObject<TankDeathRequest>(body);
                if (data != null)
                    _gameController.TankDeath(data, iPEndPoint);
            }
            else if (lobbyHandlersType == GameTypeHandler.TankRespawnRequest)
            {
                TankRespawnRequest data = JsonConvert.DeserializeObject<TankRespawnRequest>(body);
                if (data != null)
                    _gameController.TankRespawn(data, iPEndPoint);
            }
            else if (lobbyHandlersType == GameTypeHandler.TankActiveBonusRequest)
            {
                TankActiveBonusRequest data = JsonConvert.DeserializeObject<TankActiveBonusRequest>(body);
                if (data != null)
                    _gameController.TankActiveBonus(data, iPEndPoint);
            }
            else if (lobbyHandlersType == GameTypeHandler.GetTankHitRequest)
            {
                GetTankHitRequest data = JsonConvert.DeserializeObject<GetTankHitRequest>(body);
                if (data != null)
                    _gameController.TankHit(data, iPEndPoint);
            }
        }
    }
}
