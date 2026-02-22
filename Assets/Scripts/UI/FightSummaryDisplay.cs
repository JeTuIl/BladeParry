using UnityEngine;
using TMPro;

/// <summary>
/// Subscribes to GameplayLoopController.GameEnded and updates a TextMeshProUGUI with the fight
/// summary from FightSummaryRecorder. Displayed text is in French.
/// </summary>
public class FightSummaryDisplay : MonoBehaviour
{
    [SerializeField] private GameplayLoopController gameplayLoopController;
    [SerializeField] private FightSummaryRecorder recorder;
    [SerializeField] private TMP_Text summaryText;

    private void Start()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded += HandleGameEnd;
    }

    private void OnDestroy()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded -= HandleGameEnd;
    }

    private void HandleGameEnd(GameEndResult result)
    {
        if (summaryText == null)
            return;

        var summary = recorder != null ? recorder.GetLastSummary() : default;
        summaryText.text = FormatSummary(summary);
    }

    private static string FormatSummary(FightSummaryRecorder.FightSummary s)
    {
        int totalSeconds = Mathf.RoundToInt(s.DurationSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return string.Format(
            "Duree du combat : {0} min {1} s\n" +
            "Parades parfaites : {2}\n" +
            "Parades non parfaites : {3}\n" +
            "Plus grande serie de parades parfaites : {4}\n" +
            "Nombre de combos ennemis : {5}\n" +
            "Nombre coups reçus : {6}",
            minutes,
            seconds,
            s.PerfectParryCount,
            s.NonPerfectParryCount,
            s.MaxPerfectStreak,
            s.EnemyComboCount,
            s.PlayerLoseHealthCount);
    }
}
