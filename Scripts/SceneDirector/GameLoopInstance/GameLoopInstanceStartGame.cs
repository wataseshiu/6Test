using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using R3;
using Timeline;
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

        public GameLoopInstanceStartGame(PhaseAnnounceManager phaseAnnounceManager, TurnDataList turnDataList, IEnumerable<HandManager> handManagers)
        {
            _phaseAnnounceManager = phaseAnnounceManager;
            _turnDataList = turnDataList;
            
            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
        }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceStartGame Initialize");
            _turnDataList.turnDataList.Clear();
            _myHandManager.ResetHandPosition();
            _opponentHandManager.ResetHandPosition();
            await UniTask.WaitForSeconds(1); 
            return;
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceStartGame Execute");
            await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.StartGame, 0);
            NextState = GameState.SelectCard;
            return;
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceStartGame Terminate");
            await UniTask.WaitForSeconds(1); 
            StateEnd.OnNext(Unit.Default);
            return NextState;
        }
    }
}