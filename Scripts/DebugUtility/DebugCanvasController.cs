using System;
using R3;
using TMPro;
using UI;
using UnityEngine;

namespace DebugUtility
{
    public class DebugCanvasController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI frameRateText;
        [SerializeField] private CustomButton timeScaleButton;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
#if DEVELOPMNT_BUILD || UNITY_EDITOR
            //１秒に１回フレームレート表示を更新する
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => UpdateFrameRateText());
            
            //timeScaleButtonをクリックするたびにtimeScaleがx2x4x1の間を切り替える
            timeScaleButton.OnClickAsObservable().Subscribe(_ => { ChangeTimeScale(); });
#endif
        }

        private void ChangeTimeScale()
        {
            switch (Time.timeScale)
            {
                case 1.0f:
                    Time.timeScale = 2.0f;
                    timeScaleButton.ButtonText.text = "x2";
                    break;
                case 2.0f:
                    Time.timeScale = 4.0f;
                    timeScaleButton.ButtonText.text = "x4";
                    break;
                case 4.0f:
                    Time.timeScale = 1.0f;
                    timeScaleButton.ButtonText.text = "x1";
                    break;
            }
        }

        private void UpdateFrameRateText()
        {
            frameRateText.text = $"{Mathf.RoundToInt(1.0f / Time.deltaTime)}FPS";
        }
    }
}
