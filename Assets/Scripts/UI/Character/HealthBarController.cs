using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace dutpekmezi
{
    public class HealthBarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider slider;

        [Header("Smooth Settings")]
        [SerializeField] private float tweenDuration = 0.5f;

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

                slider.minValue = 0;
                slider.maxValue = CharacterSystem.Instance.GetExpForNextLevel();
                slider.value = CharacterSystem.Instance.GetExp();
            }
        }

        private void UpdateSlider(CharacterBase character, int currentExp)
        {
            DOTween.Kill(slider);

            if (slider != null)
            {
                slider.minValue = 0;
                slider.maxValue = CharacterSystem.Instance.GetExpForNextLevel();

                int targetExp = CharacterSystem.Instance.GetExp();

                slider.DOValue(targetExp, tweenDuration).SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        DOTween.Kill(slider);
                    });
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
