using System;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    public class DialogConfigVolume : MonoBehaviour
    {
        [SerializeField] private VolumeControllerView volumeControllerViewBGM;
        [SerializeField] private VolumeControllerView volumeControllerViewSE;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Image closeButton;

        private void Awake()
        {
            closeButton.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Destroy(gameObject);
            }).AddTo(this);
            volumeControllerViewBGM.OnVolumeChangedSubject.Subscribe(volume =>
            {
                audioMixer.SetFloat("BGMVolume", Mathf.Clamp(Mathf.Log10(volume) * 20f, -80f, 0f));
            }).AddTo(this);
            volumeControllerViewSE.OnVolumeChangedSubject.Subscribe(volume =>
            {
                audioMixer.SetFloat("SEVolume", Mathf.Clamp(Mathf.Log10(volume) * 20f, -80f, 0f));
            }).AddTo(this);
        }
    }
}
