using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.Handlers
{
    internal enum LobbyHandlersType
    {
        ChangeTankRequest = 0,
        ChangeTankResponse = 1,
        ConnectToLobbyRequest = 2,
        ConnectToLobbyResponse = 3,
        CreateLobbyRequest = 4,
        CreateLobbyResponse = 5,
        GetLobbySessionResponse = 6,
        GetLobbySessionRequest = 7,
        PlayerConnectToLobby = 8,
        LeaveLobbyRequest = 9,
        LeaveLobbyResponse = 10,
        LoginRequest = 11,
        LoginResponse = 12,
        UpdateLobbyResponse = 13,
        StartGameSessionRequest = 14,
        StartGameSessionResponse = 15,
        LobbyParticipantReadyRequest = 16,
        LobbyParticipantReadyResponse = 17,
        RegisterRequest = 18,
        RegisterResponse = 19,
        LobbyChatMessageRequest = 20,
        LobbyChatMessageResponse = 21,
        MakeBidRequest = 22,
        MakeBidResponse = 23,
        GetUserRequest = 24,
        GetUserResponse = 25,
        GetProfileRequest = 26,
        GetProfileResponse = 27,
        SendCoinsRequest = 28,
        SendCoinsResponse = 29,
        BuyTankRequest = 30,
        BuyTankResponse = 31,
    }
}
