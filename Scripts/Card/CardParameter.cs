using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Card
{
    public class CardParameter : MonoBehaviour
    {
        public int CardAttackPoint => cardAttackPoint;
        [SerializeField] private int cardAttackPoint = 1;
        [SerializeField] private Color cardColor = Color.red;
        [SerializeField] private TextMeshProUGUI attackPointText;
        [SerializeField] private Image cardMainImage;
        [SerializeField] private Image cardImageBack;
        [SerializeField] private Image cardFrameImage;
        
        public Image CardImage => cardMainImage;
        public Image CardImageBack => cardImageBack;
        public TextMeshProUGUI AttackPointText => attackPointText;
        
        [field: SerializeField] public CardType CardType { get; private set; }

        public CardAttackInfoPresenter cardAttackInfoPresenter;
        public void InitializeFromCardDataMap(CardDataMapBase cardDataMapBase)
        {
            cardAttackPoint = cardDataMapBase.AttackPoint;
            cardColor = cardDataMapBase.Color;
            attackPointText.text = cardAttackPoint.ToString();
            cardMainImage.color = cardColor;
            CardType = cardDataMapBase.CardType;
            cardMainImage.sprite = cardDataMapBase.MainImage;
            cardImageBack.sprite = cardDataMapBase.BackgroundImage;
            cardFrameImage.sprite = cardDataMapBase.FrameImage;
            
            cardAttackInfoPresenter.SetAttackPoint(cardAttackPoint);
        }
        
        public void InitializeFromCoreParameter(CardOverridableParameter coreParameter)
        {
            cardAttackPoint = coreParameter.CardAttackPoint;
            attackPointText.text = cardAttackPoint.ToString();
            CardType = coreParameter.CardType;
            cardMainImage.sprite = coreParameter.cardMainSprite;
            cardAttackInfoPresenter.SetAttackPoint(cardAttackPoint);
        }
    }

    public class CardOverridableParameter
    {
        public int CardAttackPoint { get; set; }

        [SerializeField] public Sprite cardMainSprite;
        [field: SerializeField] public CardType CardType { get; set; }
        public CardOverridableParameter(int cardAttackPoint, Sprite cardMainSprite, CardType cardType)
        {
            CardAttackPoint = cardAttackPoint;
            this.cardMainSprite = cardMainSprite;
            CardType = cardType;
        }
    }
}
