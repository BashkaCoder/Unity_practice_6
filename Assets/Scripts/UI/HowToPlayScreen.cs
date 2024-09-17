using RPG.Saving;
using RPG.SceneManagement;
using UnityEngine;

namespace RPG.UI
{
    public class HowToPlayScreen : MonoBehaviour, ISaveable
    {
        [SerializeField] private bool _isNewGame;

        private bool IsNewGame
        {
            get => _isNewGame;
            set
            {
                _isNewGame = value;
                gameObject.SetActive(_isNewGame);
            }
        }

        private void Start()
        {
            IsNewGame = !SavingWrapper.SaveFileExist();
            transform.GetChild(0).gameObject.SetActive(IsNewGame);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return)) IsNewGame = false;
        }

        #region Saving
        public object CaptureState() => _isNewGame;

        public void RestoreState(object state) => IsNewGame = (bool)state;
        #endregion
    }
}