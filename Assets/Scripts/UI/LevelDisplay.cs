using RPG.Stats;
using TMPro;
using UnityEngine;

namespace RPG.UI
{
    public class LevelDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;

        private BaseStats _playerStats;

        private void Awake()
        {
            _playerStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }

        private void OnEnable()
        {
            _playerStats.OnLevelUp += UpdateLevel;
        }

        private void OnDisable()
        {
            _playerStats.OnLevelUp -= UpdateLevel;
        }

        private void Start()
        {
            UpdateLevel();
        }

        private void UpdateLevel()
        {
            _levelText.text = $"{_playerStats.CurrentLevel}";
        }
    }
}