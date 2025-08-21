using dutpekmezi;
using System;
using UnityEngine;

namespace dutpekmezi
{
    public abstract class AbilityBase : ScriptableObject
    {
        [Header("Ability Data")]
        public string AbilityId;
        public string Name;
        public string Description;
        public Sprite Icon;
        public Sprite Background;

        [Header("Settings")]
        public int MaxCount;
        public int CurrentCount;
        public float Speed;
        public float Damage;

        public float baseScaleAmount = 1f;
        public float scaleMultiplier = 1f;

        [Tooltip("If true, this is an active ability with gameplay effect. If false, it's a passive upgrade.")]
        public bool IsActualAbility = true;

        protected CharacterBase character; // cached owner after Listener/Activate

        public Guid UniqueId { get; } = Guid.NewGuid();
        public string UniqueAbilityId => $"{UniqueId}_{AbilityId}";

        // can this ability be used at the moment?
        public abstract bool CanUse(CharacterBase character);

        // cache character reference or subscribe to events
        public virtual void Listener(CharacterBase character)
        {
            this.character = character;
        }

        // main execution of the ability
        public abstract void Activate(CharacterBase character);
    }
}