using R3;
using R3.Triggers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasController
{
    public class TitleCanvasController : MonoBehaviour
    {
        public Subject<Unit> TitleClicked { get; } = new Subject<Unit>();

        [SerializeField] private Image titleImage;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private CustomButton volumeDialogButton;
        [SerializeField] private DialogManager dialogManager;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //titleImageをクリックしたらtitleClickedを発行
            titleImage.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Debug.Log("TitleImage Clicked");
                TitleClicked.OnNext(Unit.Default);
            });
            
            //volumeDialogButtonをクリックしたらダイアログ表示
            volumeDialogButton.OnClickAsObservable().Subscribe(_ =>
            {
                dialogManager.CreateDialog( Dialog.ConfigVolume, 0);
            });

            versionText.text = $"Version: {Application.version}";
        }

        public void DeactivateTitleCanvas()
        {
            gameObject.SetActive(false);
        }
        public void ActivateTitleCanvas()
        {
            gameObject.SetActive(true);
        }
    }
}
