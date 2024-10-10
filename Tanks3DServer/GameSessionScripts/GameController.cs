using Autofac;
using Azure;
using EnglishWordsServer.AutofacConfiig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.Database;
using Tanks3DServer.DTO;
using Tanks3DServer.GameSessionScripts.GameDTO;
using Tanks3DServer.LobbyScripts;
using Tanks3DServer.ServerUDPScripts;

namespace Tanks3DServer.GameSessionScripts
{
    internal class GameController
    {
        private List<GameSession> _gameSessions = new List<GameSession>();
        private DatabaseService _databaseService;

        public GameController()
        {
            _databaseService = AutofacProjectContext.Container.Resolve<DatabaseService>();
        }

        public void RemoveParticipantFromSession(WebSocketUDP WebSocketUDP)
        {
            GameSession session = _gameSessions.FirstOrDefault(x => x.participants.Select(y => y.WebSocketUDP).Contains(WebSocketUDP));
            if (session != null)
            {
                session.RemoveParticipantByWebSocket(WebSocketUDP);
                if(session.participants.Count <= 0)
                {
                    _gameSessions.Remove(session);
                }
            }
        }

        public void CreateGameSession(string sessionID, LobbyMode lobbyMode)
        {
            Console.WriteLine("CreateGameSession " + sessionID);
            GameSession session = new GameSession();
            session.InitGameSession();
            session.sessionID = sessionID;
            session.lobbyMode = lobbyMode;
            _gameSessions.Add(session);
        }

        private bool TryGetGameSessionByID(string sessionID, out GameSession gameSession)
        {
            gameSession = _gameSessions.FirstOrDefault(x => x.sessionID == sessionID);
            if(gameSession == null)
            {
                return false;
            }
            return true;
        }

        private bool TryGetPlayerInSession(string playerID , GameSession session , out GameParticipant gameParticipant)
        {
            gameParticipant = session.participants.FirstOrDefault(x => x.id == playerID);
            if(gameParticipant == null)
            {
                return false;
            }
            return true;
        }


        public async Task ConnectToGameSession(ConnectToGameSessionRequest connectToGameSessionRequest, WebSocketUDP WebSocketUDP)
        {
            if(TryGetGameSessionByID(connectToGameSessionRequest.gameSession, out GameSession gameSession))
            {
                Console.WriteLine("ConnectToGameSession " + gameSession.sessionID);
                GameParticipant gameParticipant = new GameParticipant();
                gameParticipant.command = connectToGameSessionRequest.command;
                gameParticipant.id = connectToGameSessionRequest.playerID;
                gameParticipant.WebSocketUDP = WebSocketUDP;
                gameParticipant.bid = connectToGameSessionRequest.bid;
                gameParticipant.userID = connectToGameSessionRequest.userID;
                gameSession.participants.Add(gameParticipant);
                gameSession.sumBid += gameParticipant.bid;

                _ = _databaseService.SubtractCurrencyAsync(gameParticipant.userID, gameParticipant.bid);

                ConnectToGameSessionResponse response = new ConnectToGameSessionResponse();
                response.bonusDatas = gameSession.bonusDatas;
                response.result = true;
                response.participant = gameParticipant;

                string body = JsonConvert.SerializeObject(response);
                Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.ConnectToGameSessionResponse, "", body);
                string messageData = JsonConvert.SerializeObject(message);
                _ = WebSocketUDP.SendMessageAsync(messageData);
            }
        }

