using System.Collections.Generic;
using System.Linq;
using Card;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using R3;
using SceneDirector.GameLoopInstance;
using Timeline;
using UnityEngine;

namespace SceneDirector
{
    public class GameLoopInstanceShowBattle : IGameLoopInstanceBase
    {
        private TurnDataList _turnDataList;
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private PhaseAnnounceManager _phaseAnnounceManager;
        
        public GameLoopInstanceShowBattle(TurnDataList turnDataList, IEnumerable<HandManager> handManagers, PhaseAnnounceManager phaseAnnounceManager)
        {
            _turnDataList = turnDataList;
            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
            _phaseAnnounceManager = phaseAnnounceManager;
        }

        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceShowBattle Initialize");
            await UniTask.WaitForSeconds(1); 
            return;
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceShowBattle Execute");

            var playerSelectCard = _turnDataList.turnDataList.Last().PlayerSelectedCardType;
            var opponentSelectCard = _turnDataList.turnDataList.Last().OpponentSelectedCardType;
            var playerSelectCardIndex = _turnDataList.turnDataList.Last().PlayerSelectedCardIndex;
            var opponentSelectCardIndex = _turnDataList.turnDataList.Last().OpponentSelectedCardIndex;
            Debug.Log($"Player : {playerSelectCard}, {playerSelectCardIndex} vs Opponent : {opponentSelectCard}, {opponentSelectCardIndex}");

            var isBattleEnd = playerSelectCard != opponentSelectCard;

            if (isBattleEnd)
            {
                //Playerが勝った場合はWinPlayerが1、Playerが負けた場合はWinPlayerが2
                _turnDataList.WinPlayer = ((playerSelectCard == CardType.Red && opponentSelectCard == CardType.Green) ||
                                           (playerSelectCard == CardType.Green && opponentSelectCard == CardType.Blue) ||
                                           (playerSelectCard == CardType.Blue && opponentSelectCard == CardType.Red)) ? 1 : 2;
            }
            
            NextState = isBattleEnd ? GameState.EndGame : GameState.SelectCard;
            
            //演出部分
            await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.ShowBattle, 0);
            await UniTask.DelayFrame(15);

            var myCardOpen = _myHandManager.OpenDecidedCards(playerSelectCardIndex);
            await UniTask.DelayFrame(15);
            var opponentCardOpen = _opponentHandManager.OpenDecidedCards(opponentSelectCardIndex);
            
            await UniTask.WhenAll(myCardOpen, opponentCardOpen);
            
            await UniTask.WaitForSeconds(1);
            
            //ゲームが終わっていないならカードを元の位置に移動する
            if (NextState == GameState.SelectCard)
            {
                var myCardTakeBack = _myHandManager.TakeBackCardToHand(playerSelectCardIndex);
                await UniTask.DelayFrame(15);
                var opponentCardTakeBack = _opponentHandManager.TakeBackCardToHand(opponentSelectCardIndex);
                await UniTask.WhenAll(myCardTakeBack, opponentCardTakeBack);
            }

            return;
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceShowBattle Terminate");
            await UniTask.WaitForSeconds(1); 
            StateEnd.OnNext(Unit.Default);
            return NextState;
        }
    }
}