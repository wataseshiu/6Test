using System.Collections.Generic;
using System.Linq;
using Audio;
using Card;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using MultiPlay;
using R3;
using SceneDirector.GameLoopInstance;
using Timeline;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace SceneDirector
{
    public class GameLoopInstanceShowBattle : IGameLoopInstanceBase
    {
        private TurnDataList _turnDataList;
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private PhaseAnnounceManager _phaseAnnounceManager;
        private HUDLifePresenter _hudLifePresenter;
        private SeManager _seManager;
        private MultiPlayManager _multiPlayManager;

        public GameLoopInstanceShowBattle(TurnDataList turnDataList, IEnumerable<HandManager> handManagers, PhaseAnnounceManager phaseAnnounceManager, HUDLifePresenter hudLifePresenter, SeManager seManager)
        {
            _turnDataList = turnDataList;
            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
            _phaseAnnounceManager = phaseAnnounceManager;
            _hudLifePresenter = hudLifePresenter;
            _seManager = seManager;
        }

        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceShowBattle Initialize");
            return;
        }
        public async UniTask Initialize(MultiPlayManager multiPlayManager)
        {
            _multiPlayManager = multiPlayManager;
            await Initialize();
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceShowBattle Execute");
            //演出部分
            await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.ShowBattle, 0);
            await UniTask.DelayFrame(15);

            var playerSelectCard = _turnDataList.turnDataList.Last().PlayerSelectedCardType;
            var opponentSelectCard = _turnDataList.turnDataList.Last().OpponentSelectedCardType;
            var playerSelectCardIndex = _turnDataList.turnDataList.Last().PlayerSelectedCardIndex;
            var opponentSelectCardIndex = _turnDataList.turnDataList.Last().OpponentSelectedCardIndex;
            Debug.Log($"Player : {playerSelectCard}, {playerSelectCardIndex} vs Opponent : {opponentSelectCard}, {opponentSelectCardIndex}");

            //演出部分
            _seManager.PlaySe(SeNumber.Book1);
            var myCardOpen = _myHandManager.OpenDecidedCards(playerSelectCardIndex);
            await UniTask.DelayFrame(15);
            _seManager.PlaySe(SeNumber.Book2);
            var opponentCardOpen = _opponentHandManager.OpenDecidedCards(opponentSelectCardIndex);
            await UniTask.WhenAll(myCardOpen, opponentCardOpen);
            await UniTask.WaitForSeconds(1);

            var isAttackExecute = playerSelectCard != opponentSelectCard;

            if (isAttackExecute)
            {
                //Playerが勝った場合はWinPlayerが1、Playerが負けた場合はWinPlayerが2
                _turnDataList.WinPlayer = ((playerSelectCard == CardType.Red && opponentSelectCard == CardType.Green) ||
                                           (playerSelectCard == CardType.Green && opponentSelectCard == CardType.Blue) ||
                                           (playerSelectCard == CardType.Blue && opponentSelectCard == CardType.Red)) ? 1 : 2;
                
                //Playerが勝った場合はOpponentのLifeを減らす
                if (_turnDataList.WinPlayer == 1)
                {
                    _seManager.PlaySe(SeNumber.Sword1);
                    var damage = _myHandManager.GetCardAttackParameterFromIndex(playerSelectCardIndex);
                    _hudLifePresenter.RemoveOpponentLife(damage);
                    Debug.Log($"プレイヤー：{playerSelectCard}、相手：{opponentSelectCard}で、プレイヤーの勝ち、相手に{damage}のダメージ");
                }
                //Playerが負けた場合はPlayerのLifeを減らす
                else
                {
                    _seManager.PlaySe(SeNumber.Sword1);
                    var damage = _opponentHandManager.GetCardAttackParameterFromIndex(opponentSelectCardIndex);
                    _hudLifePresenter.RemovePlayerLife(damage);
                    Debug.Log($"プレイヤー：{playerSelectCard}、相手：{opponentSelectCard}で、相手の勝ち、プレイヤーに{damage}のダメージ");
                }
            }
            var isGameEnd = _hudLifePresenter.IsPlayerDead || _hudLifePresenter.IsOpponentDead;
            
            NextState = isGameEnd ? GameState.EndGame : GameState.GetCard;
            
            //演出部分
            
            //ゲームが終わっていないなら使ったカードを削除する
            if (NextState == GameState.GetCard)
            {
                //使ったカードを削除する
                var task = _myHandManager.RemoveCardFromHand(playerSelectCardIndex);
                var task2 = _opponentHandManager.RemoveCardFromHand(opponentSelectCardIndex);

                await UniTask.WhenAll(task, task2);
                await UniTask.DelayFrame(30);
                await _opponentHandManager.OpenAllHands();
            }

            return;
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceShowBattle Terminate");
            if (_multiPlayManager)
            {
                _multiPlayManager.SetupOnTurnStart();
            }
            StateEnd.OnNext(Unit.Default);
            return NextState;
        }
    }
}