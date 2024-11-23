using System.Threading;
using CanvasController;
using Cysharp.Threading.Tasks;
using LobbyManagement;
using UnityEngine;
using VContainer.Unity;
using R3;
using UI;

namespace SceneDirector
{
    public class TitleSceneDirector : IStartable
    {
        private TitleCanvasController _titleCanvasController;
        private QuickSessionDirector _quickSessionDirector;
        private LobbyMaker _lobbyMaker;
        private GameLoopDirector _gameLoopDirector;
        private DialogManager _dialogManager;

        public TitleSceneDirector(TitleCanvasController titleCanvasController, QuickSessionDirector quickSessionDirector, LobbyMaker lobbyMaker, GameLoopDirector gameLoopDirector, DialogManager dialogManager)
        {
            _titleCanvasController = titleCanvasController;
            _quickSessionDirector = quickSessionDirector;
            _lobbyMaker = lobbyMaker;
            _gameLoopDirector = gameLoopDirector;
            _dialogManager = dialogManager;
        } 
        public async void Start()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            _titleCanvasController.TitleClicked.SubscribeAwait( async (x,ct)=>
            {
                Debug.Log("TitleClicked");

                var dialog = _dialogManager.CreateDialog(Dialog.TitleMenu, 0);
                //ダイアログのいずれかのボタンが押されるのを待つ
                var ctsTitleMenu = new CancellationTokenSource();
                var ctTitleMenu = ctsTitleMenu.Token;
                var task1 = dialog.GetComponent<TitleMenuDialog>().OnSoloGameAsObservable().FirstAsync(ctTitleMenu).AsUniTask();
                var task2 = dialog.GetComponent<TitleMenuDialog>().OnMultiPlayGameAsObservable().FirstAsync(ctTitleMenu).AsUniTask();
                var task3 = dialog.GetComponent<TitleMenuDialog>().OnExitAsObservable().FirstAsync(ctTitleMenu).AsUniTask();
                var result = await UniTask.WhenAny(task1, task2, task3);
                ctsTitleMenu.Cancel();

                switch (result.winArgumentIndex)
                {
                    case 0:
                        Debug.Log("SoloPlay");
                        _dialogManager.DestroyDialog(dialog);
                        _gameLoopDirector.Initialize();
                        break;
                    case 1:
                        Debug.Log("MultiPlay");
                        _dialogManager.DestroyDialog(dialog);
                        RunQuickJoinSession();
                        break;
                    case 2:
                        Debug.Log("Exit");
                        _dialogManager.DestroyDialog(dialog);
                        Application.Quit();
                        break;
                }
            }, AwaitOperation.Drop);
            Debug.Log("TitleSceneDirector Start");
        }

        private void RunQuickJoinSession()
        {
            _quickSessionDirector.CreateQuickJoinSession();
        }
    }
}