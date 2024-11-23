using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Card
{
    public class CardDataSetter : MonoBehaviour
    {
        [SerializeField] private int cardAttackPoint = 1;
        [SerializeField] private Color cardColor = Color.red;
        [SerializeField] private TextMeshProUGUI attackPointText;
        [SerializeField] private Image cardMainImage;
        [SerializeField] private Image cardImageBack;
        [SerializeField] private Image cardFrameImage;
        [SerializeField] private Image cardBackgroundImage;
        
        public Image CardImage => cardMainImage;
        public Image CardImageBack => cardImageBack;
        public TextMeshProUGUI AttackPointText => attackPointText;
        
        [field: SerializeField] public CardType CardType { get; private set; }

        public void SetCardData(CardData cardData)
        {
            cardAttackPoint = cardData.AttackPoint;
            cardColor = cardData.Color;
            attackPointText.text = cardAttackPoint.ToString();
            cardMainImage.color = cardColor;
            CardType = cardData.CardType;
            cardMainImage.sprite = cardData.MainImage;
            cardImageBack.sprite = cardData.BackgroundImage;
            cardFrameImage.sprite = cardData.FrameImage;
            cardBackgroundImage.sprite = cardData.BackgroundImage;
        }
    }


}
