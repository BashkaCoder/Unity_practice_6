using RPG.Control;
using RPG.Stats;
using System.Collections;
using UnityEngine;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] private WeaponConfig _weapon;
        [SerializeField] private float _respawnTime = 5f;
        [SerializeField] private float _healthToRestore;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Pickup(other.gameObject);
        }

        private void Pickup(GameObject subject)
        {
            if (_weapon) subject.GetComponent<Fighter>().EquipWeapon(_weapon);

            if (_healthToRestore != 0) subject.GetComponent<Health>().Heal(_healthToRestore);

            StartCoroutine(HideForSeconds(_respawnTime));
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            SwitchPickup(false);
            yield return new WaitForSeconds(seconds);
            SwitchPickup(true);
        }

        private void SwitchPickup(bool enabled)
        {
            GetComponent<Collider>().enabled = enabled;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(enabled);
            }
        }

        #region IRaycastable
        public CursorType GetCursorType() => CursorType.Pickup;

        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Pickup(callingController.gameObject);
            }
            return true;
        }
        #endregion
    }
}
