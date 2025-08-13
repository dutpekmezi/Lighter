using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dutpekmezi
{
    public class AbilityCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Assigned Data")]
        public AbilityBase ability;
        public void Init(AbilityBase ability)
        {
            this.ability = ability;

            iconImage.sprite = ability.Icon;

            nameText.text = ability.Name;
            descriptionText.text = ability.Description;
        }
    }
}
