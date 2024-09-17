using RPG.Core;
using RPG.Movement;
using RPG.Saving;
using RPG.Stats;
using RPG.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        #region Fields and Properties
        [SerializeField] private float _timeBetweenAttacks = 1f;
        private float _timeSinceLastAttack = Mathf.Infinity;
        private Animator _animator;
        private Mover _mover;
        private Health _target;

        [Header("Weapons")]
        [SerializeField] private WeaponConfig _defaultWeapon;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;
        private WeaponConfig _currentWeaponConfig;
        private LazyValue<Weapon> _currentWeapon;

        private Weapon CurrentWeapon
        {
            get => _currentWeapon.Value;
            set => _currentWeapon.Value = value;
        }
        #endregion

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _mover = GetComponent<Mover>();
            _currentWeaponConfig = _defaultWeapon;
            _currentWeapon = new(SetupDefaultWeapon);
        }

        private void Start()
        {
            _currentWeapon.ForceInit();
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;

            if (!_target || _target.IsDead) return;

            if (GetIsInAttackRange(_target.transform))
            {
                _mover.Cancel();
                AttackBehaviour();
            }
            else
            {
                _mover.MoveTo(_target.transform.position);
            }
        }

        #region Weapons
        private Weapon SetupDefaultWeapon() => AttachWeapon(_defaultWeapon);

        private Weapon AttachWeapon(WeaponConfig weapon) =>
            weapon.Spawn(_leftHandTransform, _rightHandTransform, _animator);

        public void EquipWeapon(WeaponConfig weapon)
        {
            _currentWeaponConfig = weapon;
            CurrentWeapon = AttachWeapon(weapon);
        }

        private WeaponConfig LoadWeapon(string weaponName) => Resources.Load<WeaponConfig>(weaponName);
        #endregion

        private bool GetIsInAttackRange(Transform target) =>
            Vector3.Distance(transform.position, target.position) < _currentWeaponConfig.AttackRange;

        private void AttackBehaviour()
        {
            transform.LookAt(_target.transform);

            if (_timeSinceLastAttack > _timeBetweenAttacks)
            {
                // Запускает Hit() Animation Event
                TriggerAttack();
                _timeSinceLastAttack = 0;
            }
        }

        private void TriggerAttack()
        {
            const string TriggerName1 = "StopAttack";
            _animator.ResetTrigger(TriggerName1);

            const string TriggerName2 = "Attack";
            _animator.SetTrigger(TriggerName2);
        }

        #region Animation Events
        private void Hit()
        {
            if (!_target) return;

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            if (CurrentWeapon)
            {
                CurrentWeapon.OnHit();
            }

            if (_currentWeaponConfig.HasProjectTile)
            {
                _currentWeaponConfig.LaunchProjecttile(
                    _leftHandTransform, _rightHandTransform, _target, gameObject, damage);
            }
            else
            {
                _target.TakeDamage(gameObject, damage);
            }
        }

        private void Shoot() => Hit();
        #endregion

        public bool CanAttack(GameObject combatTarget)
        {
            if (!combatTarget ||
                (!_mover.CanMoveTo(combatTarget.transform.position) &&
                !GetIsInAttackRange(combatTarget.transform)))
                return false;

            var target = combatTarget.GetComponent<Health>();
            return combatTarget && target && !target.IsDead;
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            _target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            _target = null;
            _mover.Cancel();
        }

        private void StopAttack()
        {
            const string TriggerName1 = "Attack";
            _animator.ResetTrigger(TriggerName1);

            const string TriggerName2 = "StopAttack";
            _animator.SetTrigger(TriggerName2);
        }

        #region IModifierProvider
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return _currentWeaponConfig.Damage;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return _currentWeaponConfig.PercentageDamageBonus;
            }
        }
        #endregion

        #region ISaveable
        public object CaptureState() => _currentWeaponConfig.name;

        public void RestoreState(object state)
        {
            var weaponName = (string)state;
            EquipWeapon(LoadWeapon(weaponName));
        }
        #endregion
    }
}
