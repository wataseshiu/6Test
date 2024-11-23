using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using GameData;
using R3;
using Timeline;
using UI;
using UnityEngine;

namespace SceneDirector.GameLoopInstance
{
    public class GameLoopInstanceEndGame : IGameLoopInstanceBase
    {
        private TurnDataList _turnDataList;
        private PhaseAnnounceManager _phaseAnnounceManager;
        private DialogManager _dialogManager;

        public GameLoopInstanceEndGame(TurnDataList turnDataList, PhaseAnnounceManager phaseAnnounceManager, DialogManager dialogManager)
        {
            _turnDataList = turnDataList;
            _phaseAnnounceManager = phaseAnnounceManager;
            _dialogManager = dialogManager;
        }

        private int WinPlayer => _turnDataList.WinPlayer;
        
        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }

        public async UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceEndGame Initialize");
            await UniTask.WaitForSeconds(1);
            return;
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceEndGame Execute");
            Debug.Log(WinPlayer == 1 ? "Player Win" : "Opponent Win");
            
            //勝者のアナウンスを表示
            await _phaseAnnounceManager.ShowPhaseAnnounce(WinPlayer == 1 ? Phase.Win : Phase.Lose, 0);
            
            await UniTask.WaitForSeconds(3);
            
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
            
            //ダイアログ削除
            _dialogManager.DestroyDialog(dialog);
            
            //完了したタスクに応じた処理を実行
            //task1かtask2が完了した場合は次のゲームを開始
            NextState = result.winArgumentIndex is 0 or 1 ? GameState.StartGame : GameState.QuitGameLoop;
            return;
        }

        public async UniTask<GameState> Terminate()
        {
            Debug.Log("GameLoopInstanceEndGame Terminate");
            await UniTask.WaitForSeconds(1);
            StateEnd.OnNext(Unit.Default);
            Debug.Log(NextState);
            return NextState;
        }
    }
}