using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Card
{
    [CreateAssetMenu(fileName = "CardData", menuName = "CardDataList")]
    public class CardDataList : ScriptableObject
    {
        public List<CardData> cardDataList;
    }

    [Serializable]
    public class CardData
    {
        public int AttackPoint;
        public Color Color;
        public CardType CardType;
        public Sprite MainImage;
        public Sprite BackgroundImage;
        public Sprite FrameImage;
    }

    public enum CardType
    {
        Red,
        Green,
        Blue
    }
}