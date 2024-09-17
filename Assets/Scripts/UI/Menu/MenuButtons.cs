using RPG.SceneManagement;
using RPG.Stats;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG.UI.Menu
{
    public class MenuButtons : MonoBehaviour
    {
        [SerializeField] private MenuButton[] _menuButtons;

        [System.Serializable]
        private struct MenuButton
        {
            [SerializeField] private MenuButtonType _type;
            [SerializeField] private Button _button;
            public MenuButtonType Type => _type;
            public Button Button => _button;
        }

        private enum MenuButtonType
        {
            NewGame,
            Continue,
            ContinuePrevGame,
            Exit,
            ExitToMainMenu,
            Options,
        }

        private void OnEnable()
        {
            foreach (var button in _menuButtons)
            {
                button.Button.onClick.AddListener(GetButtonAction(button.Type));
            }
        }

        private UnityEngine.Events.UnityAction GetButtonAction(MenuButtonType buttonType) =>
            buttonType switch
            {
                MenuButtonType.NewGame => NewGame,
                MenuButtonType.Continue => Continue,
                MenuButtonType.ContinuePrevGame => ContinuePrevGame,
                MenuButtonType.Exit => Exit,
                MenuButtonType.ExitToMainMenu => ExitToMainMenu,
                MenuButtonType.Options => null,
                _ => null,
            };

        private void OnDisable()
        {
            foreach (var button in _menuButtons)
            {
                button.Button.onClick.RemoveAllListeners();
            }
        }

        private void Start()
        {
            InitializeContinueButton();
        }

        private void InitializeContinueButton()
        {
            foreach (var button in _menuButtons)
            {
                if (button.Type != MenuButtonType.ContinuePrevGame) continue;
                button.Button.interactable = SavingWrapper.SaveFileExist();
            }
        }

        private void NewGame() => StartCoroutine(NewGameCoroutine());

        private IEnumerator NewGameCoroutine()
        {
            SavingWrapper.Delete();
            DontDestroyOnLoad(gameObject);

            yield return Fader.Instance.FadeOut(Fader.FadeOutTime);
            yield return SceneManager.LoadSceneAsync((int)GameScene.Village);
            yield return Fader.Instance.FadeIn(Fader.FadeInTime);
            SavingWrapper.Save();

            Destroy(gameObject);
        }

        private void Continue() => GetComponent<PauseMenu>().SwitchPauseMenu();

        private void ContinuePrevGame() => StartCoroutine(ContinueCoroutine());

        private IEnumerator ContinueCoroutine()
        {
            DontDestroyOnLoad(gameObject);

            yield return Fader.Instance.FadeOut(Fader.FadeOutTime);
            yield return SavingWrapper.LoadLastScene();

            Destroy(gameObject);
        }

        private void Exit() => Application.Quit();

        private void ExitToMainMenu() => StartCoroutine(ExitToMainMenuCoroutine());

        private IEnumerator ExitToMainMenuCoroutine()
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);

            if (!GameObject.FindWithTag("Player").GetComponent<Health>().IsDead)
            {
                SavingWrapper.Save();
            }

            yield return Fader.Instance.FadeOut(Fader.FadeOutTime);
            yield return SceneManager.LoadSceneAsync((int)GameScene.MainMenu);
            yield return Fader.Instance.FadeIn(Fader.FadeInTime);

            Destroy(gameObject);
        }
    }
}