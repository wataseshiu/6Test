using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using MultiPlay;
using R3;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hand
{
    public class HandManager : MonoBehaviour
    {
        [SerializeField]private List<CardMover> cardMovers;
        private HandObjectSetter _handObjectSetter;
        private HandMover _handMover;
        [SerializeField] private HitAreaVisualSwitcher hitAreaVisualSwitcher;

        private void Start()
        {
            _handObjectSetter = GetComponent<HandObjectSetter>();
            _handMover = GetComponent<HandMover>();
        }

        public async UniTask CreateHand()
        {
            await _handObjectSetter.CreateHandObject();
        }
        public void DeleteHand()
        {
            _handObjectSetter.DeleteHandObject();
            foreach (var cardObject in cardMovers)
            {
                Destroy(cardObject.gameObject);
            }
            cardMovers.Clear();
        }
        
        public void Initialize()
        {
            
            cardMovers = GetCardObject();
            _handMover.Initialize().Forget();
            
            //cardObjectsのIsCardDraggingの値がどれか1つでも変わっていたら処理をする
            cardMovers.ForEach(cardObject =>
            {
                cardObject.IsCardDragging.Subscribe(isDragging =>
                {
                    if (isDragging)
                    {
                        hitAreaVisualSwitcher.SetHitAreaVisualize(true);
                    }
                    else
                    {
                        hitAreaVisualSwitcher.SetHitAreaVisualize(false);
                    }
                });
            });
        }
        
        private List<CardMover> GetCardObject()
        {
            //ログ出力
            foreach (var card in GetComponentsInChildren<CardMover>())
            {
                Debug.Log(card.gameObject.name);
            }
            return GetComponentsInChildren<CardMover>().ToList();
        }
        
        public CardMover GetCardMoverFromIndex(int index)
        {
            return cardMovers[index];
        }
        
        public int GetIndexOfCardMover(CardMover cardMover)
        {
            return cardMovers.IndexOf(cardMover);
        }

        public int GetCardAttackParameterFromIndex(int index)
        {
            return cardMovers[index].GetComponent<CardParameter>().CardAttackPoint;
        }
        
        public async UniTask<(CardType, RectTransform, int)> WaitCardSelect()
        {
            _handObjectSetter.SetCardIntaractable(true);
            hitAreaVisualSwitcher.SetHitAreaColliderActive(true);
            var cts = new CancellationTokenSource();
            var cancelToken = cts.Token;
            var tasks = cardMovers.Select(cardObject => cardObject.CardSelected.FirstAsync(cancelToken).AsUniTask());
            //_cardObjectsの中のいずれかのCardSelectedのFirstAsyncの完了まで待つ
            var result = await UniTask.WhenAny(tasks);
            cts.Cancel();
            
            //resultのCardType
            var cardType = result.result.Item1;
            var cardRectTransform = result.result.Item2;
            Debug.Log("選択されたカード : " + cardType);
            hitAreaVisualSwitcher.SetHitAreaColliderActive(false);
            _handObjectSetter.SetCardIntaractable(false);
            return (cardType, cardRectTransform, result.winArgumentIndex);
        }
        public async UniTask<(CardType, RectTransform, int)> WaitCardSelect(MultiPlayManager localMultiPlayManager)
        {
            var result = await WaitCardSelect();
            localMultiPlayManager.SendSelectCard(result.Item3);
            //
            Debug.Log("自分のカード選択を通知");
            return result;
        }
        //対戦相手のカード選出を待つ
        public async UniTask<(CardType, RectTransform, int)> WaitOpponentCardSelect()
        {
            await FlipAllCard();
            await ShuffleHandPosition();
            
            //TODO : 対戦相手のカード選出処理_とりあえずランダムで選出
            var rand = Random.Range(0, 3);
            var cardRectTransform = cardMovers[rand].GetComponent<RectTransform>();
            var cardDataSetter = cardMovers[rand].GetComponent<CardParameter>();
            var cardType = cardDataSetter.CardType;

            await UniTask.WaitForSeconds(0.5f);
            Debug.Log("対戦相手が選択したカード : " + cardType);
            return (cardType, cardRectTransform, rand);
        }
        public async UniTask<(CardType, RectTransform, int)> WaitOpponentCardSelect(MultiPlayManager multiPlayManager)
        {
            var result = await multiPlayManager.WaitOtherPlayerSelectCard();
            var rectTransform = cardMovers[result].GetComponent<RectTransform>();
            var cardDataSetter = cardMovers[result].GetComponent<CardParameter>();
            Debug.Log("対戦相手のカード選択を待ち終えた");

            return (cardDataSetter.CardType, rectTransform, result);
        }

        public async UniTask MoveCardToMyTarget(RectTransform myCardItem2, Vector2? offset = null, float duration = 1)
        {
            await _handMover.MoveCardToMyTarget(myCardItem2, offset, duration);
        }
        public async UniTask MoveCardToOpponentTarget(RectTransform opponentCardItem2, Vector2? offset = null, float duration = 1)
        {
            await _handMover.MoveCardToOpponentTarget(opponentCardItem2, offset, duration);
        }

        public async UniTask OpenDecidedCards(int selectedCardIndex)
        {
            await _handMover.OpenDecidedCards(cardMovers[selectedCardIndex]);
        }

        public async UniTask TakeBackCardToHand(int playerSelectedCardIndex, bool isFlip = false)
        {
            await _handMover.TakeBackCardToHand(playerSelectedCardIndex, cardMovers);
            if (isFlip)
            {
                await _handMover.FlipCard(cardMovers[playerSelectedCardIndex].GetComponent<RectTransform>());
            }
        }
        
        public void ResetHandPosition()
        {
            _handObjectSetter.SetCardLayoutHorizontal(cardMovers.Select(cardObject => cardObject.gameObject).ToList());
        }

        public async UniTask FlipAllCard()
        {
            await _handMover.FlipAllCard(cardMovers);
        }

        public async UniTask ShuffleHandPosition()
        {
            var cardIndexList = await _handMover.ShuffleHandPosition(cardMovers);
            //_cardObjectsをcardIndexListの順番に並び替える

            var newList = cardIndexList.Select( indexList => indexList.GetComponent<CardMover>()).ToList();

            cardMovers.Clear();
            await UniTask.Delay(100);
            cardMovers.AddRange(newList);

            //_cardObjectsのGameObjectのHierarchyの並び順を更新
            foreach (var card in cardMovers)
            {
                card.transform.SetAsLastSibling();
            }
            
            foreach (var card in cardMovers)
            {
                Debug.Log(card.gameObject.name);
            }
        }

        public async UniTask OpenAllHands()
        {
            await _handMover.OpenAllHand(cardMovers);
        }
        
        public async UniTask AddCardToHand(CardMover cardMover)
        {
            cardMovers.Add(cardMover);
            
            //一時的にCanvasを作成しアタッチ、SortOrderを最前面にする
            var canvas = cardMover.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            await UniTask.Yield();
            
            await cardMover.MoveCardToGettingItemPosition(transform.parent.position, 0.5f);
            await UniTask.Delay(1000);
            cardMover.transform.SetParent(transform);
            _handObjectSetter.SetDefaultCardPositionHorizontalLayout(cardMovers);
            //cardMoversをDefaultPositionに移動させる
            await UniTask.WhenAll(cardMovers.Select(cardObject => cardObject.MoveCardToTargetPosition(cardObject.DefaultPosition, false, 0.5f)));
            
            Destroy(canvas);
            
//            _handObjectSetter.SetCardLayoutHorizontal(cardMovers.Select(cardObject => cardObject.gameObject).ToList());
        }

        public async UniTask RemoveCardFromHand(int playerSelectCardIndex)
        {
            var card = cardMovers[playerSelectCardIndex];
            await card.PlayRemoveAnimation();
            cardMovers.Remove(card);
            Destroy(card.gameObject);
            _handObjectSetter.SetCardLayoutHorizontal(cardMovers.Select(cardObject => cardObject.gameObject).ToList());
        }
    }
}