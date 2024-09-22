using RPG.Control;
using RPG.Core;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicControlRemover : MonoBehaviour
    {
        [SerializeField] private GameObject _canvasWithText;
        private GameObject _player;

        private void Awake()
        {
            _player = GameObject.FindWithTag("Player");
        }

        private void OnEnable()
        {
            var playableDirector = GetComponent<PlayableDirector>();
            playableDirector.played += DisableControl;
            playableDirector.stopped += EnableControl;
        }

        private void OnDisable()
        {
            var playableDirector = GetComponent<PlayableDirector>();
            playableDirector.played -= DisableControl;
            playableDirector.stopped -= EnableControl;
        }

        private void DisableControl(PlayableDirector playableDirector)
        {
            _player.GetComponent<ActionScheduler>().CancelCurrentAction();
            _player.GetComponent<PlayerController>().enabled = false;
            _canvasWithText.SetActive(true);
        }

        private void EnableControl(PlayableDirector playableDirector)
        {
            _player.GetComponent<PlayerController>().enabled = true;
            _canvasWithText.SetActive(false);
        }
    }
}