using UnityEngine;

namespace dutpekmezi
{
    [CreateAssetMenu(fileName = "CountUpgrade", menuName = "Game/Scriptable Objects/Ability System/Passif/CountUpgrade")]
    public class CountUpgrade : AbilityBase
    {
        [SerializeField] private AbilityBase targetAbility;
        // wired in inspector, but we resolve runtime copy by id

        public override void Activate(CharacterBase character)
        {
            if (targetAbility == null) return;

            // try to grab the runtime version instead of touching the asset
            var runtime = AbilitySystem.Instance != null
                ? AbilitySystem.Instance.GetGainedAbilityById(targetAbility.AbilityId)
                : null;

            // if player doesn't own it yet, nothing to do (could auto-gain but not now)
            if (runtime == null) return;

            int addAmount = Mathf.RoundToInt(baseScaleAmount * scaleMultiplier);
            if (addAmount <= 0) return;

            int max = runtime.MaxCount > 0 ? runtime.MaxCount : int.MaxValue;
            runtime.CurrentCount = Mathf.Clamp(runtime.CurrentCount + addAmount, 0, max);
            // LightBall will see CurrentCount change in its update loop and refresh itself
        }

        public override bool CanUse(CharacterBase character)
        {
            return targetAbility != null; // basic guard, don't overthink
        }
    }
}
