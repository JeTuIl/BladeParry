using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterComboSequence : MonoBehaviour
{
    [SerializeField] private CharacterAttaqueSequence characterAttaqueSequence;
    [SerializeField] private int numberOfAttaques = 3;
    [SerializeField] private float durationBetweenAttaque = 0.2f;
    [SerializeField] private float windUpDuration = 0.3f;
    [SerializeField] private float windDownDuration = 0.2f;
    [SerializeField] private UnityEvent onComboComplete;

    private Coroutine _comboCoroutine;

    private static readonly Direction[] AttackDirections = { Direction.Up, Direction.Left, Direction.Right, Direction.Down };

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

    [ContextMenu("Debug: Trigger Combo")]
    private void DebugTriggerCombo()
    {
        TriggerCombo();
    }

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
}
