using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Hand
{
    public class HandManager : MonoBehaviour
    {
        [SerializeField]private List<CardObject> _cardObjects;
        private HandObjectSetter _handObjectSetter;
        private HandMoveVisualizer _handMoveVisualizer;
        [SerializeField] private HitAreaVisualSwitcher _hitAreaVisualSwitcher;

        private void Start()
        {
            _handObjectSetter = GetComponent<HandObjectSetter>();
            _handMoveVisualizer = GetComponent<HandMoveVisualizer>();
        }
        
        public void Initialize()
        {
            _cardObjects = GetCardObject();
            _handObjectSetter.Intaractable = true;
            _handObjectSetter.SetCardIntaractable();
            _handMoveVisualizer.Initialize().Forget();
            
            //cardObjectsのIsCardDraggingの値がどれか1つでも変わっていたら処理をする
            _cardObjects.ForEach(cardObject =>
            {
                cardObject.IsCardDragging.Subscribe(isDragging =>
                {
                    if (isDragging)
                    {
                        _hitAreaVisualSwitcher.SetHitAreaVisual(true);
                    }
                    else
                    {
                        _hitAreaVisualSwitcher.SetHitAreaVisual(false);
                    }
                });
            });
        }
        
        private List<CardObject> GetCardObject()
        {
            return GetComponentsInChildren<CardObject>().ToList();
        }

        public async UniTask<(CardType, RectTransform, int)> WaitCardSelect()
        {
            var cts = new CancellationTokenSource();
            var cancelToken = cts.Token;
            var tasks = _cardObjects.Select(cardObject => cardObject.CardSelected.FirstAsync(cancelToken).AsUniTask());
            //_cardObjectsの中のいずれかのCardSelectedのFirstAsyncの完了まで待つ
            var result = await UniTask.WhenAny(tasks);
            cts.Cancel();
            
            //resultのCardType
            var cardType = result.result.Item1;
            var cardRectTransform = result.result.Item2;
            Debug.Log("選択されたカード : " + cardType);
            _handObjectSetter.Intaractable = false;
            _handObjectSetter.SetCardIntaractable();
            return (cardType, cardRectTransform, result.winArgumentIndex);
        }
        
        //対戦相手のカード選出を待つ
        public async UniTask<(CardType, RectTransform, int)> WaitOpponentCardSelect()
        {
            //TODO : 対戦相手のカード選出処理_とりあえずランダムで選出
            var rand = Random.Range(0, 3);
            var cardType = (CardType)rand;
            var cardRectTransform = _cardObjects[rand].GetComponent<RectTransform>();
            await UniTask.WaitForSeconds(1);
            Debug.Log("対戦相手が選択したカード : " + cardType);
            return (cardType, cardRectTransform, rand);
        }

        public async UniTask MoveCardToMyTarget(RectTransform myCardItem2, Vector2? offset = null, float duration = 1)
        {
            await _handMoveVisualizer.MoveCardToMyTarget(myCardItem2, offset, duration);
        }
        public async UniTask MoveCardToOpponentTarget(RectTransform opponentCardItem2, Vector2? offset = null, float duration = 1)
        {
            await _handMoveVisualizer.MoveCardToOpponentTarget(opponentCardItem2, offset, duration);
        }

        public async UniTask OpenDecidedCards(int selectedCardIndex)
        {
            await _handMoveVisualizer.OpenDecidedCards(_cardObjects[selectedCardIndex]);
        }

        public async UniTask TakeBackCardToHand(int playerSelectedCardIndex)
        {
            await _handMoveVisualizer.TakeBackCardToHand(playerSelectedCardIndex, _cardObjects);
        }
        
        public void ResetHandPosition()
        {
            _handObjectSetter.SetCardLayoutHorizontal(_cardObjects.Select(cardObject => cardObject.gameObject).ToList());
        }
    }
}