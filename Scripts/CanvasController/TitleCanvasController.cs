using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace CanvasController
{
    public class TitleCanvasController : MonoBehaviour
    {
        public Subject<Unit> TitleClicked { get; } = new Subject<Unit>();

        [SerializeField] private Image titleImage;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //titleImageをクリックしたらtitleClickedを発行
            titleImage.OnPointerClickAsObservable().Subscribe(_ => TitleClicked.OnNext(Unit.Default));
        }
    }
}
