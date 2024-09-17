using RPG.Core;
using RPG.Utils;
using System;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] private CharacterClass _characterClass;
        [SerializeField, Range(1, 99)] private int _startingLevel = 1;
        [SerializeField] private Progression _progression;
        [SerializeField] private DestroyAfterEffect _levelUpParticleEffect;
        [SerializeField] private bool _shouldUseModifiers;
        [SerializeField] private AudioSource _levelUpSound;
        private Experience _experience;
        private LazyValue<int> _currentLevel;

        public int CurrentLevel
        {
            get => _currentLevel.Value;
            private set => _currentLevel.Value = value;
        }

        public event System.Action OnLevelUp;

        private void Awake()
        {
            _experience = GetComponent<Experience>();
            _currentLevel = new(CalculateLevel);
        }

        private void OnEnable()
        {
            if (_experience) _experience.OnExperienceGained += UpdateLevel;
        }

        private void OnDisable()
        {
            if (_experience) _experience.OnExperienceGained -= UpdateLevel;
        }

        private void Start()
        {
            _currentLevel.ForceInit();
        }

        private int CalculateLevel()
        {
            if (!_experience) return _startingLevel;

            float currentXP = _experience.ExperiencePoints;
            int penultimateLevel = _progression.GetLevelsCount(Stat.ExperienceToLevelUp, _characterClass);
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float XPToLevelUp = _progression.GetStat(Stat.ExperienceToLevelUp, _characterClass, level);
                if (XPToLevelUp > currentXP)
                {
                    _experience.SetExperienceToLevelUp(XPToLevelUp);
                    return level;
                }
            }
            return penultimateLevel + 1;
        }

        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > CurrentLevel)
            {
                CurrentLevel = newLevel;
                OnLevelUp();
                PlayLevelUpEffect();
                PlayLevelUpSound();
            }
        }

        private void PlayLevelUpEffect() => Instantiate(_levelUpParticleEffect, transform);

        private void PlayLevelUpSound() => _levelUpSound.Play();

        public float GetStat(Stat stat) => !_shouldUseModifiers ? GetBaseStat(stat) :
            (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat) / 100);

        private float GetBaseStat(Stat stat) => _progression.GetStat(stat, _characterClass, CurrentLevel);

        private float GetAdditiveModifier(Stat stat)
        {
            float totalValue = 0;
            foreach (var provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    totalValue += modifier;
                }
            }
            return totalValue;
        }

        private float GetPercentageModifier(Stat stat)
        {
            float totalPercentage = 0;
            foreach (var provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    totalPercentage += modifier;
                }
            }
            return totalPercentage;
        }
    }
}
