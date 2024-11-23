using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Hand
{
    public class HandMoveVisualizer : MonoBehaviour
    {
        [SerializeField]private RectTransform _myCardMoveTarget;
        [SerializeField]private RectTransform _opponentCardMoveTarget;
        private List<RectTransform> _cards = new List<RectTransform>();
        public async UniTask Initialize()
        {
            _cards = GetComponentsInChildren<RectTransform>().ToList();
            return;
        }
        
        public async UniTask MoveCardToMyTarget(RectTransform card, Vector2? offset = null, float duration = 1)
        {
            //TODO : カードの移動処理
            Vector2 offsetValue = offset ?? Vector2.zero;
            FlipCard(card).Forget();
            await card.transform.DOMove( _myCardMoveTarget.transform.position, duration).AsyncWaitForCompletion();
//            await card.DOAnchorPos(_myCardMoveTarget.anchoredPosition + offsetValue, duration).AsyncWaitForCompletion();
            return;
        }
        public async UniTask MoveCardToOpponentTarget(RectTransform card, Vector2? offset = null, float duration = 1)
        {
            //TODO : カードの移動処理
            Vector2 offsetValue = offset ?? Vector2.zero;
            FlipCard(card).Forget();
            await card.transform.DOMove( _opponentCardMoveTarget.transform.position, duration).AsyncWaitForCompletion();
            return;
        }
        public async UniTask MoveCardToTarget(RectTransform card, RectTransform target, Vector2? offset = null, float duration = 1)
        {
            //TODO : カードの移動処理
            Vector2 offsetValue = offset ?? Vector2.zero;
            await card.DOAnchorPos(target.anchoredPosition + offsetValue, duration).AsyncWaitForCompletion();
            return;
        }
        public async UniTask MoveCardToVector2(RectTransform card, Vector2 anchoredPosition, Vector2? offset = null, float duration = 1)
        {
            //TODO : カードの移動処理
            Vector2 offsetValue = offset ?? Vector2.zero;
            await card.DOAnchorPos(anchoredPosition + offsetValue, duration).AsyncWaitForCompletion();
            return;
        }

        public async UniTask OpenDecidedCards(CardObject selectedCardObject, float duration = 0.5f)
        {
            //TODO : カードのオープン処理
            var cardImage = selectedCardObject.GetComponent<CardDataSetter>().CardImage;
            var cardImageBack = selectedCardObject.GetComponent<CardDataSetter>().CardImageBack;
            var cardText = selectedCardObject.GetComponent<CardDataSetter>().AttackPointText;
            var rectTransform = selectedCardObject.GetComponent<RectTransform>();
            await rectTransform.DORotate(new Vector3(0, 90, 0), duration / 2).OnComplete(() =>
            {
                //TODO : CardDataSetterのCardImageとCardImageBackの表示を切り替え
                cardImage.gameObject.SetActive(true);
                cardImageBack.gameObject.SetActive(false);
                cardText.gameObject.SetActive(true);
                
                rectTransform.DORotate(new Vector3(0, 0, 0), duration / 2);
            }).AsyncWaitForCompletion();
            return;
        }

        private async UniTask SetHands()
        {
            //TODO : カードのセット処理
            await UniTask.WaitForSeconds(1);
            return;
        }
        
        private async UniTask FlipCard(RectTransform card, float duration = 0.5f)
        {
            //TODO : カードの裏返し処理
            
            //cardをY軸を中心に90度回転させた瞬間、CardDataSetterのCardImageとCardImageBackの表示を切り替え、残りの90度を回転させる
            var cardImage = card.GetComponent<CardDataSetter>().CardImage;
            var cardImageBack = card.GetComponent<CardDataSetter>().CardImageBack;
            var cardText = card.GetComponent<CardDataSetter>().AttackPointText;
            await card.DORotate(new Vector3(0, 90, 0), duration / 2).OnComplete(() =>
            {
                //TODO : CardDataSetterのCardImageとCardImageBackの表示を切り替え
                cardImage.gameObject.SetActive(false);
                cardImageBack.gameObject.SetActive(true);
                cardText.gameObject.SetActive(false);
                
                card.DORotate(new Vector3(0, 180, 0), duration / 2);
            }).AsyncWaitForCompletion();

//            await UniTask.WaitForSeconds(duration);
            return;
        }

        public async UniTask TakeBackCardToHand(int playerSelectedCardIndex, List<CardObject> cardObjects, float duration = 0.5f)
        {
            var card = cardObjects[playerSelectedCardIndex].GetComponent<RectTransform>();
            await card.transform.DOMove(card.GetComponent<CardObject>().DefaultPosition, duration).AsyncWaitForCompletion();
            return;
        }
    }
}