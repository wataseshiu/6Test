using System.Collections.Generic;
using System.Text.RegularExpressions;
using Audio;
using Cysharp.Threading.Tasks;
using GameData;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace Timeline
{
    public class PhaseAnnounceManager : MonoBehaviour
    {
        [SerializeField] List<GameObject> phaseAnnounceObjects = new List<GameObject>();
        private SeManager _seManager;
        
        [Inject]
        public void Construct(SeManager seManager)
        {
            _seManager = seManager;
        }
        
        public async UniTask ShowPhaseAnnounce(Phase phase, int turnCount)
        {
            int index = (int) phase;
            var phaseObject = Instantiate(phaseAnnounceObjects[index], transform);
            
            var director = phaseObject.GetComponent<PlayableDirector>();

            var text = GetComponentInChildren<PhaseAnnounceObject>().phaseText;
            SeNumber seNumber = SeNumber.PhaseChange;
            if (phase == Phase.TurnStart)
            {
                //{turn}の部分を置換する
                text.text = Regex.Replace( text.text, "{turn}", turnCount.ToString());
            }
            else if(phase is Phase.Win or Phase.Lose)
            {
                text.text = phase == Phase.Win ? "You Win" : "You Lose";
                seNumber = phase == Phase.Win ? SeNumber.Victory : SeNumber.Defeat;
            }
            _seManager.PlaySe(seNumber);

            //phaseObjectのTimelineの再生終了を待つ
            await director.WaitForEnd();
            
            //Timelineの再生終了後にphaseObjectを破棄する
            Destroy(phaseObject);
        }
    }
    
    public enum Phase
    {
        StartGame,
        TurnStart,
        GetCard,
        SelectCard,
        ShowBattle,
        Win,
        Lose,
    }
}