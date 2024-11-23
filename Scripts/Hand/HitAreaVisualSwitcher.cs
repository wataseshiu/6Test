using UnityEngine;
using UnityEngine.UI;

namespace Hand
{
    public class HitAreaVisualSwitcher : MonoBehaviour
    {
        [SerializeField] private Image hitAreaImage;
        
        public void SetHitAreaVisual(bool isOn)
        {
            hitAreaImage.enabled = isOn;
        }
    }
}