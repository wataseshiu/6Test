using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class DialogManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> dialogObjects = new List<GameObject>();
        
        public GameObject CreateDialog<T>(Dialog dialogType, T param)
        {
            var index = (int)dialogType;
            var dialog = Instantiate(dialogObjects[index], transform);
            dialog.GetComponent<IDialogObjectBase<T>>().param = param;
            return dialog;
        }

        public void DestroyDialog(GameObject dialog)
        {
            Destroy(dialog);
        }
    }

    public enum Dialog
    {
        Result,
        TitleMenu
    }
}
