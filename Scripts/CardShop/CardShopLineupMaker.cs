using System.Collections.Generic;
using Card;
using UnityEngine;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random; 

namespace CardShop
{
    public class CardShopLineupMaker
    {
        private List<CardOverridableParameter> Lineup { get; set; }
        private Random _random;

        public List<CardOverridableParameter> CreateLineup(CardDataMap cardDataMap,
            int lineupCount = 5)
        {
            //todo この辺の生成ロジックがおかしい
            Lineup = new List<CardOverridableParameter>();
            int count = 0;
            for (var i = 0; i < lineupCount; i++)
            {
                //CardDataをランダムなパラメータで生成
                var cardData = CreateCardParameterFromLottery(cardDataMap);
                Lineup.Add(cardData);
            }

            return Lineup;
        }

        public void SetRandom(Random random)
        {
            _random = random;
            Debug.Log("SetRandom: " + _random);
        }
        private CardOverridableParameter CreateCardParameterFromLottery(CardDataMap cardDataMap)
        {
            var attackPoint = _random.NextInt(1, 4);
//            var attackPoint = Random.Range(1, 4);
            var cardType = (CardType)_random.NextInt(0, 3);
            var sprite = cardDataMap.cardDataList[(int)cardType].MainImage;
            var parameter = new CardOverridableParameter(attackPoint, sprite, cardType);
            Debug.Log("CardType: " + parameter.CardType);
            Debug.Log("CardType: " + (int)parameter.CardType);
            
            Debug.Log("MainImage: " + cardDataMap.cardDataList[(int)parameter.CardType].MainImage);
            Debug.Log("MainImage: " + parameter.cardMainSprite);
            
            parameter.cardMainSprite = cardDataMap.cardDataList[(int)parameter.CardType].MainImage;
            
            
            return parameter;
        }
    }
}