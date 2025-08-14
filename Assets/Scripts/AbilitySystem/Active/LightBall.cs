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
        public float selfSpinSpeedDeg = 360f;           // per-star local spin speed (deg/s)
        public Vector3 selfSpinAxis = Vector3.forward;  // Z in 2D
        public bool selfSpinClockwise = false;

        [Header("Pulse")]
        public float pulseScale = 1.25f;                // target scale multiplier
        public float pulseDuration = 0.35f;             // up or down duration (Yoyo)
        public Ease pulseEase = Ease.InOutSine;        // easing for pulse

        private readonly List<Transform> stars = new List<Transform>();
        private Coroutine orbitRoutine;
        private float baseAngleDeg;
        private Transform center;

        public override bool CanUse(CharacterBase character)
            => IsActive && character != null && abilityPrefab != null && initialCount > 0;

        public override void Listener(CharacterBase character)
        {
            base.Listener(character);
            center = character ? character.transform : null;
        }

        public override void Activate(CharacterBase character)
        {
            if (!CanUse(character)) return;

            this.character = character;
            center = character.transform;

            if (stars.Count == 0)
            {
                Vector2 facing = character.transform.right;
                if (facing.sqrMagnitude <= 0.0001f) facing = Vector2.right;
                baseAngleDeg = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

                EnsureAtLeast(initialCount);
            }
            else
            {
                EnsureAtLeast(initialCount);
            }

            if (orbitRoutine == null)
                orbitRoutine = character.StartCoroutine(OrbitLoop());
        }

        public void AddStars(int amount, CharacterBase character)
        {
            if (amount <= 0 || character == null || abilityPrefab == null) return;
            center = character.transform;
            if (orbitRoutine == null)
                orbitRoutine = character.StartCoroutine(OrbitLoop());

            for (int i = 0; i < amount; i++) SpawnOne();
            Redistribute();
        }

        private void EnsureAtLeast(int count)
        {
            while (stars.Count < count) SpawnOne();
            Redistribute();
        }

        private void SpawnOne()
        {
            var instance = Object.Instantiate(abilityPrefab, Vector3.zero, Quaternion.identity);
            var t = instance.transform;
            stars.Add(t);

            StartSelfSpin(t);   // continuous local spin
            StartPulse(t);      // continuous DOScale pulse
        }

        private void StartSelfSpin(Transform t)
        {
            float speed = Mathf.Abs(selfSpinSpeedDeg);
            if (speed <= 0f || t == null) return;

            float dur = 360f / speed; // one full rotation time
            float sign = selfSpinClockwise ? -1f : 1f;
            Vector3 step = selfSpinAxis.normalized * (360f * sign);

            t.DORotate(step, dur, RotateMode.FastBeyond360)
             .SetRelative(true)
             .SetLoops(-1, LoopType.Incremental)
             .SetEase(Ease.Linear)
             .SetLink(t.gameObject); // auto-kill with object
        }

        private void StartPulse(Transform t)
        {
            if (t == null) return;
            var baseScale = t.localScale;

            t.DOScale(baseScale * pulseScale, pulseDuration) // up
             .SetLoops(-1, LoopType.Yoyo)                    // then down, forever
             .SetEase(pulseEase)
             .SetLink(t.gameObject); // tween dies when object dies
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
            orbitRoutine = null;
        }

        private void PositionStar(Transform star, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 c = center.position;
            Vector2 p = c + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            star.position = p;
        }
    }
}
