using System.Collections.Generic;
using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardShop
{
    public class CardShopView : MonoBehaviour
    {
        [SerializeField] GameObject cardPrefab;
        [SerializeField] List<GameObject> cardLineup;
        [SerializeField] Transform cardShopRoot;
        
        public async UniTask SetShopLineup(List<CardOverridableParameter> lineup, CardDataMap cardDataMap)
        {
            foreach (var card in lineup)
            {
                var cardObject = Instantiate(cardPrefab, cardShopRoot);
                cardLineup.Add(cardObject);
                
                cardObject.GetComponent<CardParameter>().InitializeFromCoreParameter(card);
            }
            await UniTask.Yield();
            cardLineup.ForEach(card =>
            {
                card.GetComponent<CardMover>().SetDefaultCardPosition();
            });
        }
        
        public List<GameObject> GetCardLineup()
        {
            return cardLineup;
        }
        
        public int GetIndexFromLineup(GameObject card)
        {
            return cardLineup.IndexOf(card);
        }
        
        public void SetLineupMoveable(bool isMovable)
        {
            foreach (var card in cardLineup)
            {
                card.GetComponent<CardMover>().IsMovable = isMovable;
            }
        }
        
        public void RemoveCardFromLineupList(GameObject card)
        {
            cardLineup.Remove(card);
        }
        
        public void RemoveAllCardFromLineupList()
        {
            cardLineup.ForEach(Destroy);
            cardLineup.Clear();
        }
    }
}