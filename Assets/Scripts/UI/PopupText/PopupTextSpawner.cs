using UnityEngine;

namespace RPG.UI.PopupText
{
    public class PopupTextSpawner : MonoBehaviour
    {
        [SerializeField] private PopupText _popupTextPrefab;

        public void SpawnPopupText(float value)
        {
            PopupText popupText = Instantiate(_popupTextPrefab, transform);
            popupText.SetTextValue(value);
        }
    }
}
