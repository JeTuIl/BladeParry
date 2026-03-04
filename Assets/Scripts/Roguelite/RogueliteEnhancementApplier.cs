using UnityEngine;

/// <summary>
/// Applies roguelite enhancement modifiers to a GameplayConfig at fight build time.
/// Call from FightSetupBuilder after NumericalGenerator.FillGameplayConfig.
/// </summary>
public static class RogueliteEnhancementApplier
{
    /// <summary>
    /// Applies current run enhancements to the config (max health, heal, damage bonuses, etc.).
    /// No-op if RunState is null or not in a run.
    /// Order: max life bonus and heal (player life), then damage bonus multiplier (parry damage).
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
        float heal = state.GetTotalValueForEffect(RogueliteEnhancementEffectType.Heal);
        if (maxLifeBonus > 0f || heal > 0f)
        {
            float life = config.PlayerStartLife;
            life += maxLifeBonus;
            life += heal;
            if (life > 0f) config.SetPlayerStartLife(life);
        }

        // DamageBonus (Whetstone) is applied at deal-time in GameplayLoopController via GetDamageBonusMultiplier()
        // so that every damage type (parry, combo parry, Cadence Stone, Trinity Seal) is multiplied by 1 + value.
    }
}
