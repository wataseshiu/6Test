using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HUDToDoPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _toDoText;
        [SerializeField] private List<string> _toDoTextList;
        
        //テキストのONOFF
        public void SetToDoTextActive(bool isActive)
        {
            _toDoText.gameObject.SetActive(isActive);
        }

        //テキストのセット
        public void SetToDoText(HUDToDoText hudToDoText)
        {
            _toDoText.text = _toDoTextList[(int)hudToDoText];
        }   
    }

    public enum HUDToDoText
    {
        SelectBattleCard,
        WaitOpponentSelectBattleCard,
        SelectGetCard,
        WaitOpponentSelectGetCard,
        WaitOpponentArrival,
    }
}