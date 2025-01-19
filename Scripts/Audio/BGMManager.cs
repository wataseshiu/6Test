using System;
using UnityEngine;

namespace Audio
{
    public class BGMManager : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;

        private void Start()
        {
            PlayBGM();
        }

        public void PlayBGM()
        {
            audioSource.Play();
        }
        
        public void StopBGM()
        {
            audioSource.Stop();
        }
        
        public void PauseBGM()
        {
            audioSource.Pause();
        }
        
        public void ResumeBGM()
        {
            audioSource.UnPause();
        }

        //BGMのクロスフェード切り替え
        public void CrossFadeBGM(AudioClip clip, float duration)
        {
        }
        
        public void SwitchBGM(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
