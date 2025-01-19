using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using MultiPlay;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace CardShop
{
    public class CardShopManager
    {
        private CardShopPresenter _cardShopPresenter;
        private CardShopLineupMaker _cardShopLineupMaker;

        public CardShopManager(CardShopPresenter cardShopPresenter)
        {
            _cardShopPresenter = cardShopPresenter;
            _cardShopLineupMaker = new CardShopLineupMaker();
        }

        public async UniTask Initialize(Random random)
        {
            _cardShopPresenter.HideCardShop();
        }
        public void InitializeRandom(Random random)
        {
            _cardShopLineupMaker.SetRandom(random);
        }

        public async UniTask ShowCardShop()
        {
            _cardShopPresenter.ShowCardShop();
            await _cardShopPresenter.CreateLineup(_cardShopLineupMaker);
        }

        public int GetIndexFromLineup(GameObject card)
        {
            return _cardShopPresenter.GetIndexFromLineup(card);
        }
        
        public async UniTask<CardMover> SelectCardFirst(MultiPlayManager multiPlayManager = null)
        {
            //カードをドロップフィールド上までドラッグしたら対象を選択したとみなして返す
            var card = await _cardShopPresenter.SelectCardAsync(multiPlayManager);
            Debug.Log("SelectCardFirst : " + card.name);
            return card.GetComponent<CardMover>();
        }
        
        public async UniTask<CardMover> SelectCardSecond(MultiPlayManager multiPlayManager = null)
        {
            //カードをドロップフィールド上までドラッグしたら対象を選択したとみなして返す
            var card = await _cardShopPresenter.SelectCardAsync(multiPlayManager);
            Debug.Log("SelectCardSecond : " + card.name);
            return card.GetComponent<CardMover>();
        }
        
        public async UniTask<CardMover> SelectCardNPC()
        {
            var card = _cardShopPresenter.SelectCardNpc();
            Debug.Log("SelectCardNPC : " + card.name);
            //NPCのカード選択処理
            return card.GetComponent<CardMover>();
        }

        public async UniTask HideCardShop()
        {
            _cardShopPresenter.HideCardShop();
        }
        
        public void ResetCardShop()
        {
            _cardShopPresenter.ResetCardShop();
        }

        public CardMover GetCardFromIndex(int index)
        {
            return _cardShopPresenter.GetCardMoverFromIndex(index);
        }

        public void RemoveCardLineupFromIndex(int index)
        {
            _cardShopPresenter.RemoveCardLineupFromIndex(index);
        }
    }
}