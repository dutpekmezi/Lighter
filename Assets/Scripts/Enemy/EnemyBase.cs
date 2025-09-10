using UnityEngine;

namespace dutpekmezi
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Assigned Data")]
        [SerializeField] private EnemyData enemyData;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;

        private Transform targetTransform;

        private void Start()
        {
            targetTransform = CharacterSystem.Instance.GetCharacter().transform;
        }

        private void FixedUpdate()
        {
            if (targetTransform == null) return;

            // get dir to target
            Vector2 dir = (targetTransform.position - transform.position).normalized;

            // move with rb
            rb.MovePosition(rb.position + dir * enemyData.MoveSpeed * Time.fixedDeltaTime);
        }
    }
}
