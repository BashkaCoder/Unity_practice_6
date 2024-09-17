using RPG.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private bool _isHoming;
        [SerializeField] private GameObject _hitEffect;
        [SerializeField] private GameObject[] _destroyOnHit;
        [SerializeField] private UnityEvent _onHit;
        private Health _target;
        private float _damage;
        private GameObject _instigator;
        private const float LifeTime = 5f;

        private void Start()
        {
            AimAtTarget(_target.transform);
        }

        private void AimAtTarget(Transform target)
        {
            transform.LookAt(GetAimLocation(target));
        }

        private Vector3 GetAimLocation(Transform target)
        {
            var targetCapsule = target.GetComponent<CapsuleCollider>();
            if (!targetCapsule) return target.position;

            return target.position + Vector3.up * targetCapsule.height / 2;
        }

        private void Update()
        {
            if (!_target) return;

            if (_isHoming && !_target.IsDead) AimAtTarget(_target.transform);
            transform.Translate(_speed * Time.deltaTime * Vector3.forward);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != _target || _target.IsDead) return;

            _target.TakeDamage(_instigator, _damage);
            _onHit.Invoke();
            if (_hitEffect) SpawnHitEffect();
            DestroyOnHit();
            _target = null;
        }

        private void SpawnHitEffect()
        {
            GameObject hitEffect = Instantiate(
                _hitEffect, GetAimLocation(_target.transform), transform.rotation);
        }

        private void DestroyOnHit()
        {
            foreach (GameObject toDestroy in _destroyOnHit)
            {
                toDestroy.SetActive(false);
            }
        }

        public void SetTarget(Health target, GameObject instigator, float damage)
        {
            _target = target;
            _instigator = instigator;
            _damage = damage;

            Destroy(gameObject, LifeTime);
        }
    }
}
