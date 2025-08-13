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
        public GameObject abilityPrefab;
        public int initialCount = 1;
        public float radius = 1.6f;
        public float angularSpeedDeg = 120f;
        public bool clockwise = false;

        [Header("Self Spin")]
        public float selfSpinSpeedDeg = 360f; // how fast each star spins around its own axis
        public Vector3 selfSpinAxis = Vector3.forward; // Z for 2D top-down, Y for 3D
        public bool selfSpinClockwise = false; // flip spin direction if needed

        // runtime state (single player, so keeping it here is OK)
        private readonly List<Transform> stars = new List<Transform>();
        private Coroutine orbitRoutine;
        private float baseAngleDeg; // phase angle for rotation
        private Transform center;   // cached character transform

        public override bool CanUse(CharacterBase character) => IsActive && character != null && abilityPrefab != null && initialCount > 0;

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

                EnsureAtLeast(initialCount); // spawn initial ring (with self spin)
            }
            else
            {
                EnsureAtLeast(initialCount); // make sure we have at least this many
            }

            if (orbitRoutine == null)
                orbitRoutine = character.StartCoroutine(OrbitLoop()); // run inside ability, hosted by character
        }

        public void AddStars(int amount, CharacterBase character)
        {
            if (amount <= 0 || character == null || abilityPrefab == null) return;
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
            var instance = Object.Instantiate(abilityPrefab, Vector3.zero, Quaternion.identity); // detached, world-space
            stars.Add(instance.transform);
            StartSelfSpin(instance.transform); // spin this star around its own axis forever (DOTween)
        }

        private void StartSelfSpin(Transform t)
        {
            float speed = Mathf.Abs(selfSpinSpeedDeg);
            if (speed <= 0f || t == null) return;

            float dur = 360f / speed; // time to complete one full rotation
            float sign = selfSpinClockwise ? -1f : 1f;
            Vector3 step = selfSpinAxis.normalized * (360f * sign);

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
                float dir = clockwise ? -1f : 1f;
                baseAngleDeg += dir * angularSpeedDeg * Time.deltaTime;

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
            Vector2 p = c + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            star.position = p; // world-space; no parenting needed
        }
    }
}