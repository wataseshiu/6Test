using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audio;
using CanvasController;
using Cysharp.Threading.Tasks;
using LobbyManagement;
using MultiPlay;
using UnityEngine;
using VContainer.Unity;
using R3;
using UI;
using UI.Fade;
using Unity.Netcode;

namespace SceneDirector
{
    public class TitleSceneDirector : IStartable
    {
        private TitleCanvasController _titleCanvasController;
        private QuickSessionDirector _quickSessionDirector;
        private LobbyMaker _lobbyMaker;
        private GameLoopDirector _gameLoopDirector;
        private DialogManager _dialogManager;
        private SeManager _seManager;
        private FadePresenter _fadePresenter;
        private CustomNetworkManager _customNetworkManager;

        public TitleSceneDirector(TitleCanvasController titleCanvasController, QuickSessionDirector quickSessionDirector, LobbyMaker lobbyMaker, GameLoopDirector gameLoopDirector, DialogManager dialogManager, SeManager seManager, FadePresenter fadePresenter, CustomNetworkManager customNetworkManager)
        {
            _titleCanvasController = titleCanvasController;
            _quickSessionDirector = quickSessionDirector;
            _lobbyMaker = lobbyMaker;
            _gameLoopDirector = gameLoopDirector;
            _dialogManager = dialogManager;
            _seManager = seManager;
            _fadePresenter = fadePresenter;
            _customNetworkManager = customNetworkManager;
        } 
        public async void Start()
        {
            Debug.Log("TitleSceneDirector Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            _titleCanvasController.TitleClicked.SubscribeAwait( async (x,ct)=>
            {
                Debug.Log("TitleClicked");
                _seManager.PlaySe(SeNumber.Decide);

                var dialog = _dialogManager.CreateDialog(Dialog.TitleMenu, 0).GetComponent<TitleMenuDialog>();
                
                Debug.Log("await SelectMenu");
                
                //GetSelectMenuをSubscribeAwaitで待つ
                dialog.OnSelectMenuAsObservable().Subscribe(async x =>
                {
                    await GetSelectMenu(x, dialog, ct);
                }).AddTo(dialog);

                Debug.Log("GetSelectMenuAsync");
                await dialog.GetSelectMenuAsync(ct);
            }, AwaitOperation.Drop);
        }

        private async UniTask GetSelectMenu(int result, TitleMenuDialog titleMenuDialog, CancellationToken ct)
        {
            _seManager.PlaySe(SeNumber.Decide);
            switch (result)
            {
                case 0:
                    Debug.Log("SoloPlay");
                    await ExecuteSoloPlay(titleMenuDialog, ct);
                    break;
                case 1:
                    Debug.Log("MultiPlay");
                    await ExecuteMultiPlay(titleMenuDialog, ct);
                    break;
                case 2:
                    Debug.Log("Exit");
                    _dialogManager.DestroyDialog(titleMenuDialog.gameObject);
                    Application.Quit();
                    break;
            }
        }

        private async UniTask ExecuteMultiPlay(TitleMenuDialog titleMenuDialog, CancellationToken ct)
        {
            await RunQuickJoinSession(titleMenuDialog.gameObject, ct);
            var multiPlayManager = GameObject.FindGameObjectWithTag("Player").GetComponent<MultiPlayManager>();
            multiPlayManager.Initialize();
            await _fadePresenter.FadeOut(1.0f);
            _titleCanvasController.DeactivateTitleCanvas();
            _dialogManager.DestroyDialog(titleMenuDialog.gameObject);
            _gameLoopDirector.Initialize(multiPlayManager);
//                        _gameLoopDirector.Initialize();
            // _gameLoopDirectorのGameEndが発行されるまで待つ
            await _gameLoopDirector.GameEnd.FirstAsync(ct);
            _titleCanvasController.ActivateTitleCanvas();
            await _fadePresenter.FadeIn(1.0f);
        }

        private async UniTask ExecuteSoloPlay(TitleMenuDialog titleMenuDialog, CancellationToken ct)
        {
            await _fadePresenter.FadeOut(1.0f);
            _titleCanvasController.DeactivateTitleCanvas();
            _dialogManager.DestroyDialog(titleMenuDialog.gameObject);
            _gameLoopDirector.Initialize();
            // _gameLoopDirectorのGameEndが発行されるまで待つ
            await _gameLoopDirector.GameEnd.FirstAsync(ct);
            _titleCanvasController.ActivateTitleCanvas();
            await _fadePresenter.FadeIn(1.0f);
        }

        private async UniTask RunQuickJoinSession(GameObject dialog, CancellationToken ct)
        {
            var dialogCreateSession = _dialogManager.CreateDialog( Dialog.CreateSession).GetComponent<DialogCreateSession>();
            await _quickSessionDirector.CreateQuickJoinSession(dialogCreateSession);
            _dialogManager.DestroyDialog(dialogCreateSession.gameObject);
        }
    }
}