using CanvasController;
using Cysharp.Threading.Tasks;
using LobbyManagement;
using Session;
using UnityEngine;
using VContainer.Unity;
using VContainer;

namespace SceneDirector
{
    public class QuickSessionDirector
    {
        private QuickSessionCanvasController _quickSessionCanvasController;
        private LobbyMaker _lobbyMaker;
        private SessionMaker _sessionMaker;

        public QuickSessionDirector(QuickSessionCanvasController quickSessionCanvasController, LobbyMaker lobbyMaker, SessionMaker sessionMaker)
        {
            this._quickSessionCanvasController = quickSessionCanvasController;
            this._lobbyMaker = lobbyMaker;
            this._sessionMaker = sessionMaker;
        }

        public async void CreateQuickJoinSession()
        {
            _quickSessionCanvasController.ShowSessionDialog();
            await _lobbyMaker.Initialize();
            await _lobbyMaker.QuickJoinLobby();
            Debug.Log("Lobby Created");
            //ロビーにメンバーが揃うのを待つ
            _lobbyMaker.HandleLobbyPolling().Forget();

            _lobbyMaker.Heartbeat().Forget();
            
            await _lobbyMaker.IsJoinedFullMember();

            await _lobbyMaker.CreateRelay();

            await _sessionMaker.IsAllMemberConnectedRelay();
            Debug.Log("All Member Connected Relay");
        }
    }
}