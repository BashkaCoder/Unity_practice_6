using RPG.Stats;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI
{
    public class ExperienceBar : MonoBehaviour
    {
        [SerializeField] private Experience _ownerExperience;
        [SerializeField] private Image _fill;
        [SerializeField] private TMP_Text _experienceText;
        private Coroutine _currentCoroutine;

        private void OnEnable()
        {
            _ownerExperience.OnExperienceGained += UpdateBar;
            _ownerExperience.GetComponent<BaseStats>().OnLevelUp += UpdateBar;
        }

        private void OnDisable()
        {
            _ownerExperience.OnExperienceGained -= UpdateBar;
            _ownerExperience.GetComponent<BaseStats>().OnLevelUp -= UpdateBar;
        }

        private void Start()
        {
            UpdateBar();
            SetBarImageFill(_ownerExperience.ExperiencePoints / _ownerExperience.ExperienceToLevelUp, true);
        }

        private void UpdateBar()
        {
            SetBarText(_ownerExperience.ExperiencePoints, _ownerExperience.ExperienceToLevelUp);
            SetBarImageFill(_ownerExperience.ExperiencePoints / _ownerExperience.ExperienceToLevelUp);
        }

        private void SetBarText(float currentValue, float maxValue)
        {
            _experienceText.text = $"XP {currentValue} / {maxValue}";
        }

        private void SetBarImageFill(float newValue, bool instantly = false)
        {
            if (instantly)
            {
                _fill.fillAmount = newValue;
            }
            else
            {
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(FillBarSmoothlyCoroutine(_fill, newValue));
            }
        }

        private static IEnumerator FillBarSmoothlyCoroutine(Image bar, float value)
        {
            float elapsedTime = 0;
            const float fillingRate = 0.5f;
            while (bar.fillAmount <= value)
            {
                elapsedTime += Time.deltaTime * fillingRate;
                bar.fillAmount = Mathf.Lerp(bar.fillAmount, value, elapsedTime);
                yield return null;
            }
            bar.fillAmount = value;
        }
    }
}