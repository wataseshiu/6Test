using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hand
{
    public class HitAreaVisualSwitcher : MonoBehaviour
    {
        [SerializeField] private Image hitAreaImage;
        [SerializeField]private BoxCollider2D hitAreaCollider;

        public void SetHitAreaVisualize(bool isOn)
        {
            hitAreaImage.enabled = isOn;
        }
        
        public void SetHitAreaColliderActive(bool isActive)
        {
            hitAreaCollider.enabled = isActive;
        }
    }
}