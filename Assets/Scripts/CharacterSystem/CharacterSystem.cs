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

            character = FindAnyObjectByType<CharacterBase>();
        }

        public CharacterBase GetCharacter()
        {
            return character;
        }

        public int GetExpForNextLevel()
        {
            var character = GetCharacter();

            return expForLevel[character.GetLevel()];
        }

        public int GetLevel()
        {
            var character = GetCharacter();

            return character.GetLevel();
        }

        public int GetNextLevel()
        {
            return GetLevel() + 1;
        }

        public int GetExp()
        {
            return GetCharacter().GetExp();
        }
    }
}