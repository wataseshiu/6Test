using UnityEngine;
using UnityEngine.Serialization;

namespace Audio
{
    public class SeManager : MonoBehaviour
    { 
        [SerializeField] private AudioSource[] audioSources;

        public void PlaySe(SeNumber seNumber)
        {
            var audioSource = audioSources[(int)seNumber];
            audioSource.Play();
        }
    }

    public enum SeNumber
    {
        Victory,
        Defeat,
        PhaseChange,
        Shield1,
        Shield2,
        Shield3,
        Sword1,
        Sword2,
        Sword3,
        Magic1,
        Magic2,
        Magic3,
        Decide,
        Cancel,
        Book1,
        Book2,
        Fabric1,
    }
}