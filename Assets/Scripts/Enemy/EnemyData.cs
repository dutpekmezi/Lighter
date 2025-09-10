using UnityEngine;

namespace dutpekmezi
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Scriptable Objects/Enemy/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public string Id;
        public string Name;

        public float MoveSpeed;

        public int MaxHealth;

        public EnemyBase Prefab;
    }
}