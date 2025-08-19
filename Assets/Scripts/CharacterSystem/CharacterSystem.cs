using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace dutpekmezi
{
    public class CharacterSystem : MonoBehaviour
    {
        private static CharacterSystem instance;
        public static CharacterSystem Instance => instance;

        private CharacterBase character;

        [Header("Settings")]
        [SerializeField] private List<int> expForLevel = new List<int>();
        public List<int> ExpForLevel => expForLevel;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(instance);
                return;
            }

            instance = this;
        }

        private void Start()
        {
            character = FindAnyObjectByType<CharacterBase>();
        }

        public CharacterBase GetCharacter()
        {
            return character;
        }
    }
}