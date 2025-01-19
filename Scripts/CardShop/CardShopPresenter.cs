using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Linq;
using Cysharp.Threading.Tasks.Linq;
using Hand;
using MultiPlay;
using R3;
using Random = UnityEngine.Random;

namespace CardShop
{
    public class CardShopPresenter : MonoBehaviour
    {
        [SerializeField] Transform shopUIRoot;
        [SerializeField] Image dropArea;
        [SerializeField] CardDataMap cardDataMap;
        [SerializeField] CardShopView cardShopView;
        [SerializeField] HitAreaVisualSwitcher hitAreaVisualSwitcher;
        
        public void ShowCardShop()
        {
            shopUIRoot.gameObject.SetActive(true);
        }
        
        public void HideCardShop()
        {
            shopUIRoot.gameObject.SetActive(false);
        }
        
        public async UniTask CreateLineup(CardShopLineupMaker cardShopLineupMaker)
        {
            var lineUp = cardShopLineupMaker.CreateLineup(cardDataMap);
            await cardShopView.SetShopLineup(lineUp, cardDataMap);
        }
        
        public int GetIndexFromLineup(GameObject card)
        {
            return cardShopView.GetIndexFromLineup(card);
        }
        
        public CardMover GetCardMoverFromIndex(int index)
        {
            var cardObject = cardShopView.GetCardLineup()[index];
            return cardObject.GetComponent<CardMover>();
        }
        
        public async UniTask<RectTransform> SelectCardAsync(MultiPlayManager multiPlayManager = null)
        {
            //GetCardLineup()で取得したカードのいずれかを選択するまで待つ
            var cardObject = cardShopView.GetCardLineup();
            var cardMovers = cardObject.Select(card => card.GetComponent<CardMover>()).ToList();
            var cardSelectSubjects = cardMovers.Select(card => card.CardSelected);

            hitAreaVisualSwitcher.SetHitAreaColliderActive(true);

            //カードドラッグ中はドロップエリアを表示する
            //cardObjectsのIsCardDraggingの値がどれか1つでも変わっていたら処理をする
            List<IDisposable> disposables = new List<IDisposable>();
            cardMovers.ForEach(mover =>
            {
                disposables.Add(mover.IsCardDragging.Subscribe(isDragging => { dropArea.enabled = isDragging; }));
            });

            var tasks = new List<UniTask<(CardType, RectTransform)>>();
            foreach (var subject in cardSelectSubjects)
            {
                tasks.Add(subject.FirstAsync().AsUniTask());
            }

            cardShopView.SetLineupMoveable(true);
            var completedTask = await UniTask.WhenAny(tasks);
            disposables.ForEach(disposable => disposable.Dispose());
            var card = completedTask.result.Item2;

            //選択したカードの情報をMultiPlayManagerに渡す
            if (multiPlayManager)
            {
                var index = cardShopView.GetIndexFromLineup(card.gameObject);
                multiPlayManager.SendSelectShopCard(index);
                Debug.Log($"入手したカードのショップリスト内インデックス：{index}");
            }
            cardShopView.SetLineupMoveable(false);
            hitAreaVisualSwitcher.SetHitAreaColliderActive(false);

            cardShopView.RemoveCardFromLineupList(card.gameObject);
            
            //resultの中身を取得
            return card;
        }

        public RectTransform SelectCardNpc()
        {
            var cardObject = cardShopView.GetCardLineup();
            //cardObjectの中からランダムで1つ選択
            var card = cardObject[Random.Range(0, cardObject.Count)].GetComponent<CardMover>();
            
            cardShopView.RemoveCardFromLineupList(card.gameObject);
            
            return card.GetComponent<RectTransform>();
        }

        public void ResetCardShop()
        {
            cardShopView.RemoveAllCardFromLineupList();
        }

        public void RemoveCardLineupFromIndex(int index)
        {
            var card = cardShopView.GetCardLineup()[index];
            cardShopView.RemoveCardFromLineupList(card);
        }
    }
}