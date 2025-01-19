using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Card
{
    [CreateAssetMenu(fileName = "CardData", menuName = "CardDataMap")]
    public class CardDataMap : ScriptableObject
    {
        public List<CardDataMapBase> cardDataList;
    }

    [Serializable]
    public class CardDataMapBase
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
        Blue,
        None
    }
}