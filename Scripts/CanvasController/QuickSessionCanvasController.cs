using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace CanvasController
{
    public class QuickSessionCanvasController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI infomationText;
        [SerializeField] private RectTransform dialog;

        public void ShowSessionDialog()
        {
            //dialogを表示
            dialog.gameObject.SetActive(true);
            infomationText.text = "create session...";
        }
    }
}