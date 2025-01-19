using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;

namespace UI
{
    public class DialogCreateSession : MonoBehaviour
    {
        [SerializeField]private TextMeshProUGUI infoText;
        public TextMeshProUGUI InfoText => infoText;
        
        [SerializeField]private CustomButton cancelButton;

        public readonly Subject<Unit> CancelCreateSession = new Subject<Unit>();
        public CancellationToken Ct { get; private set; }
        private void Awake()
        {
            Ct = this.GetCancellationTokenOnDestroy();
            cancelButton.OnClickAsObservable().Subscribe(_ =>
            {
                CancelCreateSession.OnNext(Unit.Default);
                Destroy(gameObject);
            }).AddTo(this);
        }
    }
}
