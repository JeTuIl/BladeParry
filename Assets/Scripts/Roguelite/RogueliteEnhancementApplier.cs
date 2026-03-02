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
    /// </summary>
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

        float damageBonus = state.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageBonus);
        float parryMultiplier = 1f + damageBonus;
        if (parryMultiplier != 1f)
            config.SetDamageOnParry(config.DamageOnParry * parryMultiplier);

    }
}
