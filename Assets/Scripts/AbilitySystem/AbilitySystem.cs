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

        [SerializeField] private int maxAbilityCountPerLevel = 3;
        public int MaxAbilityCountPerLevel => maxAbilityCountPerLevel;

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

            ability.Listener(character);
            if (ability.IsActive && ability.CanUse(character))
                ability.Activate(character);

            return ability;
        }

        public List<string> GetRandomAbilities(bool excludeGained = true)
        {
            if (abilities == null || abilities.Count == 0) return new List<string>();

            var seenIds = new HashSet<string>();
            var pool = new List<AbilityBase>();
            foreach (var a in abilities)
            {
                if (a == null) continue;
                if (string.IsNullOrEmpty(a.AbilityId)) continue;
                if (excludeGained && gainedAbilities.Any(g => g != null && g.AbilityId == a.AbilityId)) continue;
                if (seenIds.Add(a.AbilityId)) pool.Add(a);
            }

            if (pool.Count == 0) return new List<string>();

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            int take = Mathf.Clamp(maxAbilityCountPerLevel, 0, pool.Count);
            return pool.GetRange(0, take).Select(a => a.AbilityId).ToList();
        }
    }
}
