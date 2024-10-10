using Autofac;
using EnglishWordsServer.AutofacConfiig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.Database;
using Tanks3DServer.Handlers;
using Tanks3DServer.LobbyScripts.LobbyDTO;
using Tanks3DServer.ServerTCPScripts;

namespace Tanks3DServer.LobbyScripts
{
    internal class LobbyHandler
    {
        private LobbyModel _lobbyModel;
        private DatabaseService _databaseService;

        public LobbyHandler()
        {
            _lobbyModel = AutofacProjectContext.Container.Resolve<LobbyModel>();
            _databaseService = AutofacProjectContext.Container.Resolve<DatabaseService>();
        }

        public void Handle(string body , int handle , WebSocketBehavior WebSocket)
        {
            LobbyHandlersType lobbyHandlersType = (LobbyHandlersType)handle;
            Console.WriteLine(lobbyHandlersType);
            if (lobbyHandlersType == LobbyHandlersType.ChangeTankRequest)
            {
                ChangeTankRequest data = JsonConvert.DeserializeObject<ChangeTankRequest>(body);
                if(data != null)
                    _lobbyModel.ChangeTank(data);
            }
            else if (lobbyHandlersType == LobbyHandlersType.ConnectToLobbyRequest)
            {
                ConnectToLobbyRequest data = JsonConvert.DeserializeObject<ConnectToLobbyRequest>(body);
                if (data != null)
                    _lobbyModel.ConnectoToLobby(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.CreateLobbyRequest)
            {
                CreateLobbyRequest data = JsonConvert.DeserializeObject<CreateLobbyRequest>(body);
                if (data != null)
                    _lobbyModel.CreateLobby(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.GetLobbySessionRequest)
            {
                GetLobbySessionsRequest data = JsonConvert.DeserializeObject<GetLobbySessionsRequest>(body);
                if (data != null)
                    _lobbyModel.GetLobbySession(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.LeaveLobbyRequest)
            {
                LeaveLobbyRequest data = JsonConvert.DeserializeObject<LeaveLobbyRequest>(body);
                if (data != null)
                    _lobbyModel.LeaveLobbySession(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.LoginRequest)
            {
                LoginRequest data = JsonConvert.DeserializeObject<LoginRequest>(body);
                if (data != null)
                    _ = _databaseService.LoginAsync(data.Login, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.LobbyParticipantReadyRequest)
            {
                LobbyParticipantReadyRequest data = JsonConvert.DeserializeObject<LobbyParticipantReadyRequest>(body);
                if (data != null)
                    _lobbyModel.ParticipantReady(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.RegisterRequest)
            {
                RegisterRequest data = JsonConvert.DeserializeObject<RegisterRequest>(body);
                if (data != null)
                    _ = _databaseService.RegisterUserAsync(data.Username, WebSocket);
            }
            else if(lobbyHandlersType == LobbyHandlersType.LobbyChatMessageRequest)
            {
                LobbyChatMessageRequest data = JsonConvert.DeserializeObject<LobbyChatMessageRequest>(body);
                if (data != null)
                    _ = _lobbyModel.SendMessageToLobbyChat(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.MakeBidRequest)
            {
                MakeBidRequest data = JsonConvert.DeserializeObject<MakeBidRequest>(body);
                if (data != null)
                    _ = _lobbyModel.MakeBid(data);
            }
            else if(lobbyHandlersType == LobbyHandlersType.GetUserRequest)
            {
                GetUserRequest data = JsonConvert.DeserializeObject<GetUserRequest>(body);
                if (data != null)
                    _ = _lobbyModel.GetUser(data, WebSocket);
            }
            else if(lobbyHandlersType == LobbyHandlersType.GetProfileRequest)
            {
                GetProfileRequest data = JsonConvert.DeserializeObject<GetProfileRequest>(body);
                if (data != null)
                    _ = _lobbyModel.GetProfile(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.SendCoinsRequest)
            {
                SendCoinsRequest data = JsonConvert.DeserializeObject<SendCoinsRequest>(body);
                if (data != null)
                    _ = _lobbyModel.SendCoins(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.BuyTankRequest)
            {
                BuyTankRequest data = JsonConvert.DeserializeObject<BuyTankRequest>(body);
                if (data != null)
                    _ = _lobbyModel.BuyTank(data, WebSocket);
            }
            else if (lobbyHandlersType == LobbyHandlersType.UpdateWalletRequest)
            {
                UpdateWalletRequest data = JsonConvert.DeserializeObject<UpdateWalletRequest>(body);
                if (data != null)
                    _ = _lobbyModel.GetUpdatedWallet(data, WebSocket);
            }
        }
    }
}
