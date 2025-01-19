using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Card;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Hand
{
    public class HandMover : MonoBehaviour
    {
        [SerializeField]private RectTransform _myCardMoveTarget;
        [SerializeField]private RectTransform _opponentCardMoveTarget;
        private List<RectTransform> _cardsRectTransforms = new List<RectTransform>();
        public async UniTask Initialize()
        {
            _cardsRectTransforms = GetComponentsInChildren<RectTransform>().ToList();
            return;
        }
        
        public async UniTask MoveCardToMyTarget(RectTransform card, Vector2? offset = null, float duration = 1)
        {
            var mover = card.GetComponent<CardMover>();
            await mover.MoveCardToTargetPosition(_myCardMoveTarget.position, true, duration);
            return;
        }
        public async UniTask MoveCardToOpponentTarget(RectTransform card, Vector2? offset = null, float duration = 1)
        {
            var mover = card.GetComponent<CardMover>();
            await mover.MoveCardToTargetPosition(_opponentCardMoveTarget.position, false, duration);
            return;
        }

        public async UniTask OpenDecidedCards(CardMover selectedCardMover, float duration = 0.5f)
        {
            await selectedCardMover.FlipCardToOpen(duration);
            return;
        }
        
        public async UniTask FlipCard(RectTransform card, float duration = 0.5f)
        {
            //cardをY軸を中心に90度回転させた瞬間、CardDataSetterのCardImageとCardImageBackの表示を切り替え、残りの90度を回転させる
            var cardImage = card.GetComponent<CardParameter>().CardImage;
            var cardImageBack = card.GetComponent<CardParameter>().CardImageBack;
            var cardText = card.GetComponent<CardParameter>().AttackPointText;
            var cardAttackInfo = card.GetComponent<CardParameter>().cardAttackInfoPresenter;
            await card.DORotate(new Vector3(0, 90, 0), duration / 2).OnComplete(() =>
            {
                cardImage.gameObject.SetActive(false);
                cardImageBack.gameObject.SetActive(true);
                cardText.gameObject.SetActive(false);
                cardAttackInfo.gameObject.SetActive(false);
                
                card.DORotate(new Vector3(0, 180, 0), duration / 2);
            }).AsyncWaitForCompletion();

            card.GetComponent<CardMover>().isFrontOpen = false;
            return;
        }

        public async UniTask TakeBackCardToHand(int playerSelectedCardIndex, List<CardMover> cardMovers, float duration = 0.5f)
        {
            var cardMover = cardMovers[playerSelectedCardIndex];
            await cardMover.MoveCardToTargetPosition(cardMover.DefaultPosition, false, duration);
            return;
        }

        public async UniTask FlipAllCard(List<CardMover> cardMovers)
        {
            foreach (var cardMover in cardMovers.Where(cardMover => cardMover.isFrontOpen))
            {
                await cardMover.FlipCardToClose();
            }
            return;
        }

        public async UniTask<List<RectTransform>> ShuffleHandPosition(List<CardMover> cardMovers)
        {
            Debug.Log("ShuffleHandPosition");
            _cardsRectTransforms = cardMovers.Select(cardObject => cardObject.GetComponent<RectTransform>()).ToList();
            //_cardsのpositionを保存
            var defaultPositions = cardMovers.Select( cardMover => cardMover.DefaultPosition).ToList();
            
            //_cardsのpositionをindex0のpositionに変更
            List<UniTask> tasksResetPosition = new List<UniTask>();
            foreach (var cardMover in cardMovers)
            {
                tasksResetPosition.Add(cardMover.MoveCardToTargetPosition(defaultPositions[0], false, 0.5f));
            }
            await UniTask.WhenAll(tasksResetPosition);

            //_cardsのインデックス順をシャッフル
            _cardsRectTransforms = _cardsRectTransforms.OrderBy(a => System.Guid.NewGuid()).ToList();

            List<UniTask> tasksShufflePosition = new List<UniTask>();
            //シャッフル後のインデックス順に合わせて_cardsをDoMoveで移動させ完了をawaitする
            var shuffleMovers = _cardsRectTransforms.Select(card => card.GetComponent<CardMover>()).ToList();
            shuffleMovers.ForEach(cardMover =>
            {
                tasksShufflePosition.Add(cardMover.MoveCardToTargetPosition(cardMover.DefaultPosition, false, 0.5f));
            });
            await UniTask.WhenAll(tasksShufflePosition);

            //_cardsのSetDefaultCardPositionをコール
            shuffleMovers.ForEach(cardMover => cardMover.SetDefaultCardPosition());
            return _cardsRectTransforms;
        }

        public async UniTask OpenAllHand(List<CardMover> cardMovers)
        {
            //表面を向いていないカードすべてを表面にする
            foreach (var cardMover in cardMovers.Where(cardMover => cardMover.isFrontOpen == false))
            {
                await cardMover.FlipCardToOpen();
            }
        }
    }
}