using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

namespace Card
{
    public class CardObject : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        //カードの元の位置座標
        public Vector3 DefaultPosition { get; private set; }
        private Vector3 _offset;
        public Subject<(CardType, RectTransform)> CardSelected { get; } = new Subject<(CardType, RectTransform)>();
        [field:SerializeField] public bool Intaractable { get; set; } = false;

        public ReactiveProperty<bool> IsCardDragging { get; } = new ReactiveProperty<bool>(false);
        public void SetDefaultCardPosition()
        {
            //カードの元の位置座標をセット_動的生成しているため生成時にコールしている
            DefaultPosition = transform.position;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Intaractable) {return;}
            _offset = transform.position - Input.mousePosition;
            IsCardDragging.Value = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Intaractable) {return;}
            Vector3 mousePos = Input.mousePosition;
            
            //カードの位置をマウス+オフセットの位置にする
            transform.position = mousePos + _offset;
            
            //vector3がCollider2Dの範囲内かチェック
            var collider = Physics2D.OverlapPoint(Input.mousePosition);
            if (collider != null)
            {
                //カードが範囲内にある場合の処理
//                Debug.Log("カードが範囲内にあります");
            }
            else
            {
                //カードが範囲外にある場合の処理
//                Debug.Log("カードが範囲外にあります");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Intaractable) {return;}

            IsCardDragging.Value = false;
            
            //vector3がCollider2Dの範囲内かチェック
            if (Physics2D.OverlapPoint(Input.mousePosition) != null)
            {
                //カードが範囲内にある場合の処理
                var cardType = GetComponent<CardDataSetter>().CardType;
                Debug.Log("カードを選択しました : " + cardType);
                CardSelected.OnNext((cardType, GetComponent<RectTransform>()));
            }
            else
            {
                //カードが範囲外にある場合の処理
//                Debug.Log("カードが範囲外にあります");
                //カードの位置を元に戻す
                transform.DOMove(DefaultPosition, 0.5f);
            }
            
        }
    }
}