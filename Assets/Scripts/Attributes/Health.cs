using RPG.Core;
using RPG.Saving;
using RPG.SceneManagement;
using RPG.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Stats
{
    [RequireComponent(typeof(BaseStats))]
    public class Health : MonoBehaviour, ISaveable
    {
        #region Fields and Properties
        [SerializeField, Range(0, 100)] private int _regenerationPercentage;
        [SerializeField] private UnityEvent<float> _onTakeDamage;
        [SerializeField] private UnityEvent<float> _onHeal;
        [SerializeField] private UnityEvent _onDie;
        private BaseStats _baseStats;
        private LazyValue<float> _healthPoints;

        public float HealthPoints
        {
            get => _healthPoints.Value;
            private set => _healthPoints.Value = value;
        }
        public float MaxHealthPoints => _baseStats.GetStat(Stat.Health);
        public bool IsDead { get; private set; }
        #endregion

        private void Awake()
        {
            _baseStats = GetComponent<BaseStats>();
            _healthPoints = new(() => _baseStats.GetStat(Stat.Health));
        }

        private void OnEnable()
        {
            if (_baseStats) _baseStats.OnLevelUp += RegenerateHealth;
        }

        private void OnDisable()
        {
            if (_baseStats) _baseStats.OnLevelUp -= RegenerateHealth;
        }

        private void Start()
        {
            _healthPoints.ForceInit();
        }

        private void RegenerateHealth()
        {
            float regenHP = _baseStats.GetStat(Stat.Health) * _regenerationPercentage / 100;
            HealthPoints = Mathf.Max(HealthPoints, regenHP);
        }

        public void TakeDamage(GameObject instigator, float damage)
        {
            HealthPoints = Mathf.Max(HealthPoints - damage, 0);

            if (HealthPoints == 0)
            {
                _onDie.Invoke();
                Die();
                AwardExperience(instigator);
            }
            else
            {
                _onTakeDamage.Invoke(-damage);
            }
        }

        private void Die()
        {
            if (IsDead) return;

            IsDead = true;
            const string TriggerName = "Die";
            GetComponent<Animator>().SetTrigger(TriggerName);
            GetComponent<ActionScheduler>().CancelCurrentAction();

            SavingWrapper.Delete();
        }

        private void AwardExperience(GameObject instigator)
        {
            var experience = instigator.GetComponent<Experience>();
            if (!experience) return;

            experience.GainExperience(_baseStats.GetStat(Stat.ExperienceReward));
        }

        public void Heal(float healthToRestore)
        {
            HealthPoints = Mathf.Min(HealthPoints + healthToRestore, MaxHealthPoints);
            _onHeal.Invoke(healthToRestore);
        }

        #region ISaveable
        public object CaptureState() => HealthPoints;

        public void RestoreState(object state)
        {
            HealthPoints = (float)state;
            if (HealthPoints == 0) Die();
        }
        #endregion
    }
}
