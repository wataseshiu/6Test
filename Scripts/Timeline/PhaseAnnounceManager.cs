using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using GameData;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline
{
    public class PhaseAnnounceManager : MonoBehaviour
    {
        [SerializeField] List<GameObject> phaseAnnounceObjects = new List<GameObject>();
        public async UniTask ShowPhaseAnnounce(Phase phase, int turnCount)
        {
            int index = (int) phase;
            var phaseObject = Instantiate(phaseAnnounceObjects[index], transform);
            
            var director = phaseObject.GetComponent<PlayableDirector>();

            var text = GetComponentInChildren<PhaseAnnounceObject>().phaseText;
            if (phase == Phase.TurnStart)
            {
                //{turn}の部分を置換する
                text.text = Regex.Replace( text.text, "{turn}", turnCount.ToString());
            }
            else if(phase is Phase.Win or Phase.Lose)
            {
                text.text = phase == Phase.Win ? "You Win" : "You Lose";
            }

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
        SelectCard,
        ShowBattle,
        Win,
        Lose,
    }
}