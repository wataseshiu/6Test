using System;
using System.Collections.Generic;
using System.Linq;
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
        
        //Observableの公開メソッド
        public Observable<Unit> OnSoloGameAsObservable() => _soloPlaySubject.AsObservable();
        public Observable<Unit> OnMultiPlayGameAsObservable() => _multiPlaySubject.AsObservable();
        public Observable<Unit> OnExitAsObservable() => _exitSubject.AsObservable();
        
        void Awake()
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

        public void Dispose()
        {
            Destroy(this);
        }

    }
}
