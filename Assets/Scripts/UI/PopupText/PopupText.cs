using TMPro;
using UnityEngine;

namespace RPG.UI.PopupText
{
    public class PopupText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _popupText;

        public void SetTextValue(float value)
        {
            _popupText.color = value > 0 ? Color.green : Color.red;
            _popupText.text = $"{(int)Mathf.Abs(value)}";
        }

        public void DestroyText()
        {
            Destroy(gameObject);
        }
    }
}
