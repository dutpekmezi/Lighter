using UnityEngine;

namespace dutpekmezi
{
    public class CharacterBase : MonoBehaviour
    {
        [Header("Assigned Datas")]
        [SerializeField] private CharacterData characterData; // data container I'm wiring in the inspector

        [Header("Aim Settings")]
        [SerializeField] private float angleOffset = -90f; // my sprite/model faces up by default, so I offset to align with +X math

        [Header("Movement Settings")]
        [SerializeField] private bool useRawInput = true; // I prefer snappy WASD (no smoothing) for a top-down feel


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

            // hook ability listeners once (single-player; no need for runtime clones)
            if (characterData != null && AbilitySystem.Instance.Abilities != null)
            {
                foreach (var ability in AbilitySystem.Instance.Abilities)
                {
                    if (ability == null) continue;
                    ability.Listener(this); // let the ability cache owner / subscribe to events if needed
                    ability.Init(this);     // optional hook
                }
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                UseAbility();
            }

            LookAtMouse(); // keep aiming toward the mouse
        }

        void FixedUpdate()
        {
            Move(); // apply movement in FixedUpdate for consistent physics timing
        }

        public void UseAbility()
        {
            if (characterData == null || AbilitySystem.Instance.Abilities == null) return;

            foreach (var ability in AbilitySystem.Instance.Abilities)
            {
                if (ability == null) continue;
                if (!ability.IsActive) continue;
                if (!ability.CanUse(this)) continue;

                ability.Activate(this); // run the ability logic (e.g., orbiting stars, buffs, etc.)
            }
        }

        private void Move()
        {
            float h = useRawInput ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal"); // horizontal input (-1..1)
            float v = useRawInput ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");   // vertical input (-1..1)
            moveInput = new Vector2(h, v); // compose input vector from X/Y axes
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize(); // normalize so diagonal speed matches cardinal

            float speed = (characterData != null) ? characterData.MoveSpeed : 5f; // safe default if data is missing
            if (rb == null) return;

            rb.linearVelocity = moveInput * speed; // Unity 6+ API; drives crisp top-down motion
        }

        private void LookAtMouse()
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition); // convert screen coords to world
            mouseWorld.z = transform.position.z;                                   // lock Z so I'm aiming on my plane

            Vector3 dir = mouseWorld - transform.position;                         // vector from me to the mouse
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;              // signed angle in degrees on the XY plane

            transform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);    // rotate around Z and add my sprite/model forward offset
        }
    }
}