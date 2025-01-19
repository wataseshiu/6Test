using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using MultiPlay;
using R3;
using Timeline;
using UI;
using UI.Fade;
using Unity.Netcode;
using UnityEngine;

namespace SceneDirector.GameLoopInstance
{
    public class GameLoopInstanceStartGame : IGameLoopInstanceBase
    {
        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        private PhaseAnnounceManager _phaseAnnounceManager;
        private TurnDataList _turnDataList;
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private HUDLifePresenter _hudLifePresenter;
        private FadePresenter _fadePresenter;

        private MultiPlayManager _localPlayerNetworkObject;
        
        public GameLoopInstanceStartGame(PhaseAnnounceManager phaseAnnounceManager, TurnDataList turnDataList, IEnumerable<HandManager> handManagers, HUDLifePresenter hudLifePresenter, FadePresenter fadePresenter)
        {
            _phaseAnnounceManager = phaseAnnounceManager;
            _turnDataList = turnDataList;
            _hudLifePresenter = hudLifePresenter;
            _fadePresenter = fadePresenter;

            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
        }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceStartGame Initialize");
            
            _turnDataList.turnDataList.Clear();
            await _myHandManager.CreateHand();
            await _opponentHandManager.CreateHand();
            
            _myHandManager.ResetHandPosition();
            _opponentHandManager.ResetHandPosition();

            _myHandManager.Initialize();
            _opponentHandManager.Initialize();
            
            _hudLifePresenter.Initialize();
            
            await _fadePresenter.FadeIn(1.0f);
            return;
        }
        public async UniTask Initialize(MultiPlayManager multiPlayManager)
        {
            _localPlayerNetworkObject = multiPlayManager;
            await Initialize();
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceStartGame Execute");
            await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.StartGame, 0);
            //todo カード入手フェーズの開発のためにいきなりカード入手フェーズに遷移させる
//            NextState = GameState.GetCard;
            NextState = GameState.SelectCard;
            return;
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceStartGame Terminate");
            StateEnd.OnNext(Unit.Default);
            return NextState;
        }
    }
}