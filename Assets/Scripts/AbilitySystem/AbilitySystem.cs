using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dutpekmezi
{
    public class AbilitySystem : MonoBehaviour
    {
        [SerializeField] private List<AbilityBase> abilities;       // master pool
        [SerializeField] private List<AbilityBase> gainedAbilities; // acquired

        public List<AbilityBase> Abilities => abilities;
        public List<AbilityBase> GainedAbilities => gainedAbilities;

        private static AbilitySystem instance;
        public static AbilitySystem Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            if (abilities == null) abilities = new List<AbilityBase>();
            if (gainedAbilities == null) gainedAbilities = new List<AbilityBase>();
        }

        public bool HasAbility(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId)) return false;
            return gainedAbilities.Any(a => a != null && a.AbilityId == abilityId);
        }

        public AbilityBase GetAbilityById(string abilityId, bool searchGainedFirst = true)
        {
            if (string.IsNullOrEmpty(abilityId)) return null;

            AbilityBase found = null;
            if (searchGainedFirst)
                found = gainedAbilities.FirstOrDefault(a => a != null && a.AbilityId == abilityId);

            if (found == null)
                found = abilities.FirstOrDefault(a => a != null && a.AbilityId == abilityId);

            return found;
        }

        public AbilityBase GainAbilityById(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId)) { Debug.LogWarning("GainAbilityById: empty id"); return null; }

            var ability = abilities.FirstOrDefault(a => a != null && a.AbilityId == abilityId);
            if (ability == null) { Debug.LogWarning($"GainAbilityById: '{abilityId}' not in Abilities"); return null; }
            if (gainedAbilities.Any(a => a != null && a.AbilityId == abilityId)) return null; // already owned

            gainedAbilities.Add(ability);
            ActivateAbility(ability); // run immediately if active-type
            return ability;
        }

        public AbilityBase GainAbility(AbilityBase ability)
        {
            if (ability == null) return null;
            if (!abilities.Contains(ability)) { Debug.LogWarning("GainAbility: ability not in Abilities list"); return null; }
            if (gainedAbilities.Any(a => a != null && a.AbilityId == ability.AbilityId)) return null; // already owned by id

            gainedAbilities.Add(ability);
            ActivateAbility(ability);
            return ability;
        }

        public AbilityBase ActivateAbilityById(string abilityId)
        {
            var ability = GetAbilityById(abilityId, searchGainedFirst: true);
            if (ability == null) { Debug.LogWarning($"ActivateAbilityById: '{abilityId}' not found"); return null; }
            return ActivateAbility(ability);
        }

        public AbilityBase ActivateAbility(AbilityBase ability)
        {
            var character = CharacterSystem.Instance != null ? CharacterSystem.Instance.GetCharacter() : null;
            if (character == null) { Debug.LogError("ActivateAbility: Character not found"); return ability; }

            ability.Listener(character); // let ability cache owner / subscribe
            if (ability.IsActive && ability.CanUse(character))
                ability.Activate(character); // active abilities start immediately; passives usually start via Listener

            return ability;
        }
    }
}
