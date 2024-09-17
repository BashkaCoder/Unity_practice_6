using RPG.Saving;
using RPG.Utils;
using UnityEngine;

namespace RPG.Stats
{
    [RequireComponent(typeof(BaseStats))]
    public class Experience : MonoBehaviour, ISaveable
    {
        private float _experiencePoints;
        private LazyValue<float> _experienceToLevelUp;

        public float ExperiencePoints => _experiencePoints;
        public float ExperienceToLevelUp
        {
            get => _experienceToLevelUp.Value;
            private set => _experienceToLevelUp.Value = value;
        }

        public event System.Action OnExperienceGained;

        private void Awake()
        {
            var _baseStats = GetComponent<BaseStats>();
            _experienceToLevelUp = new(() => _baseStats.GetStat(Stat.ExperienceToLevelUp));
        }

        public void GainExperience(float experience)
        {
            _experiencePoints += experience;
            OnExperienceGained();
        }

        public void SetExperienceToLevelUp(float value)
        {
            ExperienceToLevelUp = value;
        }

        #region Saving
        public object CaptureState() => _experiencePoints;

        public void RestoreState(object state) => _experiencePoints = (float)state;
        #endregion
    }
}
