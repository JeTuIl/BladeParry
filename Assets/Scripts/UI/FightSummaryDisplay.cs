using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Subscribes to GameplayLoopController.GameEnded and updates a TextMeshProUGUI with the fight
/// summary from FightSummaryRecorder. Format string is loaded from the localization table.
/// </summary>
public class FightSummaryDisplay : MonoBehaviour
{
    [SerializeField] private GameplayLoopController gameplayLoopController;
    [SerializeField] private FightSummaryRecorder recorder;
    [SerializeField] private TMP_Text summaryText;

    private static readonly LocalizedString s_summaryFormat = new LocalizedString("BladeParry_LocalizationTable", "UI_FightSummary");
    private string _cachedFormat;
    private bool _formatLoadStarted;

    private void Start()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded += HandleGameEnd;
        StartCoroutine(PreloadFormat());
    }

    private void OnDestroy()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded -= HandleGameEnd;
    }

    private IEnumerator PreloadFormat()
    {
        if (_formatLoadStarted)
            yield break;
        _formatLoadStarted = true;
        var op = s_summaryFormat.GetLocalizedStringAsync();
        if (!op.IsDone)
            yield return op;
        if (op.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(op.Result))
            _cachedFormat = op.Result;
        yield return null;
    }

    private void HandleGameEnd(GameEndResult result)
    {
        if (summaryText == null)
            return;

        var summary = recorder != null ? recorder.GetLastSummary() : default;
        summaryText.text = FormatSummary(summary);
    }

    private string FormatSummary(FightSummaryRecorder.FightSummary s)
    {
        int totalSeconds = Mathf.RoundToInt(s.DurationSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string format = _cachedFormat;
        if (string.IsNullOrEmpty(format))
        {
            format = "Duree du combat : {0} min {1} s\n" +
                "Parades parfaites : {2}\n" +
                "Parades non parfaites : {3}\n" +
                "Plus grande serie de parades parfaites : {4}\n" +
                "Nombre de combos ennemis : {5}\n" +
                "Nombre coups reçus : {6}";
        }
        return string.Format(format, minutes, seconds, s.PerfectParryCount, s.NonPerfectParryCount, s.MaxPerfectStreak, s.EnemyComboCount, s.PlayerLoseHealthCount);
    }
}
