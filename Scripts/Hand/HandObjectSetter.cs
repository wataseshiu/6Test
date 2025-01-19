using System;
using System.Collections.Generic;
using System.Linq;
using Card;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Hand
{
    public class HandObjectSetter : MonoBehaviour
    {
        [SerializeField] private GameObject cardPrefab;
        [FormerlySerializedAs("cardDataList")] [SerializeField] private CardDataMap cardDataMap;

        private List<GameObject> _handObjects;
        private List<CardParameter> _cardDataSetters;
        public async UniTask CreateHandObject()
        {
            _handObjects = new List<GameObject>();
            _cardDataSetters = new List<CardParameter>();
            int count = 0;
            
            cardDataMap.cardDataList.ForEach(cardData =>
            {
                var createdCard = Instantiate(cardPrefab, transform);
                createdCard.name = createdCard.name + count;
                _handObjects.Add(createdCard);
                _cardDataSetters.Add(createdCard.GetComponent<CardParameter>());
                count++;
            });

            await UniTask.Yield();
            
            var handDataSetter = new HandDataSetter();
            handDataSetter.SetHandData(cardDataMap, _cardDataSetters);
            
            SetCardLayoutHorizontal(_handObjects);
        }

        public void DeleteHandObject()
        {
            _handObjects.ForEach(Destroy);
            _handObjects.Clear();
            _cardDataSetters.Clear();
        }

        public void SetCardLayoutHorizontal(List<GameObject> handObjects, float cardMargin = 10f)
        {
            //handObjectsを中央揃え横並びにする処理
            float cardWidth = cardPrefab.GetComponent<RectTransform>().rect.width;
            float totalWidth = cardWidth * handObjects.Count + cardMargin * (handObjects.Count - 1);
            float startX = -totalWidth / 2 + cardWidth / 2;
            for (int i = 0; i < handObjects.Count; i++)
            {
                handObjects[i].transform.localPosition = new Vector3(startX + (cardWidth + cardMargin) * i, 0, 0);
                handObjects[i].GetComponent<CardMover>().SetDefaultCardPosition();
            }
        }
        
        public void SetDefaultCardPositionHorizontalLayout(List<CardMover> cardMovers, float cardMargin = 1f)
        {
            //handObjectsを中央揃え横並びにする処理
            float cardWidth = cardPrefab.GetComponent<RectTransform>().rect.width;
            float totalWidth = cardWidth * cardMovers.Count + cardMargin * (cardMovers.Count - 1);
            float startX = -totalWidth / 2 + cardWidth / 2;
            for (int i = 0; i < cardMovers.Count; i++)
            {
                var position = new Vector3(startX + (cardWidth + cardMargin) * i, 0, 0);
                cardMovers[i].SetDefaultCardPosition(position + transform.position);
            }
        }
        
        //CardObjectのIntaractableを切り替える
        public void SetCardIntaractable(bool isMovable)
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<CardMover>().IsMovable = isMovable;
            }
        }
    }
}