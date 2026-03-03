using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Triggers a sequence of attacks via CharacterAttaqueSequence with random directions, then invokes onComboComplete.
/// Can use serialized timings or parameterized timings.
/// </summary>
public class CharacterComboSequence : MonoBehaviour
{
    /// <summary>Runs each individual attack in the combo.</summary>
    [SerializeField] private CharacterAttaqueSequence characterAttaqueSequence;

    /// <summary>Number of attacks when using TriggerCombo (serialized timings).</summary>
    [SerializeField] private int numberOfAttaques = 3;

    /// <summary>Seconds between the end of one attack and the start of the next when using TriggerCombo.</summary>
    [SerializeField] private float durationBetweenAttaque = 0.2f;

    /// <summary>Wind-up duration per attack when using TriggerCombo.</summary>
    [SerializeField] private float windUpDuration = 0.3f;

    /// <summary>Wind-down duration per attack when using TriggerCombo.</summary>
    [SerializeField] private float windDownDuration = 0.2f;

    /// <summary>Invoked when all attacks in the combo have finished.</summary>
    [SerializeField] public UnityEvent onComboComplete;

    /// <summary>Active combo coroutine; null when no combo is running.</summary>
    private Coroutine _comboCoroutine;

    /// <summary>Remaining attacks in the current combo; can be reduced by enhancements (e.g. ChancePerfectParryReduceComboCount).</summary>
    private int _attacksRemaining;

    /// <summary>If set, the next attack in the combo uses this direction instead of random (e.g. ChancePerfectParryNextAttackFromGivenDirection).</summary>
    private Direction? _nextAttackDirectionOverride;

    /// <summary>Cardinal directions used to pick random attack direction (excludes Neutral).</summary>
    private static readonly Direction[] AttackDirections = { Direction.Up, Direction.Left, Direction.Right, Direction.Down };

    /// <summary>
    /// Starts a combo using the serialized number of attacks and timings. Stops any running combo first.
    /// </summary>
    public void TriggerCombo()
    {
        if (characterAttaqueSequence == null)
        {
            Debug.LogWarning("CharacterComboSequence: CharacterAttaqueSequence reference is null.", this);
            return;
        }

        if (_comboCoroutine != null)
            StopCoroutine(_comboCoroutine);

        _comboCoroutine = StartCoroutine(ComboSequenceCoroutine());
    }

    /// <summary>
    /// Starts a combo with the given number of attacks and timings. Stops any running combo first.
    /// </summary>
    /// <param name="numberOfAttaques">Number of attacks in the combo.</param>
    /// <param name="durationBetweenAttaque">Seconds between the end of one attack and the start of the next.</param>
    /// <param name="windUpDuration">Wind-up duration per attack in seconds.</param>
    /// <param name="windDownDuration">Wind-down duration per attack in seconds.</param>
    public void TriggerComboWithParameters(int numberOfAttaques, float durationBetweenAttaque, float windUpDuration, float windDownDuration)
    {
        if (characterAttaqueSequence == null)
        {
            Debug.LogWarning("CharacterComboSequence: CharacterAttaqueSequence reference is null.", this);
            return;
        }

        if (_comboCoroutine != null)
            StopCoroutine(_comboCoroutine);

        _comboCoroutine = StartCoroutine(ComboSequenceCoroutineWithParameters(numberOfAttaques, durationBetweenAttaque, windUpDuration, windDownDuration));
    }

    /// <summary>
    /// Forces the next attack in the combo to use the given direction (e.g. Direction.Up for "from top").
    /// Used by enhancements (e.g. Sword Guard). Override is consumed when the next attack starts.
    /// </summary>
    /// <param name="direction">The cardinal direction for the next attack.</param>
    public void SetNextAttackDirectionOverride(Direction direction)
    {
        _nextAttackDirectionOverride = direction;
    }

    /// <summary>
    /// Reduces the number of remaining attacks in the current combo by one. Used by enhancements (e.g. Fray Charm).
    /// No effect if no combo is running or remaining is already 0.
    /// </summary>
    public void ReduceRemainingAttacks()
    {
        if (_attacksRemaining > 0)
            _attacksRemaining--;
    }

    /// <summary>
    /// Stops the current combo immediately and invokes onComboComplete so listeners can continue (e.g. game end).
    /// </summary>
    public void StopComboAndNotifyComplete()
    {
        if (_comboCoroutine != null)
        {
            StopCoroutine(_comboCoroutine);
            _comboCoroutine = null;
        }
        onComboComplete?.Invoke();
    }

    /// <summary>
    /// Context menu: triggers a combo using serialized parameters.
    /// </summary>
    [ContextMenu("Debug: Trigger Combo")]
    private void DebugTriggerCombo()
    {
        TriggerCombo();
    }

    /// <summary>
    /// Runs the combo with serialized numberOfAttaques and timings; invokes onComboComplete when done.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator ComboSequenceCoroutine()
    {
        if (numberOfAttaques <= 0)
        {
            onComboComplete?.Invoke();
            _comboCoroutine = null;
            yield break;
        }

        for (int i = 0; i < numberOfAttaques; i++)
        {
            Direction randomDirection = AttackDirections[Random.Range(0, AttackDirections.Length)];
            characterAttaqueSequence.StartAttaque(randomDirection, windUpDuration, windDownDuration);
            yield return new WaitForSeconds(windUpDuration + windDownDuration + durationBetweenAttaque);
        }

        onComboComplete?.Invoke();
        _comboCoroutine = null;
    }

    /// <summary>
    /// Runs the combo with the given parameters; invokes onComboComplete when done.
    /// </summary>
    /// <param name="numberOfAttaques">Number of attacks.</param>
    /// <param name="durationBetweenAttaque">Seconds between attacks.</param>
    /// <param name="windUpDuration">Wind-up duration per attack.</param>
    /// <param name="windDownDuration">Wind-down duration per attack.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator ComboSequenceCoroutineWithParameters(int numberOfAttaques, float durationBetweenAttaque, float windUpDuration, float windDownDuration)
    {
        if (numberOfAttaques <= 0)
        {
            onComboComplete?.Invoke();
            _comboCoroutine = null;
            yield break;
        }

        _attacksRemaining = numberOfAttaques;

        while (_attacksRemaining > 0)
        {
            Direction attackDirection;
            if (_nextAttackDirectionOverride.HasValue)
            {
                attackDirection = _nextAttackDirectionOverride.Value;
                _nextAttackDirectionOverride = null;
            }
            else
            {
                attackDirection = AttackDirections[Random.Range(0, AttackDirections.Length)];
            }
            characterAttaqueSequence.StartAttaque(attackDirection, windUpDuration, windDownDuration);
            yield return new WaitForSeconds(windUpDuration + windDownDuration + durationBetweenAttaque);
            _attacksRemaining--;
        }

        onComboComplete?.Invoke();
        _comboCoroutine = null;
    }
}
