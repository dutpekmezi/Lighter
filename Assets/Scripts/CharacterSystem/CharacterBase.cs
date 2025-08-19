using System;
using UnityEngine;

namespace dutpekmezi
{
    public class CharacterBase : MonoBehaviour
    {
        // Delegate definition for level-up notifications. 
        public delegate void OnLevelUpEvent(CharacterBase character, int newLevel);

        // Event that external systems can subscribe to when this character levels up.
        public event OnLevelUpEvent OnLevelUp;

        [Header("Assigned Datas")]
        [SerializeField] private CharacterData characterData; // data container I'm wiring in the inspector

        [Header("Aim Settings")]
        [SerializeField] private float angleOffset = -90f; // my sprite/model faces up by default, so I offset to align with +X math

        [Header("Movement Settings")]
        [SerializeField] private bool useRawInput = true; // I prefer snappy WASD (no smoothing) for a top-down feel

        [Header("Stats")]
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentLevel = 0;
        [SerializeField] private int currentExp;


        private Camera mainCam;   // cached main camera
        private Rigidbody2D rb;   // I’ll drive velocity for proper collisions
        private Vector2 moveInput; // cached input each frame (normalized so diagonals aren't faster)

        void Awake()
        {
            mainCam = Camera.main;            // grab and cache the main camera once
            TryGetComponent(out rb);          // detect if I'm using Rigidbody2D so I can choose the right movement path
        }

        void Start()
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // start with a clean rotation
            LookAtMouse();                                  // do an initial aim so the character isn't in a weird default pose
        }

        void Update()
        {
            LookAtMouse(); // keep aiming toward the mouse

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GainExp();
            }
        }

        void FixedUpdate()
        {
            Move(); // apply movement in FixedUpdate for consistent physics timing
        }

        public void ActivateAbility(AbilityBase ability)
        {
            if (characterData == null || AbilitySystem.Instance.Abilities == null || AbilitySystem.Instance.GainedAbilities == null) return;


            if (ability == null || !ability.CanUse(this)) return;

            ability.Activate(this); // run the ability logic
        }

        private void Move()
        {
            float h = useRawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal"); // horizontal input (-1..1)
            float v = useRawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");   // vertical input (-1..1)
            moveInput = new Vector2(h, v); // compose input vector from X/Y axes
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize(); // normalize so diagonal speed matches cardinal

            float speed = (characterData != null) ? characterData.MoveSpeed : 5f; // safe default if data is missing
            if (rb == null) return;

            rb.linearVelocity = moveInput * speed;
        }

        private void LookAtMouse()
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition); // convert screen coords to world
            mouseWorld.z = transform.position.z;                                   // lock Z so I'm aiming on my plane

            Vector3 dir = mouseWorld - transform.position;                         // vector from me to the mouse
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;              // signed angle in degrees on the XY plane

            transform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);    // rotate around Z and add my sprite/model forward offset
        }

        public void LevelUp(int i = 1)
        {
            currentLevel += i;
            currentExp = 0;
            OnLevelUp?.Invoke(this, currentLevel); // trigger delegate event with both character context and new level
        }

        public void GainExp(int i = 1)
        {
            currentExp += i;

            if (currentExp >= CharacterSystem.Instance.ExpForLevel[currentLevel])
            {
                LevelUp();
            }
        }
    }
}
