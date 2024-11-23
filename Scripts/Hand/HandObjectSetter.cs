using System;
using System.Collections.Generic;
using Card;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Hand
{
    public class HandObjectSetter : MonoBehaviour
    {
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private CardDataList cardDataList;
        [field: SerializeField] public bool Intaractable { get; set; } = false;

        private void Start()
        {
            CreateHandObject();
        }

        public void CreateHandObject()
        {
            List<GameObject> handObjects = new List<GameObject>();
            List<CardDataSetter> cardDataSetters = new List<CardDataSetter>();

            cardDataList.cardDataList.ForEach(cardData =>
            {
                var createdCard = Instantiate(cardPrefab, transform);
                handObjects.Add(createdCard);
                cardDataSetters.Add(createdCard.GetComponent<CardDataSetter>());
            });
            
            var handDataSetter = new HandDataSetter();
            handDataSetter.SetHandData(cardDataList, cardDataSetters);
            
            SetCardLayoutHorizontal(handObjects);
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
                handObjects[i].GetComponent<CardObject>().SetDefaultCardPosition();
                if(Intaractable) handObjects[i].GetComponent<CardObject>().Intaractable = true;
            }
        }
        
        //CardObjectのIntaractableを切り替える
        public void SetCardIntaractable()
        {
            foreach (Transform child in transform)
            {
                child.GetComponent<CardObject>().Intaractable = Intaractable;
            }
        }
    }
}