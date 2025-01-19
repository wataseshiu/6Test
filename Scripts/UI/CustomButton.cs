using System;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CustomButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler,IPointerUpHandler
    {
        public Image BackgroundImage { get; private set; }

        public TextMeshProUGUI ButtonText => buttonText;
        [SerializeField] private TextMeshProUGUI buttonText;

        [SerializeField] private bool isInteractable = true;

        private readonly Subject<Unit> _onClickSubject = new();
        private readonly Subject<Unit> _onPointerDownSubject = new();
        private readonly Subject<Unit> _onPointerUpSubject = new();

        private float debaunceMilliseconds = 500;

        void Awake()
        {
            BackgroundImage = GetComponentInChildren<Image>();
        }

        //Observableの公開メソッド
        public Observable<Unit> OnClickAsObservable() => _onClickSubject.AsObservable().ThrottleFirst(TimeSpan.FromMilliseconds(debaunceMilliseconds));
        public Observable<Unit> OnPointerDownAsObservable() => _onPointerDownSubject.AsObservable();
        public Observable<Unit> OnPointerUpAsObservable() => _onPointerUpSubject.AsObservable();
    
        //インターフェースの実装
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) { return; }
            Debug.Log("OnPointerClick");
            _onClickSubject.OnNext(Unit.Default);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable) { return; }
            Debug.Log("OnPointerDown");
            BackgroundImage.transform.DOScale( 0.9f, 0.1f);
            _onPointerDownSubject.OnNext(Unit.Default);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInteractable) { return; }
            Debug.Log("OnPointerUp");
            BackgroundImage.transform.DOScale( 1.0f, 0.1f);
            _onPointerUpSubject.OnNext(Unit.Default);
        }
    }
}
