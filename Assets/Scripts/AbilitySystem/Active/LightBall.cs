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
        public float selfSpinSpeedDeg = 360f;           // local spin speed per star (deg/s)
        public Vector3 selfSpinAxis = Vector3.forward;  // Z axis in 2D setups
        public bool selfSpinClockwise = false;

        [Header("Pulse")]
        public float pulseScale = 1.25f;                // how big the breathing gets
        public float pulseDuration = 0.35f;             // one leg of the yoyo
        public Ease pulseEase = Ease.InOutSine;         // simple and readable

        private List<Transform> stars = new List<Transform>();
        private Coroutine orbitRoutine;
        private float baseAngleDeg;
        private Transform center;

        // keep last seen values so I can react when upgrades tweak fields at runtime
        private float lastSpeed = -1f;
        private int lastCount = -1;

        public override bool CanUse(CharacterBase character)
            => character != null && abilityPrefab != null && (initialCount > 0 || CurrentCount > 0);

        public override void Listener(CharacterBase character)
        {
            base.Listener(character);
            center = character ? character.transform : null; // cache owner transform
        }

        public override void Activate(CharacterBase character)
        {
            if (!CanUse(character)) return;

            this.character = character;
            center = character.transform;

            // map generic Speed to our orbit speed (fallback to existing value if not set)
            angularSpeedDeg = Mathf.Max(1f, Speed > 0f ? Speed : angularSpeedDeg);
            selfSpinSpeedDeg = Mathf.Max(1f, selfSpinSpeedDeg);

            // use CurrentCount if provided, else fallback to initialCount; clamp to MaxCount if defined
            int targetCount = Mathf.Max(CurrentCount, initialCount);
            CurrentCount = ClampToMax(targetCount);

            // capture a baseline facing once, so the ring has a stable starting phase
            if (stars.Count == 0)
            {
                Vector2 facing = character.transform.right;
                if (facing.sqrMagnitude <= 0.0001f) facing = Vector2.right;
                baseAngleDeg = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            }

            // first sync and kick the loop
            CleanupDeadStars();                 // drop any missing refs left behind by destroys
            SetStarCount(CurrentCount);         // grow/shrink scene objects to match desired count
            CacheParams();                      // remember fields for change detection in the loop

            if (orbitRoutine == null)
                orbitRoutine = character.StartCoroutine(OrbitLoop());
        }

        public void AddStars(int amount, CharacterBase character)
        {
            if (amount <= 0 || character == null || abilityPrefab == null) return;
            center = character.transform;

            CurrentCount = ClampToMax(CurrentCount + amount);
            CleanupDeadStars();                 // be safe before touching the list
            SetStarCount(CurrentCount);         // reflect new count in the scene
            ResetOrbit();                       // rebuild spacing so the ring stays even
            CacheParams();
        }

        /// <summary>
        /// Rebuild spacing and recalc baseline. Important: do NOT start/stop coroutines here.
        /// </summary>
        public void ResetOrbit()
        {
            CleanupDeadStars();
            if (center == null || stars.Count == 0) return;

            // re-anchor the phase to the owner's facing so motion looks intentional
            Vector2 facing = center.right;
            if (facing.sqrMagnitude <= 0.0001f) facing = Vector2.right;
            baseAngleDeg = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

            Redistribute();

            // NOTE: no StopCoroutine/StartCoroutine here to avoid nested resets
        }

        private void EnsureAtLeast(int count)
        {
            CleanupDeadStars();
            while (stars.Count < count) SpawnOne();
            Redistribute();
        }

        private void SpawnOne()
        {
            var instance = Object.Instantiate(abilityPrefab, Vector3.zero, Quaternion.identity);
            var t = instance.transform;
            stars.Add(t);

            StartSelfSpin(t);   // continuous local spin (purely cosmetic)
            StartPulse(t);      // subtle scale yoyo to keep them alive
        }

        private void StartSelfSpin(Transform t)
        {
            float speed = Mathf.Abs(selfSpinSpeedDeg);
            if (speed <= 0f || t == null) return;

            float dur = 360f / speed; // time for a full turn
            float sign = selfSpinClockwise ? -1f : 1f;
            Vector3 step = selfSpinAxis.normalized * (360f * sign);

            t.DORotate(step, dur, RotateMode.FastBeyond360)
             .SetRelative(true)
             .SetLoops(-1, LoopType.Incremental)
             .SetEase(Ease.Linear)
             .SetLink(t.gameObject); // tween dies with the object
        }

        private void StartPulse(Transform t)
        {
            if (t == null) return;
            var baseScale = t.localScale;

            t.DOScale(baseScale * pulseScale, pulseDuration)
             .SetLoops(-1, LoopType.Yoyo)
             .SetEase(pulseEase)
             .SetLink(t.gameObject);
        }

        private void Redistribute()
        {
            CleanupDeadStars();
            if (center == null || stars.Count == 0) return;

            int n = stars.Count;
            for (int i = 0; i < n; i++)
            {
                var star = stars[i];
                if (star == null) continue;    // skip holes, list gets cleaned next tick anyway
                float ang = baseAngleDeg + (360f / n) * i;
                PositionStar(star, ang);
            }
        }

        private IEnumerator OrbitLoop()
        {
            while (center != null)
            {
                // keep the list neat before we touch it
                CleanupDeadStars();
                if (stars.Count == 0) break;

                // live-reload when someone tweaks Speed from the outside (upgrades etc.)
                if (!Mathf.Approximately(Speed, lastSpeed))
                {
                    angularSpeedDeg = Mathf.Max(1f, Speed);
                    lastSpeed = Speed;         // update first to avoid double triggers
                    ResetOrbit();              // only rebuild spacing, do NOT restart coroutine
                }

                // same idea for CurrentCount: rebuild when it changes
                int clampedCount = ClampToMax(CurrentCount);
                if (clampedCount != lastCount)
                {
                    lastCount = clampedCount;  // update first
                    CurrentCount = clampedCount;
                    SetStarCount(CurrentCount);
                    ResetOrbit();              // only spacing/phase refresh
                }

                float dir = clockwise ? -1f : 1f;
                baseAngleDeg += dir * angularSpeedDeg * Time.deltaTime;

                int n = stars.Count;
                for (int i = 0; i < n; i++)
                {
                    var star = stars[i];
                    if (star == null) continue;
                    float ang = baseAngleDeg + (360f / n) * i;
                    PositionStar(star, ang);
                }
                yield return null;
            }
            orbitRoutine = null;
        }

        private void PositionStar(Transform star, float angleDeg)
        {
            if (star == null) return; // guard against destroyed entries
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 c = center.position;
            Vector2 p = c + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            star.position = p;
        }

        private int ClampToMax(int value)
        {
            if (MaxCount > 0) return Mathf.Clamp(value, 0, MaxCount); // respect max if configured
            return Mathf.Max(0, value);
        }

        private void SetStarCount(int target)
        {
            if (abilityPrefab == null) return;

            CleanupDeadStars();

            // trim extras
            while (stars.Count > target)
            {
                var last = stars[stars.Count - 1];
                if (last != null) Object.Destroy(last.gameObject);
                stars.RemoveAt(stars.Count - 1);
            }

            // add missing
            while (stars.Count < target)
                SpawnOne();

            Redistribute();
        }

        private void CacheParams()
        {
            lastSpeed = Speed;
            lastCount = CurrentCount;
        }

        // destroyed objects leave null slots; clear them out so loops stay simple
        private void CleanupDeadStars()
        {
            for (int i = stars.Count - 1; i >= 0; i--)
            {
                if (stars[i] == null) stars.RemoveAt(i);
            }
        }
    }
}
