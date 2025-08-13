using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace dutpekmezi
{
    public class AbilitySystem : MonoBehaviour
    {
        [SerializeField] private List<AbilityBase> abilities;

        public List<AbilityBase> Abilities => abilities;

        private static AbilitySystem instance;

        public static AbilitySystem Instance => instance;

        private void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(instance);
            }

            instance = this;
        }

        public AbilityBase GainAbility(AbilityBase ability)
        {
            if (abilities.Contains(ability)) return null;

            abilities.Add(ability);
            return ability;
        }
    }
}