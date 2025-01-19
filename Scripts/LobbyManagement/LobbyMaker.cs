using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Session;
using TMPro;
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

        private TextMeshProUGUI sessionInfoMessage;
        
        private Lobby joinedLobby;
        //ロビー人数が揃ったことを通知するイベント
        private UniTaskCompletionSource _lobbyFullMemberTcs = new UniTaskCompletionSource();

        public LobbyMaker(SessionMaker sessionMaker)
        {
            this._sessionMaker = sessionMaker;
        }

        public async UniTask Initialize(TextMeshProUGUI infoText, CancellationToken ct)
        {
            sessionInfoMessage = infoText;
            sessionInfoMessage.text = "Initializing...";
            playerName = "PlayerName" + Random.Range(0, 1000);

            try
            {
                InitializationOptions initializationOptions = new InitializationOptions();
                initializationOptions.SetProfile(playerName);
            
                await UnityServices.InitializeAsync(initializationOptions);
                ct.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Canceled");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in:" + AuthenticationService.Instance.PlayerId);
                sessionInfoMessage.text = "Signed in:" + AuthenticationService.Instance.PlayerId;
            };

            try
            {
                //まだサインインしていなかったら実行
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    ct.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Canceled");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async UniTask CreateLobby(CancellationToken ct)
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
                ct.ThrowIfCancellationRequested();
                Debug.Log("Lobby created: " + joinedLobby.LobbyCode);
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Canceled");
                
                //ロビーを削除
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                throw;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("Exception:" + e.Message);
                if(joinedLobby != null)
                {
                    await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                }
            }
        }

        public async UniTask QuickJoinLobby(CancellationToken ct)
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

                sessionInfoMessage.text = "Searching for lobby...";
                joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
                ct.ThrowIfCancellationRequested();
                Debug.Log("Lobby joined: " + joinedLobby.LobbyCode);
                Debug.Log(joinedLobby.Players.Count.ToString());
                sessionInfoMessage.text = "Lobby joined: " + joinedLobby.LobbyCode;
            }
            catch (OperationCanceledException e)
            {
                Debug.Log("Canceled");
                //ロビーを削除
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                throw;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log("no lobby found: " + e.Message);
                sessionInfoMessage.text = "No lobby found. Creating new lobby...";
                await CreateLobby(ct);
                Debug.Log(joinedLobby);
                Debug.Log(joinedLobby.Players.Count.ToString());
                sessionInfoMessage.text = "Lobby created: " + joinedLobby.LobbyCode;
                sessionInfoMessage.text = "Waiting for another player...";
            }
        }

        public async UniTask CreateRelay()
        {
            if (isHost)
            {
                try
                {
                    Debug.Log("Create Relay");
                    sessionInfoMessage.text = "Creating Relay...";
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

        public async UniTask HandleLobbyPolling(CancellationToken ct)
        {
            bool isTaskCompleted = false;
            bool isJoinningGuest = false;
            while (!isTaskCompleted)
            {
                if(joinedLobby == null){continue;}

                try
                {
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException e)
                {
                    Debug.Log("Canceled");
                    if (joinedLobby != null)
                    {
                        await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                    }
                    throw;
                }

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
                        sessionInfoMessage.text = "Guest join relay : " + relayCode;
                        joinedLobby = null;
                        isTaskCompleted = true;
                        _lobbyFullMemberTcs.TrySetResult();
                    }
                }
                
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            }
        }

        public async UniTask Heartbeat(CancellationToken ct)
        {
            while (joinedLobby != null)
            {
                try
                {
                    Debug.Log("Heartbeat sent");
                    await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException e)
                {
                    Debug.Log("Canceled");
                    if (joinedLobby != null)
                    {
                        await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                    }
                    throw;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log("Error sending heartbeat: " + e.Message);
                }

                //15秒に1回Heartbeatを送る
                await UniTask.Delay(TimeSpan.FromSeconds(25), cancellationToken: ct);
            }
        }

        public async UniTask IsJoinedFullMember(CancellationToken ct)
        {
            Debug.Log("IsJoinedFullMember1");
            sessionInfoMessage.text = "Waiting for another player...";
            //ロビー内のメンバーが2人になるまでawaitする
            await _lobbyFullMemberTcs.Task.AttachExternalCancellation(ct);//todo ここのキャンセルの書き方があってるのかわからない
            Debug.Log("IsJoinedFullMember2");
        }
    }
}