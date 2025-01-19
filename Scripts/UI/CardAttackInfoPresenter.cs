using System;
using Card;
using TMPro;
using UnityEngine;

namespace UI
{
    public class CardAttackInfoPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI attackPointText;

        private void Awake()
        {
            SetAttackPoint(transform.parent.GetComponent<CardParameter>().CardAttackPoint);
        }

        public void SetAttackPoint(int attackPoint)
        {
            attackPointText.text = attackPoint.ToString();
        }
    }
}
