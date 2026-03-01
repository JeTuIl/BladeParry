using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ReGolithSystems.UI;

/// <summary>
/// Map scene controller for roguelite runs: generates three level options and loads the fight scene when the player picks one.
/// UI behaviour is handled by a separate class that can read LevelOptions and call SelectLevel.
/// </summary>
public class RogueliteMapController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private RogueliteProgressionConfig progressionConfig;
    [Tooltip("Scene name to load when a level is selected (e.g. FightingScene).")]
    [SerializeField] private string fightSceneName = "FightingScene";

    [Header("Fade")]
    [SerializeField] private UiFader blackFader;

    [SerializeField] private FightConfig[] _levelOptions = new FightConfig[FightSetupBuilder.LevelOptionsCount];

    [Header("Events")]
    [Tooltip("Invoked when level options have been built. Wire MapUiManager.BuildLevelButtons here.")]
    [SerializeField] private UnityEvent onLevelOptionsReady;

    /// <summary>Level options (3) built at Start. Use this from the UI to display and bind selection.</summary>
    public FightConfig[] LevelOptions => _levelOptions;

    /// <summary>Total number of fights in the run (from progression config).</summary>
    public int TotalFightsInRun => progressionConfig != null ? progressionConfig.TotalFightsInRun : 0;

    private void Start()
    {
        if (!RogueliteRunState.IsRunActive)
        {
            Debug.LogWarning("RogueliteMapController: No active run. Consider loading MainMenu.");
            return;
        }

        if (progressionConfig == null)
        {
            Debug.LogError("RogueliteMapController: progressionConfig is not set.", this);
            return;
        }

        int fightsCompleted = RogueliteRunState.Instance != null ? RogueliteRunState.Instance.GetFightsCompleted() : 0;
        bool isBossMap = progressionConfig.BossFightConfig != null && fightsCompleted == progressionConfig.TotalFightsInRun;
        if (isBossMap)
        {
            Debug.Log("Building boss level. Fights completed: " + fightsCompleted);
            _levelOptions = new FightConfig[1] { progressionConfig.BossFightConfig };
        }
        else
        {
            Debug.Log("Building random levels. Fights completed: " + fightsCompleted);
            _levelOptions = FightSetupBuilder.BuildLevelOptions(progressionConfig, fightsCompleted);
        }
        onLevelOptionsReady?.Invoke();

        if (RogueliteRunState.Instance != null)
        {
            float playerLife = RogueliteRunState.Instance.TryGetPlayerLifeForNextFight(out float life) ? life : -1f;
            RunSaveService.SaveRun(
                RogueliteRunState.Instance.GetRunSeed(),
                fightsCompleted,
                playerLife,
                TotalFightsInRun,
                "WorldMap");
        }
    }

    /// <summary>
    /// Called when the player selects a level (0, 1, or 2). Sets the fight config and loads the fight scene.
    /// </summary>
    public void SelectLevel(int index)
    {
        if (index < 0 || index >= _levelOptions.Length)
        {
            Debug.LogWarning($"RogueliteMapController: Invalid level index {index}.", this);
            return;
        }

        FightConfig chosen = _levelOptions[index];
        if (chosen == null)
        {
            Debug.LogWarning($"RogueliteMapController: No config at index {index}.", this);
            return;
        }

        if (FightConfigProvider.Instance != null)
            FightConfigProvider.Instance.SetFightConfig(chosen);
        else
            Debug.LogError("RogueliteMapController: FightConfigProvider.Instance is null.", this);

        if (string.IsNullOrEmpty(fightSceneName))
        {
            Debug.LogError("RogueliteMapController: fightSceneName is not set.", this);
            return;
        }

        StartCoroutine(FadeThenLoadFightScene());
    }

    [ContextMenu("Select Level 0")]
    private void SelectLevel0FromInspector()
    {
        SelectLevel(0);
    }

    private IEnumerator FadeThenLoadFightScene()
    {
        if (blackFader != null)
        {
            blackFader.FadeIn();
            float duration = blackFader.FadeDuration;
            if (duration > 0f)
                yield return new WaitForSeconds(duration);
        }

        SceneManager.LoadScene(fightSceneName);
    }
}
