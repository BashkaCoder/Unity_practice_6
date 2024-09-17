using UnityEngine;

namespace RPG.UI.Menu
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _options;

        private void Start()
        {
            HidePauseMenu();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (_options && _options.activeSelf)
                {
                    _options.SetActive(false);
                    return;
                }
                SwitchPauseMenu();
            }
        }

        private void HidePauseMenu()
        {
            var blackBg = transform.GetChild(0).gameObject;
            blackBg.SetActive(false);
        }

        public void SwitchPauseMenu()
        {
            var blackBg = transform.GetChild(0).gameObject;
            bool enabled = !blackBg.activeSelf;
            blackBg.SetActive(enabled);
        }
    }
}
