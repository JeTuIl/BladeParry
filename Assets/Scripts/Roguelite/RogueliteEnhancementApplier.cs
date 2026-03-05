using UnityEngine;

/// <summary>
/// Applies roguelite enhancement modifiers to a GameplayConfig at fight build time.
/// Call from FightSetupBuilder after NumericalGenerator.FillGameplayConfig.
/// </summary>
public static class RogueliteEnhancementApplier
{
    /// <summary>
    /// Applies current run enhancements to the config (max health, damage bonuses, etc.).
    /// No-op if RunState is null or not in a run.
    /// Heal (e.g. Vital Essence) is not applied here; it is applied at fight start in GameplayLoopController
    /// so that current life gets +heal each fight (capped by max life).
    /// </summary>
    /// <param name="config">Gameplay config to modify (from FightSetupBuilder after FillGameplayConfig).</param>
    public static void ApplyToConfig(GameplayConfig config)
    {
        if (config == null || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return;

        var state = RogueliteRunState.Instance;

        float maxLifeBonus = state.GetMaxLifeBonus();
        if (maxLifeBonus == 0f)
            maxLifeBonus = state.GetTotalValueForEffect(RogueliteEnhancementEffectType.MaxHealthBonus);
        if (maxLifeBonus > 0f)
        {
            float life = config.PlayerStartLife;
            life += maxLifeBonus;
            if (life > 0f) config.SetPlayerStartLife(life);
        }

        // DamageBonus (Whetstone) is applied at deal-time in GameplayLoopController via GetDamageBonusMultiplier()
        // so that every damage type (parry, combo parry, Cadence Stone, Trinity Seal) is multiplied by 1 + value.
    }
}
