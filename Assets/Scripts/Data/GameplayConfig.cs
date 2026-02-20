using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "BladeParry/Gameplay Config")]
public class GameplayConfig : ScriptableObject
{
    [Header("Life")]
    [SerializeField] private int playerStartLife = 3;
    [SerializeField] private int enemyStartLife = 3;

    [Header("Full Life Combo")]
    [SerializeField] private int fullLifeComboNumberOfAttaques = 3;
    [SerializeField] private float fullLifeDurationBetweenAttaque = 0.2f;
    [SerializeField] private float fullLifeWindUpDuration = 0.3f;
    [SerializeField] private float fullLifeWindDownDuration = 0.2f;

    [Header("Empty Life Combo")]
    [SerializeField] private int emptyLifeComboNumberOfAttaques = 2;
    [SerializeField] private float emptyLifeDurationBetweenAttaque = 0.25f;
    [SerializeField] private float emptyLifeWindUpDuration = 0.35f;
    [SerializeField] private float emptyLifeWindDownDuration = 0.25f;

    [Header("Between Combos")]
    [SerializeField] private float pauseBetweenComboDuration = 3.0f;

    public int PlayerStartLife => playerStartLife;
    public int EnemyStartLife => enemyStartLife;

    public int FullLifeComboNumberOfAttaques => fullLifeComboNumberOfAttaques;
    public float FullLifeDurationBetweenAttaque => fullLifeDurationBetweenAttaque;
    public float FullLifeWindUpDuration => fullLifeWindUpDuration;
    public float FullLifeWindDownDuration => fullLifeWindDownDuration;

    public int EmptyLifeComboNumberOfAttaques => emptyLifeComboNumberOfAttaques;
    public float EmptyLifeDurationBetweenAttaque => emptyLifeDurationBetweenAttaque;
    public float EmptyLifeWindUpDuration => emptyLifeWindUpDuration;
    public float EmptyLifeWindDownDuration => emptyLifeWindDownDuration;

    public float PauseBetweenComboDuration => pauseBetweenComboDuration;
}
