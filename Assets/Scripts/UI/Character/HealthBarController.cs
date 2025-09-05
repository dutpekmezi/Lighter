using UnityEngine;
using UnityEngine.UI;

namespace dutpekmezi
{
    public class HealthBarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider slider;

        private CharacterBase character;

        private void Start()
        {
            character = CharacterSystem.Instance.GetCharacter();

            Init();
        }

        private void Init()
        {
            if (character != null)
            {
                character.OnExpGained += UpdateSlider;
            }
        }

        private void UpdateSlider(CharacterBase character, int currentExp)
        {
            if (slider != null)
            {
                slider.minValue = 0;
                slider.maxValue = CharacterSystem.Instance.GetExpForNextLevel();

                slider.value = CharacterSystem.Instance.GetExp();
            }
        }

        private void OnDisable()
        {
            if (character != null)
            {
                character.OnExpGained -= UpdateSlider;
            }
        }
    }
}