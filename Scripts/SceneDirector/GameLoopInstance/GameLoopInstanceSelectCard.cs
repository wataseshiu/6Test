using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Card;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using MultiPlay;
using R3;
using Timeline;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace SceneDirector.GameLoopInstance
{
    public class GameLoopInstanceSelectCard : IGameLoopInstanceBase
    {
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private PhaseAnnounceManager _phaseAnnounceManager;
        private TurnDataList _turnDataList;
        private SeManager _seManager;
        private MultiPlayManager _multiPlayManager;
        private HUDToDoPresenter _hudToDoPresenter;

        public GameLoopInstanceSelectCard( IEnumerable<HandManager> handManagers, PhaseAnnounceManager phaseAnnounceManager, TurnDataList turnDataList, SeManager seManager, HUDToDoPresenter hudToDoPresenter)
        {
            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
            _phaseAnnounceManager = phaseAnnounceManager;
            _turnDataList = turnDataList;
            _seManager = seManager;
            _hudToDoPresenter = hudToDoPresenter;
        }

        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceSelectCard Initialize");
            _myHandManager.Initialize();
            _opponentHandManager.Initialize();

            return;
        }
        public async UniTask Initialize(MultiPlayManager multiPlayManager)
        {
            _multiPlayManager = multiPlayManager;
            await Initialize();
        }

        async UniTask IGameLoopInstanceBase.Execute()
        {
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
            
            //HUD表示
            _hudToDoPresenter.SetToDoTextActive(true);
            _hudToDoPresenter.SetToDoText(HUDToDoText.SelectBattleCard);

            var myCard = (CardType.Red, new RectTransform(), 0);
            var opponentCard = (CardType.Red, new RectTransform(), 0);

            //マルチプレイ時
            if (_multiPlayManager != null)
            {
                var myTask = _myHandManager.WaitCardSelect(_multiPlayManager);
                var opponentTask = _opponentHandManager.WaitOpponentCardSelect(_multiPlayManager);
                var resultMyTask = await myTask;
                if (opponentTask.Status.IsCompleted() == false) {
                    _hudToDoPresenter.SetToDoText(HUDToDoText.WaitOpponentSelectBattleCard);
                }
                var resultOpponentTask = await opponentTask;
                
                myCard = resultMyTask;
                _seManager.PlaySe(SeNumber.Decide);
                opponentCard = resultOpponentTask;
                Debug.Log("対戦相手が選択したカード : " + opponentCard.Item1);

                await _opponentHandManager.FlipAllCard();
//                await _opponentHandManager.ShuffleHandPosition();
            }
            else
            {
                myCard = await _myHandManager.WaitCardSelect();
                _seManager.PlaySe(SeNumber.Decide);
                opponentCard = await _opponentHandManager.WaitOpponentCardSelect();
            }
            
            _hudToDoPresenter.SetToDoTextActive(false);

            _seManager.PlaySe(SeNumber.Fabric1);
            await _myHandManager.MoveCardToMyTarget(myCard.Item2);
            _seManager.PlaySe(SeNumber.Fabric1);
            await _opponentHandManager.MoveCardToOpponentTarget(opponentCard.Item2);
            
            NextState = GameState.ShowBattle;
            
            //todo カードのindexがシャッフル後の並び順になっていない
            return (myCard.Item1, opponentCard.Item1, myCard.Item3, opponentCard.Item3);
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceSelectCard Terminate");
            StateEnd.OnNext(Unit.Default);
            return NextState;
        }
    }
}