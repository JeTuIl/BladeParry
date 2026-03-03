/// <summary>
/// Effect type for roguelite enhancements. Used to link definitions to gameplay logic.
/// Each enhancement from the reference spreadsheet maps to one (or rarely two) of these.
/// </summary>
public enum RogueliteEnhancementEffectType
{
    /// <summary>No effect.</summary>
    None = 0,

    /// <summary>Ignore first damage per combo (e.g. Gambeson).</summary>
    IgnoreFirstDamagePerCombo,

    /// <summary>Chance to ignore each hit (e.g. Serpent's Coil).</summary>
    ChanceIgnoreEachHit,

    /// <summary>Flat damage bonus (e.g. Whetstone).</summary>
    DamageBonus,

    /// <summary>Damage bonus when perfect parrying the full combo (e.g. Surgeon's Edge).</summary>
    DamageBonusPerfectParryCombo,

    /// <summary>Chance that damage ends the combo (e.g. Staggering Cloak).</summary>
    ChanceDamageEndsCombo,

    /// <summary>Heal on selection (e.g. Vital Essence).</summary>
    Heal,

    /// <summary>Max health bonus (e.g. Heart of the Bull).</summary>
    MaxHealthBonus,

    /// <summary>Regen when full combo is perfectly parried (e.g. Phoenix Feather).</summary>
    RegenOnFullPerfectParryCombo,

    /// <summary>Perfect parry increases pause before next combo (e.g. Respite Charm).</summary>
    PerfectParryIncreasesPauseBeforeNextCombo,

    /// <summary>Perfect parry increases next wind-up (e.g. Watcher's Eye).</summary>
    PerfectParryIncreasesNextWindUp,

    /// <summary>Longer wind-up (e.g. Telegraph Bell).</summary>
    LongerWindUp,

    /// <summary>Longer wind-down (e.g. Grace Period Ring).</summary>
    LongerWindDown,

    /// <summary>Revive at end of combo with X lives (e.g. Soul Anchor).</summary>
    ReviveAtEndOfComboWithXLives,

    /// <summary>Damage scales with combo (e.g. Crescendo Blade).</summary>
    DamageScalesWithCombo,

    /// <summary>Damage received decreases with combo (e.g. Veteran's Carapace).</summary>
    DamageReceivedDecreasesWithCombo,

    /// <summary>Damage every X perfect parries (e.g. Cadence Stone).</summary>
    DamageEveryXPerfectParries,

    /// <summary>After three perfect parries, next full parry gets bonus damage (e.g. Trinity Seal).</summary>
    AfterThreePerfectParriesNextFullParryBonusDamage,

    /// <summary>Enemy slows with combo (e.g. Burden Stone).</summary>
    EnemySlowsWithCombo,

    /// <summary>Chance that perfect parry adds more combo (e.g. Severance Token).</summary>
    ChancePerfectParryMoreCombo,

    /// <summary>Damage inverse to remaining health (e.g. Executioner's Mark).</summary>
    DamageInverseToRemainingHealth,

    /// <summary>Chance to auto-parry on fail (e.g. Guardian's Favor).</summary>
    ChanceAutoParryOnFail,

    /// <summary>Chance that perfect parry reduces combo count (e.g. Fray Charm).</summary>
    ChancePerfectParryReduceComboCount,

    /// <summary>Shield when combo exceeds N (e.g. Endurance Ward).</summary>
    ShieldWhenComboExceedsN,

    /// <summary>Chance that perfect parry forces next attack from given direction (e.g. Sword Guard).</summary>
    ChancePerfectParryNextAttackFromGivenDirection,

    /// <summary>Receive more, deal more (e.g. Oil).</summary>
    ReceiveMoreDealMore,

    /// <summary>Damage bonus on basic parry (e.g. Better Cutting Edge).</summary>
    DamageBonusBasicParry,

    /// <summary>Perfect parry ratio bonus (e.g. Better Guard).</summary>
    PerfectParryRatioBonus,

    /// <summary>Damage bonus on full combo parry (e.g. Better Tip).</summary>
    DamageBonusFullComboParry,
}
