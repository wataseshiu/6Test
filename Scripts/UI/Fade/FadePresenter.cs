using Cysharp.Threading.Tasks;

namespace UI.Fade
{
    public class FadePresenter
    {
        private FadeView _fadeView;

        public FadePresenter(FadeView fadeView)
        {
            _fadeView = fadeView;
        }
        
        public async UniTask FadeIn(float duration = 1.0f)
        {
            if (_fadeView.IsFadeOut)
            {
                await _fadeView.FadeIn(duration);
            }
        }

        public async UniTask FadeOut(float duration = 1.0f)
        {
            if (!_fadeView.IsFadeOut)
            {
                await _fadeView.FadeOut(duration);
            }
        }
    }
}