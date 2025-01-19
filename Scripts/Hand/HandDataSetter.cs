using System.Collections.Generic;
using Card;
using UnityEngine;

namespace Hand
{
    public class HandDataSetter
    {
        public void SetHandData(CardDataMap cardDataMap, List<CardParameter> cardParameters)
        {
            int count = 0;
            cardParameters.ForEach(cardDataSetter =>
            {
                cardDataSetter.InitializeFromCardDataMap(cardDataMap.cardDataList[count]);
                count++;
            });
        }
    }
}