using UnityEngine;

namespace dutpekmezi
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Scriptable Objects/Character System/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        public string Id;
        public string Name;

        public int MaxHealth;
        public int MaxLevel;
        public float MoveSpeed;

        public CharacterBase Prefab;
    }
}