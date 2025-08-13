using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace dutpekmezi
{
    [CreateAssetMenu(fileName = "LightBall", menuName = "Game/Scriptable Objects/Ability System/Active/LightBall")]
    public class LightBall : AbilityBase
    {
        [Header("Orbit Config")]
        public GameObject StarPrefab;
        public int InitialCount = 1;
        public float Radius = 1.6f;
        public float AngularSpeedDeg = 120f;
        public bool Clockwise = false;

        [Header("Self Spin")]
        public float SelfSpinSpeedDeg = 360f; // how fast each star spins around its own axis
        public Vector3 SelfSpinAxis = Vector3.forward; // Z for 2D top-down, Y for 3D
        public bool SelfSpinClockwise = false; // flip spin direction if needed

        // runtime state (single player, so keeping it here is OK)
        private readonly List<Transform> stars = new List<Transform>();
        private Coroutine orbitRoutine;
        private float baseAngleDeg; // phase angle for rotation
        private Transform center;   // cached character transform

        public override bool CanUse(CharacterBase character) => IsActive && character != null && StarPrefab != null && InitialCount > 0;

        public override void Listener(CharacterBase character)
        {
            base.Listener(character); // store owner
            center = character ? character.transform : null; // cache center
        }

        public override void Activate(CharacterBase character)
        {
            if (!CanUse(character)) return;

            this.character = character; // keep a valid owner ref for OrbitLoop
            center = character.transform; // ensure fresh center

            if (stars.Count == 0)
            {
                // start phase from character's facing direction
                Vector2 facing = character.transform.right; // 2D top-down: +X as forward
                if (facing.sqrMagnitude <= 0.0001f) facing = Vector2.right;
                baseAngleDeg = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

                EnsureAtLeast(InitialCount); // spawn initial ring (with self spin)
            }
            else
            {
                EnsureAtLeast(InitialCount); // make sure we have at least this many
            }

            if (orbitRoutine == null)
                orbitRoutine = character.StartCoroutine(OrbitLoop()); // run inside ability, hosted by character
        }

        public void AddStars(int amount, CharacterBase character)
        {
            if (amount <= 0 || character == null || StarPrefab == null) return;
            center = character.transform;
            if (orbitRoutine == null) // if ring not running, start it
                orbitRoutine = character.StartCoroutine(OrbitLoop());

            for (int i = 0; i < amount; i++) SpawnOne(); // each new star gets self spin
            Redistribute(); // even spacing after change
        }

        private void EnsureAtLeast(int count)
        {
            while (stars.Count < count) SpawnOne();
            Redistribute();
        }

        private void SpawnOne()
        {
            var go = Object.Instantiate(StarPrefab, Vector3.zero, Quaternion.identity); // detached, world-space
            stars.Add(go.transform);
            StartSelfSpin(go.transform); // spin this star around its own axis forever (DOTween)
        }

        private void StartSelfSpin(Transform t)
        {
            float speed = Mathf.Abs(SelfSpinSpeedDeg);
            if (speed <= 0f || t == null) return;

            float dur = 360f / speed; // time to complete one full rotation
            float sign = SelfSpinClockwise ? -1f : 1f;
            Vector3 step = SelfSpinAxis.normalized * (360f * sign);

            t.DORotate(step, dur, RotateMode.FastBeyond360) // rotate by 'step' each loop
             .SetRelative(true)
             .SetLoops(-1, LoopType.Incremental)
             .SetEase(Ease.Linear)
             .SetLink(t.gameObject); // auto-kill when the star is destroyed
        }

        private void Redistribute()
        {
            if (center == null || stars.Count == 0) return;
            int n = stars.Count;
            for (int i = 0; i < n; i++)
            {
                float ang = baseAngleDeg + (360f / n) * i;
                PositionStar(stars[i], ang);
            }
        }

        private IEnumerator OrbitLoop()
        {
            while (center != null && stars.Count > 0)
            {
                float dir = Clockwise ? -1f : 1f;
                baseAngleDeg += dir * AngularSpeedDeg * Time.deltaTime;

                int n = stars.Count;
                for (int i = 0; i < n; i++)
                {
                    float ang = baseAngleDeg + (360f / n) * i;
                    PositionStar(stars[i], ang);
                }
                yield return null;
            }
            orbitRoutine = null; // stopped (no stars or no center)
        }

        private void PositionStar(Transform star, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 c = center.position;
            Vector2 p = c + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * Radius;
            star.position = p; // world-space; no parenting needed
        }
    }
}