        public void PlayerSendingPosition(PlayerSendPositionRequest playerSendPositionRequest, WebSocketUDP WebSocketUDP)
        {
            if (TryGetGameSessionByID(playerSendPositionRequest.gameSession, out GameSession gameSession))
            {
                Console.WriteLine("gameSession.participants " + gameSession.participants.Count);
                foreach(var participant in gameSession.participants)
                {
                    if(participant.id != playerSendPositionRequest.playerID)
                    {
                        Console.WriteLine("Sending info to " + participant.id);
                        PlayerSendPositionResponse playerSendPositionResponse = new PlayerSendPositionResponse();
                        playerSendPositionResponse.tankPosition = playerSendPositionRequest.tankPosition;
                        playerSendPositionResponse.tankRotation = playerSendPositionRequest.tankRotation;
                        playerSendPositionResponse.turretRotation = playerSendPositionRequest.turretRotation;

                        playerSendPositionResponse.playerID = playerSendPositionRequest.playerID;
                        string body = JsonConvert.SerializeObject(playerSendPositionResponse);
                        Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.PlayerSendPositionResponse, "", body);
                        string messageData = JsonConvert.SerializeObject(message);
                        participant.WebSocketUDP.SendMessageAsync(messageData);
                    }
                }
            }
        }

        public void TankActiveBonus(TankActiveBonusRequest request, WebSocketUDP WebSocketUDP)
        {
            if (TryGetGameSessionByID(request.gameSession, out GameSession gameSession))
            {
                gameSession.UpdateDeleteAndCreateBonus(request.bonusData);
                foreach (var participant in gameSession.participants)
                {
                    TankActiveBonusResponse activeBonus = new TankActiveBonusResponse();

                    activeBonus.bonusDatas = gameSession.bonusDatas;
                    string body = JsonConvert.SerializeObject(activeBonus);
                    Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.TankActiveBonusResponse, "", body);
                    string messageData = JsonConvert.SerializeObject(message);
                    participant.WebSocketUDP.SendMessageAsync(messageData);
                }
            }
        }

        public void TankHit(GetTankHitRequest request, WebSocketUDP WebSocketUDP)
        {
            if (TryGetGameSessionByID(request.sessionID, out GameSession gameSession))
            {
                GameParticipant gameParticipant = gameSession.participants.FirstOrDefault(x => x.id == request.tankAttakerID);

                if(request.hp != 0 && request.handleScore)
                {
                    gameParticipant.score++;
                }

                foreach (var participant in gameSession.participants)
                {
                    GetTankHitResponse getTankHit = new GetTankHitResponse();

                    getTankHit.tankID = request.tankID;
                    getTankHit.hp = request.hp;
                    getTankHit.score = gameParticipant.score;
                    getTankHit.tankAttaker = request.tankAttakerID;
                    string body = JsonConvert.SerializeObject(getTankHit);
                    Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.GetTankHitResponse, "", body);
                    string messageData = JsonConvert.SerializeObject(message);
                    participant.WebSocketUDP.SendMessageAsync(messageData);
                }
            }
        }

        public void ShootTank(ShootTankRequest request, WebSocketUDP WebSocketUDP)
        {
            if (TryGetGameSessionByID(request.gameSession, out GameSession gameSession))
            {
                foreach (var participant in gameSession.participants)
                {
                    if (participant.id != request.playerID)
                    {
                        ShootTankResponse shootTankResponse = new ShootTankResponse();

                        shootTankResponse.playerID = request.playerID;
                        string body = JsonConvert.SerializeObject(shootTankResponse);
                        Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.ShootTankResponse, "", body);
                        string messageData = JsonConvert.SerializeObject(message);
                        participant.WebSocketUDP.SendMessageAsync(messageData);
                    }
                }
            }
        }

        public async void TankDeath(TankDeathRequest request, WebSocketUDP WebSocketUDP)
        {
            if (TryGetGameSessionByID(request.gameSession, out GameSession gameSession))
            {
                if (gameSession.isEndSession)
                    return;

                gameSession.isEndSession = true;

                GameParticipant gameParticipant = gameSession.participants.FirstOrDefault(x => x.id == request.playerID);
                GameParticipant gameParticipantAttaker = gameSession.participants.FirstOrDefault(x => x.id == request.playerAttakerID);

                if (gameParticipant != null)
                {
                    gameParticipant.isDeath = true;
                }

                if(request.handleScore)
                    gameParticipantAttaker.score += 3;

                foreach (var participant in gameSession.participants)
                {
                    TankDeathResponse response = new TankDeathResponse();

                    response.playerID = request.playerID;
                    response.attakerTankID = request.playerAttakerID;
                    if (gameParticipantAttaker != null)
                        response.score = gameParticipantAttaker.score;
                    else
                        response.score = -1;
                    string body = JsonConvert.SerializeObject(response);
                    Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.TankDeathResponse, "", body);
                    string messageData = JsonConvert.SerializeObject(message);
                    _ = participant.WebSocketUDP.SendMessageAsync(messageData);
                }
                float totalCoins = gameSession.sumBid;

                Console.WriteLine("totalCoins " + totalCoins);

                if (gameSession.lobbyMode == LobbyMode.Solo)
                {
                    GameParticipant[] aliveGameParticipants = gameSession.participants.Where(x => !x.isDeath).ToArray();
                    GameParticipant[] deathGameParticipants = gameSession.participants.Where(x => x.isDeath).ToArray();

                    int totalScore = aliveGameParticipants.Select(x => x.score).Sum();
                    if (aliveGameParticipants.Length == 1)
                    {
                        float commision = totalCoins * 0.1f;

                        WinGameResponse winGameResponse = new WinGameResponse();
                        winGameResponse.tankWinnerID = aliveGameParticipants[0].id;
                        winGameResponse.bidWin = (totalCoins - commision) - aliveGameParticipants[0].bid;
                        winGameResponse.commision = commision;
                        await _databaseService.AddCurrencyAsync(aliveGameParticipants[0].userID, (decimal)(totalCoins - commision));

                        string body = JsonConvert.SerializeObject(winGameResponse);
                        Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.WinGameResponse, "", body);
                        string messageData = JsonConvert.SerializeObject(message);
                        _ = aliveGameParticipants[0].WebSocketUDP.SendMessageAsync(messageData);
                    }
                }
                else
                {
                    GameParticipant[] redParticipants = gameSession.participants.Where(x => !x.isDeath && x.command == Command.Red).ToArray();
                    GameParticipant[] blueParticipants = gameSession.participants.Where(x => !x.isDeath && x.command == Command.Blue).ToArray();

                    if(redParticipants.Length == 0)
                    {
                        int totalScore = blueParticipants.Select(x => x.score).Sum();

                        foreach (var item in blueParticipants)
                        {
                            double percentage = (double)item.score / totalScore;
                            float playerCoins = (int)Math.Round(percentage * totalCoins);
                            float commision = playerCoins * 0.1f;

                            WinGameResponse winGameResponse = new WinGameResponse();
                            winGameResponse.tankWinnerID = item.id;
                            winGameResponse.bidWin = playerCoins - commision;
                            winGameResponse.commision = commision;

                            await _databaseService.AddCurrencyAsync(item.userID, (decimal)(playerCoins - commision));
                            Console.WriteLine("Записали с " + item.userID + " сумму  " + item.bid);

                            string body = JsonConvert.SerializeObject(winGameResponse);
                            Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.WinGameResponse, "", body);
                            string messageData = JsonConvert.SerializeObject(message);
                            _ = item.WebSocketUDP.SendMessageAsync(messageData);
                        }

                    }
                    else if(blueParticipants.Length == 0)
                    {
                        int totalScore = redParticipants.Select(x => x.score).Sum();

                        foreach (var item in redParticipants)
                        {
                            double percentage = (double)item.score / totalScore;
                            float playerCoins = (int)Math.Round(percentage * totalCoins);
                            float commision = playerCoins * 0.1f;

                            WinGameResponse winGameResponse = new WinGameResponse();
                            winGameResponse.tankWinnerID = item.id;
                            winGameResponse.bidWin = playerCoins - commision;
                            winGameResponse.commision = commision;

                            await _databaseService.AddCurrencyAsync(item.userID, (decimal)(playerCoins - commision));
                            Console.WriteLine("Записали с " + item.userID + " сумму  " + item.bid);

                            string body = JsonConvert.SerializeObject(winGameResponse);
                            Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.WinGameResponse, "", body);
                            string messageData = JsonConvert.SerializeObject(message);
                            _ = item.WebSocketUDP.SendMessageAsync(messageData);
                        }

                    }
                }
            }
        }

        public void CalculatePercentForPlayer(GameParticipant[] gameParticipants , int totalBids)
        {
            int totalCoins = totalBids;
            int totalScore = gameParticipants.Select(x => x.score).Sum();

            Console.WriteLine($"Общий счет: {totalScore}");

            foreach (var player in gameParticipants)
            {
                double percentage = (double)player.score / totalScore;

                int playerCoins = (int)Math.Round(percentage * totalCoins);
            }
        }

        public void TankRespawn(TankRespawnRequest request, WebSocketUDP WebSocketUDP)
        {
            if (TryGetGameSessionByID(request.gameSession, out GameSession gameSession))
            {
                foreach (var participant in gameSession.participants)
                {
                    TankRespawnResponse response = new TankRespawnResponse();

                    response.playerID = request.playerID;
                    string body = JsonConvert.SerializeObject(response);
                    Message message = new Message(Handlers.HandlerTypes.Game, (int)GameTypeHandler.TankRespawnResponse, "", body);
                    string messageData = JsonConvert.SerializeObject(message);
                    participant.WebSocketUDP.SendMessageAsync(messageData);
                }
            }
        }
    }
}
