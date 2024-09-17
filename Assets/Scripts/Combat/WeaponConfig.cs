using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        [SerializeField] private Weapon _equippedPrefab;
        [SerializeField] private AnimatorOverrideController _animatorOverrideController;
        [SerializeField] private bool _isRightHanded = true;
        [SerializeField] private Projectile _projectilePrefab;

        [Header("Weapon Parameters")]
        [SerializeField] private float _damage;
        [SerializeField] private float _percentageDamageBonus;
        [SerializeField] private float _attackRange;

        private const string WeaponName = "Weapon";

        public float Damage => _damage;
        public float PercentageDamageBonus => _percentageDamageBonus;
        public float AttackRange => _attackRange;

        public bool HasProjectTile => _projectilePrefab != null;

        public Weapon Spawn(Transform leftHand, Transform rightHand, Animator animator)
        {
            DestroyPreviousWeapon(leftHand, rightHand);

            Weapon weapon = null;
            if (_equippedPrefab)
            {
                Transform handTransform = _isRightHanded ? rightHand : leftHand;
                weapon = Instantiate(_equippedPrefab, handTransform);
                weapon.gameObject.name = WeaponName;
            }

            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (_animatorOverrideController)
            {
                animator.runtimeAnimatorController = _animatorOverrideController;
            }
            else if (overrideController)
            {
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }
            return weapon;
        }

        private void DestroyPreviousWeapon(Transform leftHand, Transform rightHand)
        {
            Transform previousWeapon = rightHand.Find(WeaponName) == null ?
                leftHand.Find(WeaponName) : rightHand.Find(WeaponName);
            if (!previousWeapon) return;

            Destroy(previousWeapon.gameObject);
        }

        public void LaunchProjecttile(Transform leftHand, Transform rightHand,
            Health target, GameObject instigator, float calculatedDamage)
        {
            Transform handTransform = _isRightHanded ? rightHand : leftHand;
            var projectileInstance = Instantiate(_projectilePrefab, handTransform.position, Quaternion.identity);
            projectileInstance.SetTarget(target, instigator, calculatedDamage);
        }
    }
}
