using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace UI
{
    public class TitleMenuDialog : MonoBehaviour, IDialogObjectBase<int>, IDisposable
    {
        public int param { get; set; }
        [SerializeField]private List<CustomButton> buttons = new List<CustomButton>();
        
        //subject
        private readonly Subject<Unit> _soloPlaySubject = new();
        private readonly Subject<Unit> _multiPlaySubject = new();
        private readonly Subject<Unit> _exitSubject = new();
        
        private readonly Subject<int> _selectMenuSubject = new();
        
        //Observableの公開メソッド
        public Observable<Unit> OnSoloGameAsObservable() => _soloPlaySubject.AsObservable();
        public Observable<Unit> OnMultiPlayGameAsObservable() => _multiPlaySubject.AsObservable();
        public Observable<Unit> OnExitAsObservable() => _exitSubject.AsObservable();
        
        public Observable<int> OnSelectMenuAsObservable() => _selectMenuSubject.AsObservable();

        async void Awake()
        {
            buttons = GetComponentsInChildren<CustomButton>().ToList();
            
            // ボタンのイベント購読
            buttons[0].OnClickAsObservable().Subscribe(_ =>
            {
                Debug.Log("SoloPLay");
                _soloPlaySubject.OnNext(Unit.Default);
            }).AddTo(this);
            
            buttons[1].OnClickAsObservable().Subscribe(_ =>
            {
                Debug.Log("MultiPlay");
                _multiPlaySubject.OnNext(Unit.Default);
            }).AddTo(this);
            
            buttons[2].OnClickAsObservable().Subscribe(_ =>
            {
                Debug.Log("Exit");
                _exitSubject.OnNext(Unit.Default);
            }).AddTo(this);
        }

        public async UniTask GetSelectMenuAsync(CancellationToken ct)
        {
            //ダイアログのいずれかのボタンが押されるのを待つ
            var ctsTitleMenu = new CancellationTokenSource();
            var ctTitleMenu = ctsTitleMenu.Token;
            var task1 = OnSoloGameAsObservable().FirstAsync(ctTitleMenu).AsUniTask();
            var task2 = OnMultiPlayGameAsObservable().FirstAsync(ctTitleMenu).AsUniTask();
            var task3 = OnExitAsObservable().FirstAsync(ctTitleMenu).AsUniTask();
            var result = await UniTask.WhenAny(task1, task2, task3);
            ctsTitleMenu.Cancel();

            // どのボタンが押されたかをイベントで通知
            _selectMenuSubject.OnNext(result.winArgumentIndex);
        }

        public void Dispose()
        {
            Destroy(this);
        }

    }
}
