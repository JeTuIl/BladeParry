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
    /// <summary>Source of the GameEnded event.</summary>
    [SerializeField] private GameplayLoopController gameplayLoopController;

    /// <summary>Recorder that provides the last fight summary.</summary>
    [SerializeField] private FightSummaryRecorder recorder;

    /// <summary>Text to display the formatted summary.</summary>
    [SerializeField] private TMP_Text summaryText;

    /// <summary>Localized format string for the summary (UI_FightSummary).</summary>
    private static readonly LocalizedString s_summaryFormat = new LocalizedString("BladeParry_LocalizationTable", "UI_FightSummary");

    /// <summary>Cached format string after preload.</summary>
    private string _cachedFormat;

    /// <summary>True when format preload has been started to avoid duplicate loads.</summary>
    private bool _formatLoadStarted;

    /// <summary>Subscribes to GameEnded and starts format preload.</summary>
    private void Start()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded += HandleGameEnd;
        StartCoroutine(PreloadFormat());
    }

    /// <summary>Unsubscribes from GameEnded.</summary>
    private void OnDestroy()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded -= HandleGameEnd;
    }

    /// <summary>Loads the localized summary format string asynchronously.</summary>
    /// <returns>Enumerator for the coroutine.</returns>
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

    /// <summary>Handler for game end: updates summary text with the last fight summary.</summary>
    /// <param name="result">Game end result (not used for display).</param>
    private void HandleGameEnd(GameEndResult result)
    {
        if (summaryText == null)
            return;

        var summary = recorder != null ? recorder.GetLastSummary() : default;
        summaryText.text = FormatSummary(summary);
    }

    /// <summary>Formats the fight summary using the cached format string or a fallback.</summary>
    /// <param name="s">Fight summary data.</param>
    /// <returns>Formatted string for display.</returns>
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
