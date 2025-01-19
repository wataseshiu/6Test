using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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
    public class GameLoopInstanceEndGame : IGameLoopInstanceBase
    {
        private TurnDataList _turnDataList;
        private PhaseAnnounceManager _phaseAnnounceManager;
        private DialogManager _dialogManager;
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private HUDLifePresenter _hudLifePresenter;
        private FadePresenter _fadePresenter;
        private MultiPlayManager _localMultiPlayManager;

        public GameLoopInstanceEndGame(TurnDataList turnDataList, PhaseAnnounceManager phaseAnnounceManager, DialogManager dialogManager, IEnumerable<HandManager> handManagers, HUDLifePresenter hudLifePresenter, FadePresenter fadePresenter)
        {
            _turnDataList = turnDataList;
            _phaseAnnounceManager = phaseAnnounceManager;
            _dialogManager = dialogManager;
            _hudLifePresenter = hudLifePresenter;
            _fadePresenter = fadePresenter;

            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
        }

        private int WinPlayer => _turnDataList.WinPlayer;
        
        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceEndGame Initialize");
            return;
        }
        public async UniTask Initialize(MultiPlayManager multiPlayManager)
        {
            _localMultiPlayManager = multiPlayManager;
            await Initialize();
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceEndGame Execute");
            Debug.Log(WinPlayer == 1 ? "Player Win" : "Opponent Win");
            
            //勝者のアナウンスを表示
            await _phaseAnnounceManager.ShowPhaseAnnounce(WinPlayer == 1 ? Phase.Win : Phase.Lose, 0);
            
            await UniTask.WaitForSeconds(1);
            //残りの相手のカードを表向きにする
            await _opponentHandManager.OpenAllHands();
            await UniTask.WaitForSeconds(2);
            
            //リザルトダイアログを表示
            var isWinPlayer = WinPlayer == 1;
            var dialog = _dialogManager.CreateDialog(Dialog.Result, isWinPlayer);

            //ダイアログのいずれかのボタンが押されるのを待つ
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            var task1 = dialog.GetComponent<ResultDialog>().OnNextAsObservable().FirstAsync(ct).AsUniTask();
            var task2 = dialog.GetComponent<ResultDialog>().OnRetryAsObservable().FirstAsync(ct).AsUniTask();
            var task3 = dialog.GetComponent<ResultDialog>().OnExitAsObservable().FirstAsync(ct).AsUniTask();
            var result = await UniTask.WhenAny(task1, task2, task3);
            cts.Cancel();
            
            await _fadePresenter.FadeOut(1.0f);
            
            //ダイアログ削除
            _dialogManager.DestroyDialog(dialog);
            
            //完了したタスクに応じた処理を実行
            //task1かtask2が完了した場合は次のゲームを開始
            NextState = result.winArgumentIndex is 0 or 1 ? GameState.StartGame : GameState.QuitGameLoop;

            if (NextState == GameState.StartGame)
            {
                //カードを元の位置に戻す
                var myCardTakeBack = _myHandManager.TakeBackCardToHand(_turnDataList.turnDataList.Last().PlayerSelectedCardIndex);
                await UniTask.DelayFrame(15);
                var opponentCardTakeBack = _opponentHandManager.TakeBackCardToHand(_turnDataList.turnDataList.Last().OpponentSelectedCardIndex);
                await UniTask.WhenAll(myCardTakeBack, opponentCardTakeBack);
                await UniTask.DelayFrame(15);
            }
            return;
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceEndGame Terminate");
            _hudLifePresenter.Reset();

            _myHandManager.DeleteHand();
            _opponentHandManager.DeleteHand();
            await UniTask.Yield();
            
            StateEnd.OnNext(Unit.Default);
            Debug.Log(NextState);
            return NextState;
        }
    }
}