using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 1)]
    public class Progression : ScriptableObject
    {
        [SerializeField] private ProgressionCharacterClass[] _characterClasses;

        private Dictionary<CharacterClass, Dictionary<Stat, float[]>> _lookupTable;

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            BuildLookupTable();

            float[] levels = _lookupTable[characterClass][stat];
            if (levels.Length < level) return 0;

            return levels[level - 1];
        }

        private void BuildLookupTable()
        {
            if (_lookupTable != null) return;

            _lookupTable = new();

            foreach (var progressionClass in _characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, float[]>();

                foreach (var progressionStat in progressionClass.Stats)
                {
                    statLookupTable[progressionStat.Stat] = progressionStat.Levels;
                }

                _lookupTable[progressionClass.CharacterClass] = statLookupTable;
            }
        }

        public int GetLevelsCount(Stat stat, CharacterClass characterClass)
        {
            BuildLookupTable();
            float[] levels = _lookupTable[characterClass][stat];
            return levels.Length;
        }

        [System.Serializable]
        private class ProgressionCharacterClass
        {
            public CharacterClass CharacterClass;
            public ProgressionStat[] Stats;
        }

        [System.Serializable]
        private class ProgressionStat
        {
            public Stat Stat;
            public float[] Levels;
        }
    }
}
