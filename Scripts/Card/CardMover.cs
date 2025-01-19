using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Card
{
    public class CardMover : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        //カードの元の位置座標
        public Vector3 DefaultPosition { get; private set; }
        private Vector3 _offset;
        public bool isFrontOpen = true;
        public Subject<(CardType, RectTransform)> CardSelected { get; } = new Subject<(CardType, RectTransform)>();
        [field:SerializeField] public bool IsMovable { get; set; } = false;

        public ReactiveProperty<bool> IsCardDragging { get; } = new ReactiveProperty<bool>(false);
        public void SetDefaultCardPosition()
        {
            //カードの元の位置座標をセット_動的生成しているため生成時にコールしている
            DefaultPosition = transform.position;
        }
        public void SetDefaultCardPosition(Vector3 defaultPosition)
        {
            DefaultPosition = defaultPosition;
        }

        public async UniTask MoveCardToTargetPosition(Vector3 targetPosition, bool isFlipToClose, float duration = 1)
        {
            if(isFlipToClose) FlipCardToClose().Forget();
            transform.DOScale(1, duration / 2);
            await transform.DOMove(targetPosition, duration).AsyncWaitForCompletion();
            return;
        }
        public async UniTask MoveCardToGettingItemPosition(Vector3 targetPosition, float duration)
        {
            transform.DOScale(Vector3.one * 1.5f, duration / 2);
            await transform.DOMove(targetPosition, duration).AsyncWaitForCompletion();
        }
        
        public async UniTask FlipCardToClose(float duration = 0.5f)
        {
            //cardをY軸を中心に90度回転させた瞬間、CardDataSetterのCardImageとCardImageBackの表示を切り替え、残りの90度を回転させる
            var cardParameter = GetComponent<CardParameter>();
            await transform.DORotate(new Vector3(0, 90, 0), duration / 2).OnComplete(() =>
            {
                cardParameter.CardImage.gameObject.SetActive(false);
                cardParameter.CardImageBack.gameObject.SetActive(true);
                cardParameter.AttackPointText.gameObject.SetActive(false);
                cardParameter.cardAttackInfoPresenter.gameObject.SetActive(false);
                
                transform.DORotate(new Vector3(0, 180, 0), duration / 2);
            }).AsyncWaitForCompletion();

            isFrontOpen = false;
            return;
        }
        public async UniTask FlipCardToOpen(float duration = 0.5f)
        {
            var cardParameter = GetComponent<CardParameter>();

            await transform.DORotate(new Vector3(0, 90, 0), duration / 2).OnComplete(() =>
            {
                cardParameter.CardImage.gameObject.SetActive(true);
                cardParameter.CardImageBack.gameObject.SetActive(false);
                cardParameter.AttackPointText.gameObject.SetActive(true);
                cardParameter.cardAttackInfoPresenter.gameObject.SetActive(true);

                transform.DORotate(new Vector3(0, 0, 0), duration / 2);
            }).AsyncWaitForCompletion();
            
            isFrontOpen = true;
            return;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsMovable) {return;}
            _offset = transform.position - Input.mousePosition;
            IsCardDragging.Value = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsMovable) {return;}
            Vector3 mousePos = Input.mousePosition;
            
            //カードの位置をマウス+オフセットの位置にする
            transform.position = mousePos + _offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsMovable) {return;}

            IsCardDragging.Value = false;
            
            //vector3がCollider2Dの範囲内かチェック
            if (Physics2D.OverlapPoint(Input.mousePosition) != null)
            {
                //カードが範囲内にある場合の処理
                var cardType = GetComponent<CardParameter>().CardType;
                Debug.Log("カードを選択しました : " + cardType);
                CardSelected.OnNext((cardType, GetComponent<RectTransform>()));
            }
            else
            {
                //カードの位置を元に戻す
                transform.DOMove(DefaultPosition, 0.5f);
            }
        }

        public async UniTask PlayRemoveAnimation()
        {
            await transform.DOScale(Vector3.zero, 0.5f).AsyncWaitForCompletion();
        }

  
    }
}