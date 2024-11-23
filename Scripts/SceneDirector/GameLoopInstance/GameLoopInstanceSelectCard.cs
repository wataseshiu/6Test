using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Card;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using R3;
using Timeline;
using UnityEngine;

namespace SceneDirector.GameLoopInstance
{
    public class GameLoopInstanceSelectCard : IGameLoopInstanceBase
    {
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private PhaseAnnounceManager _phaseAnnounceManager;
        private TurnDataList _turnDataList;

        public GameLoopInstanceSelectCard( IEnumerable<HandManager> handManagers, PhaseAnnounceManager phaseAnnounceManager, TurnDataList turnDataList)
        {
            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
            _phaseAnnounceManager = phaseAnnounceManager;
            _turnDataList = turnDataList;
        }

        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceSelectCard Initialize");
            _myHandManager.Initialize();
            _opponentHandManager.Initialize();
            
            await UniTask.WaitForSeconds(1);
            return;
        }

        async UniTask IGameLoopInstanceBase.Execute()
        {
            await UniTask.WaitForSeconds(1);
            return;            
        }
        public async UniTask<(CardType,CardType, int, int)> Execute()
        {
            Debug.Log("GameLoopInstanceSelectCard Execute");
            //2ターン目以降の場合はターン開始のアナウンスを表示
            if (_turnDataList.turnDataList.Count > 0)
            {
                await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.TurnStart, _turnDataList.turnDataList.Count + 1);
                await UniTask.DelayFrame(15);
            }
            await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.SelectCard, 0);

            var myCard = await _myHandManager.WaitCardSelect();
            var opponentCard = await _opponentHandManager.WaitOpponentCardSelect();
            
            await _myHandManager.MoveCardToMyTarget(myCard.Item2);
            await _opponentHandManager.MoveCardToOpponentTarget(opponentCard.Item2);
            
            NextState = GameState.ShowBattle;
            return (myCard.Item1, opponentCard.Item1, myCard.Item3, opponentCard.Item3);
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceSelectCard Terminate");
            await UniTask.WaitForSeconds(1);
            StateEnd.OnNext(Unit.Default);
            return NextState;
        }
    }
}