using dutpekmezi;
using System;
using UnityEngine;

public abstract class AbilityBase : ScriptableObject
{
    [Header("Ability Data")]
    public string AbilityId;
    public string Name;
    public string Description;
    public Sprite Icon;
    public Sprite Background;

    public bool IsActive;

    protected CharacterBase character; // owner reference I keep after Listener/Activate

    public Guid UniqueId { get; } = Guid.NewGuid();
    public string UniqueAbilityId => $"{UniqueId}_{AbilityId}";

    // can I use this ability right now with the given character?
    public abstract bool CanUse(CharacterBase character);

    // allow the ability to cache the owner and subscribe to events if needed
    public virtual void Listener(CharacterBase character)
    {
        this.character = character; // store owner reference
    }

    // do the thing
    public abstract void Activate(CharacterBase character);
}
