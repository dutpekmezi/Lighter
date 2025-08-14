using UnityEngine;

namespace dutpekmezi
{
    public class CharacterSystem : MonoBehaviour
    {
        private static CharacterSystem instance;
        public static CharacterSystem Instance => instance;

        private CharacterBase character;

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