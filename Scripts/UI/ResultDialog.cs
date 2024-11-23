using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace UI
{
    public class ResultDialog : MonoBehaviour, IDialogObjectBase<bool>, IDisposable
    {
        public bool param { get; set; }
        [SerializeField]private List<CustomButton> buttons = new List<CustomButton>();
        
        //subject
        private readonly Subject<Unit> _nextSubject = new();
        private readonly Subject<Unit> _retrySubject = new();
        private readonly Subject<Unit> _exitSubject = new();
        
        //Observableの公開メソッド
        public Observable<Unit> OnNextAsObservable() => _nextSubject.AsObservable();
        public Observable<Unit> OnRetryAsObservable() => _retrySubject.AsObservable();
        public Observable<Unit> OnExitAsObservable() => _exitSubject.AsObservable();
        
        void Awake()
        {
            buttons = GetComponentsInChildren<CustomButton>().ToList();
            
            if(param)
            {
                buttons[1].gameObject.SetActive(false);
            }
            else
            {
                buttons[0].gameObject.SetActive(false);
            }
            
            // ボタンのイベント購読
            buttons[0].OnClickAsObservable().Subscribe(_ =>
            {
                Debug.Log("Next");
                _nextSubject.OnNext(Unit.Default);
            }).AddTo(this);
            
            buttons[1].OnClickAsObservable().Subscribe(_ =>
            {
                Debug.Log("Retry");
                _retrySubject.OnNext(Unit.Default);
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
