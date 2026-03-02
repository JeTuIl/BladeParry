/// <summary>
/// Effect type for roguelite enhancements. Used to link definitions to gameplay logic.
/// Each enhancement from the reference spreadsheet maps to one (or rarely two) of these.
/// </summary>
public enum RogueliteEnhancementEffectType
{
    None = 0,

    // Damage / ignore
    IgnoreFirstDamagePerCombo,  // Gambeson
    ChanceIgnoreEachHit,        // Serpent's Coil
    DamageBonus,                // Whetstone
    DamageBonusPerfectParryCombo, // Surgeon's Edge
    ChanceDamageEndsCombo,      // Staggering Cloak

    // Life / heal
    Heal,                       // Vital Essence
    MaxHealthBonus,             // Heart of the Bull
    RegenOnFullPerfectParryCombo, // Phoenix Feather

    // Timings
    PerfectParryIncreasesPauseBeforeNextCombo,  // Respite Charm
    PerfectParryIncreasesNextWindUp,            // Watcher's Eye
    LongerWindUp,                               // Telegraph Bell
    LongerWindDown,                             // Grace Period Ring

    // Revive / survival
    ReviveAtEndOfComboWithXLives, // Soul Anchor

    // Scaling with combo
    DamageScalesWithCombo,      // Crescendo Blade
    DamageReceivedDecreasesWithCombo, // Veteran's Carapace
    DamageEveryXPerfectParries, // Cadence Stone
    AfterThreePerfectParriesNextFullParryBonusDamage, // Trinity Seal
    EnemySlowsWithCombo,        // Burden Stone

    // Misc
    ChancePerfectParryMoreCombo,    // Severance Token
    DamageInverseToRemainingHealth, // Executioner's Mark
    ChanceAutoParryOnFail,          // Guardian's Favor
    ChancePerfectParryReduceComboCount, // Fray Charm
    ShieldWhenComboExceedsN,        // Endurance Ward
    ChancePerfectParryNextAttackFromGivenDirection, // Sword Guard
    ReceiveMoreDealMore,            // Oil
    DamageBonusBasicParry,         // Better Cutting Edge
    PerfectParryRatioBonus,        // Better Guard
    DamageBonusFullComboParry,     // Better Tip
}
