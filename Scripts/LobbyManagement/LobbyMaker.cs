using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Session;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace LobbyManagement
{
    public class LobbyMaker
    {
        private bool isHost = false;
        private SessionMaker _sessionMaker;
        private string playerName;
        public const string KEY_RELAY_CODE = "RelayCode";

        private Lobby joinedLobby;
        //ロビー人数が揃ったことを通知するイベント
        private UniTaskCompletionSource _lobbyFullMemberTcs = new UniTaskCompletionSource();

        public LobbyMaker(SessionMaker sessionMaker)
        {
            this._sessionMaker = sessionMaker;
        }

        public async UniTask Initialize()
        {
            playerName = "PlayerName" + Random.Range(0, 1000).ToString();
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);
            
            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in:" + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private async UniTask CreateLobby()
        {
            try
            {
                string lobbyName = "LobbyName";
                int maxPlayers = 2;
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
                {
                    IsPrivate = false,
                    Player = new Player(AuthenticationService.Instance.PlayerId)
                    {
                        Data = new Dictionary<string, PlayerDataObject>()
                        {
                            {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}
                        }
                    },
                    Data = new Dictionary<string, DataObject>()
                    {
                        {KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, "0")}
                    }
                };
                isHost = true;
                joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                Debug.Log("Lobby created: " + joinedLobby.LobbyCode);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("Exception:" + e.Message);
            }
        }

        public async UniTask QuickJoinLobby()
        {
            try
            {
                QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
                options.Filter = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.MaxPlayers,
                        op: QueryFilter.OpOptions.GE,
                        value: "2"
                    )
                };

                joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
                Debug.Log("Lobby joined: " + joinedLobby.LobbyCode);
                Debug.Log(joinedLobby.Players.Count.ToString());
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("no lobby found: " + e.Message);
                await CreateLobby();
                Debug.Log(joinedLobby);
                Debug.Log(joinedLobby.Players.Count.ToString());
            }
        }

        public async UniTask CreateRelay()
        {
            if (isHost)
            {
                try
                {
                    Debug.Log("Create Relay");
                    string relayCode = await _sessionMaker.CreateRelay();
                    Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(
                        joinedLobby.Id, new UpdateLobbyOptions
                        {
                            Data = new Dictionary<string, DataObject>
                            {
                                { KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                            }
                        });
                    Debug.Log("Relay started: " + relayCode);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log("Error starting lobby: " + e.Message);
                }
            }
        }

        public async UniTask HandleLobbyPolling()
        {
            bool isTaskCompleted = false;
            bool isJoinningGuest = false;
            while (!isTaskCompleted)
            {
                if(joinedLobby == null){continue;}

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);

                if (isHost)
                {
                    Debug.Log("Polling Host");
                    if(_sessionMaker.RelayCode.Equals("null"))
                    {
                        Debug.Log("joinedLobby Count: " + joinedLobby.Players.Count);
                        if (joinedLobby.Players.Count == 2)
                        {
                            //人数が揃ったことを示すイベントを送信
                            _lobbyFullMemberTcs.TrySetResult();
                            isTaskCompleted = true;
                        }
                    }
                }
                if (!isHost)
                {
                    if (isJoinningGuest) {continue; }
                    if (joinedLobby.Data[KEY_RELAY_CODE].Value != "0")
                    {
                        isJoinningGuest = true;
                        string relayCode = joinedLobby.Data[KEY_RELAY_CODE].Value;
                        await _sessionMaker.JoinRelay(relayCode);
                        Debug.Log("Guest join relay : " + relayCode);
                        joinedLobby = null;
                        isTaskCompleted = true;
                    }
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
        }

        public async UniTask Heartbeat()
        {
            if (!isHost) {return;}
            while (joinedLobby != null)
            {
                try
                {
                    Debug.Log("Heartbeat sent");
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log("Error sending heartbeat: " + e.Message);
                }

                //15秒に1回Heartbeatを送る
                await UniTask.Delay(TimeSpan.FromSeconds(25));
            }
        }

        public async UniTask IsJoinedFullMember()
        {
            //ロビー内のメンバーが2人になるまでawaitする
            await _lobbyFullMemberTcs.Task;
        }
    }
}