using RPG.Saving;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour, ISaveable
    {
        private bool _alreadyTriggered = false;
        private PlayableDirector _playableDirector;

        private void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_alreadyTriggered || !other.CompareTag("Player")) return;

            _alreadyTriggered = true;
            _playableDirector.Play();
        }

        private void Update()
        {
            if (_playableDirector && _alreadyTriggered && Input.GetKeyUp(KeyCode.Return))
            {
                _playableDirector.Stop();
                _playableDirector = null;
            }
        }

        #region Saving
        public object CaptureState() => _alreadyTriggered;

        public void RestoreState(object state) => _alreadyTriggered = (bool)state;
        #endregion
    }
}
