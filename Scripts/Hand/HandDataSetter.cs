using System.Collections.Generic;
using Card;
using UnityEngine;

namespace Hand
{
    public class HandDataSetter
    {
        public void SetHandData(CardDataList cardDataList, List<CardDataSetter> cardDataSetters)
        {
            int count = 0;
            cardDataSetters.ForEach(cardDataSetter =>
            {
                cardDataSetter.SetCardData(cardDataList.cardDataList[count]);
                count++;
            });
        }
    }
}