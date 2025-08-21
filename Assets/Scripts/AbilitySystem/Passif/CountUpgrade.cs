using UnityEngine;

namespace dutpekmezi
{
    [CreateAssetMenu(fileName = "CountUpgrade", menuName = "Game/Scriptable Objects/Ability System/Passif/CountUpgrade")]
    public class CountUpgrade : AbilityBase
    {
        [SerializeField] private AbilityBase targetAbility;
        // NOTE: This is kept for inspector wiring, but we will try to find the runtime instance by AbilityId.

        public override void Activate(CharacterBase character)
        {
            // pick runtime instance first (so we don't modify the asset copy)
            AbilityBase target = null;
            if (AbilitySystem.Instance != null && targetAbility != null && !string.IsNullOrEmpty(targetAbility.AbilityId))
            {
                target = AbilitySystem.Instance.GetAbilityById(targetAbility.AbilityId, searchGainedFirst: true);
            }
            // fallback to serialized reference if runtime not found (better than no-op)
            if (target == null) target = targetAbility;
            if (target == null) return;

            // compute how many to add (matches your existing scaling knobs)
            int addAmount = Mathf.RoundToInt(baseScaleAmount * scaleMultiplier);
            if (addAmount <= 0) return;

            // clamp to MaxCount if configured (>0 means bounded, 0 means unlimited)
            int max = target.MaxCount > 0 ? target.MaxCount : int.MaxValue;
            int newCount = Mathf.Clamp(target.CurrentCount + addAmount, 0, max);

            // update the field only; LightBall will notice the change itself in OrbitLoop
            target.CurrentCount = newCount;
        }

        public override bool CanUse(CharacterBase character)
        {
            // Upgrades should just apply; gating here can silently prevent Activate()
            return targetAbility != null;
        }
    }
}
