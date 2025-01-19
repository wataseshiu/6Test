using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Fade
{
    public class FadeView : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        public bool IsFadeOut { get; private set; } = false;
        public async UniTask FadeIn(float duration = 1.0f)
        {
            await fadeImage.DOFade(0, duration).AsyncWaitForCompletion();
            IsFadeOut = false;
        }

        public async UniTask FadeOut(float duration = 1.0f)
        {
            await fadeImage.DOFade(1, duration).AsyncWaitForCompletion();
            IsFadeOut = true;
        }
    }
}
