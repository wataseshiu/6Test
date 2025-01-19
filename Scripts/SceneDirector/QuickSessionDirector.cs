using System.Threading;
using CanvasController;
using Cysharp.Threading.Tasks;
using LobbyManagement;
using Session;
using UI;
using UnityEngine;
using VContainer.Unity;
using VContainer;

namespace SceneDirector
{
    public class QuickSessionDirector
    {
        private LobbyMaker _lobbyMaker;
        private SessionMaker _sessionMaker;

        public QuickSessionDirector(LobbyMaker lobbyMaker, SessionMaker sessionMaker)
        {
            _lobbyMaker = lobbyMaker;
            _sessionMaker = sessionMaker;
        }

        public async UniTask CreateQuickJoinSession(DialogCreateSession dialogCreateSession)
        {
            var ct = dialogCreateSession.Ct;
            await _lobbyMaker.Initialize(dialogCreateSession.InfoText, ct);
            await _lobbyMaker.QuickJoinLobby(ct);
            Debug.Log("Lobby Created");
            //ロビーにメンバーが揃うのを待つ
            _lobbyMaker.HandleLobbyPolling(ct).Forget();

            _lobbyMaker.Heartbeat(ct).Forget();
            
            await _lobbyMaker.IsJoinedFullMember(ct);

            await _lobbyMaker.CreateRelay();

            await _sessionMaker.IsAllMemberConnectedRelay();
            dialogCreateSession.InfoText.text = "All Member Connected Relay";
            Debug.Log("All Member Connected Relay");
        }
    }
}