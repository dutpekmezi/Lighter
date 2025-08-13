using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

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
}
