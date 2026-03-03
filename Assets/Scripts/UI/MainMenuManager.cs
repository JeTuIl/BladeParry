using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ReGolithSystems.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Main menu controller: faders for main/tuto/difficulty/options panels, start game, roguelite run, resume, and quit.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>Fader for full-screen black (e.g. before load).</summary>
    [SerializeField] private UiFader blackFader;

    /// <summary>Fader for the main menu UI.</summary>
    [SerializeField] private UiFader mainUi;

    /// <summary>Fader for the tutorial UI panel.</summary>
    [SerializeField] private UiFader tutoUi;

    /// <summary>Fader for the difficulty selection UI.</summary>
    [SerializeField] private UiFader difficultyUi;

    /// <summary>Fader for the options UI panel.</summary>
    [SerializeField] private UiFader optionsUi;

    /// <summary>Fight configs for difficulty-based start (e.g. easy, normal, hard).</summary>
    [SerializeField] private List<FightConfig> fightConfigs = new List<FightConfig>();

    /// <summary>Scene to load when starting a normal game.</summary>
    [SerializeField] private string sceneToLoad;

    /// <summary>Scene to load when starting a roguelite run (e.g. WorldMap).</summary>
    [Tooltip("Scene to load when starting a roguelite run (e.g. WorldMap).")]
    [SerializeField] private string rogueliteMapSceneName = "WorldMap";

    /// <summary>Music AudioSource; volume applied from options at Start.</summary>
    [SerializeField] private AudioSource musicAudioSource;

    /// <summary>Optional. Resume run button; interactable set from HasValidRunSave at Start.</summary>
    [Tooltip("Optional. Resume run button; interactable and visible are set from HasResumableRun() at Start.")]
    [SerializeField] private Button resumeButton;

    /// <summary>Applies music volume from options and refreshes resume button state.</summary>
    private void Start()
    {
        ApplyMusicVolumeFromOptions();
        RefreshResumeButtonState();
    }

    /// <summary>
    /// Updates the optional Resume button's interactable and visibility from RunSaveService.HasValidRunSave().
    /// Call when showing the main UI if you need to refresh (e.g. returning from another panel).
    /// </summary>
    public void RefreshResumeButtonState()
    {
        if (resumeButton == null) return;
        bool hasSave = RunSaveService.HasValidRunSave();
        resumeButton.interactable = hasSave;
        //resumeButton.gameObject.SetActive(hasSave);
    }

    /// <summary>Returns music volume from PlayerPrefs (via OptionManager).</summary>
    /// <returns>Volume in 0..1.</returns>
    private static float GetMusicVolumeFromOptions()
    {
        return OptionManager.GetMusicVolumeFromPrefs();
    }

    /// <summary>Applies the current music volume from options to the music AudioSource.</summary>
    private void ApplyMusicVolumeFromOptions()
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = GetMusicVolumeFromOptions();
    }

    /// <summary>
    /// Fades to black then quits the game (or leaves play mode in editor).
    /// </summary>
    public void QuitGame()
    {
        StartCoroutine(QuitGameCoroutine());
    }

    /// <summary>
    /// Fades to black then loads the scene specified by sceneToLoad.
    /// </summary>
    public void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }

    /// <summary>
    /// Fades out the main UI then fades in the tutorial UI.
    /// </summary>
    public void GoToTuto()
    {
        StartCoroutine(GoToTutoCoroutine());
    }

    /// <summary>
    /// Fades out the tutorial UI then fades in the main UI.
    /// </summary>
    public void GoBackFromTuto()
    {
        StartCoroutine(GoBackFromTutoCoroutine());
    }

    /// <summary>
    /// Starts a roguelite run (run state + fights completed = 0) then fades to black and loads the map scene.
    /// Requires RogueliteRunState to be present in the scene (e.g. on a persistent GameObject).
    /// </summary>
    public void StartRogueliteRun()
    {
        StartCoroutine(StartRogueliteRunCoroutine());
    }

    /// <summary>
    /// True when a valid run save exists so the player can resume. Use to enable or show the Resume button.
    /// </summary>
    public static bool HasResumableRun()
    {
        return RunSaveService.HasValidRunSave();
    }

    /// <summary>
    /// Resumes a saved run: fades to black then loads the save and the map scene. Call from the Resume button.
    /// Requires RogueliteRunState to be present in the scene.
    /// </summary>
    public void ResumeRogueliteRun()
    {
        StartCoroutine(ResumeRogueliteRunCoroutine());
    }

    /// <summary>
    /// Fades out the main UI then fades in the difficulty selection UI.
    /// </summary>
    public void GoToDifficulty()
    {
        StartCoroutine(GoToDifficultyCoroutine());
    }

    /// <summary>
    /// Fades out the difficulty UI then fades in the main UI.
    /// </summary>
    public void GoBackFromDifficultyToMain()
    {
        StartCoroutine(GoBackFromDifficultyToMainCoroutine());
    }

    /// <summary>
    /// Fades out the difficulty UI then fades in the tutorial UI.
    /// </summary>
    public void GoBackFromDifficultyToTuto()
    {
        StartCoroutine(GoBackFromDifficultyToTutoCoroutine());
    }

    /// <summary>
    /// Fades out the main UI then fades in the options UI.
    /// </summary>
    public void GoToOptions()
    {
        StartCoroutine(GoToOptionsCoroutine());
    }

    /// <summary>
    /// Fades out the options UI then fades in the main UI.
    /// </summary>
    public void GoBackFromOptions()
    {
        StartCoroutine(GoBackFromOptionsCoroutine());
    }

    /// <summary>
    /// Sets the fight config for the next fight via FightConfigProvider using the FightConfig at the given index in the fightConfigs list.
    /// </summary>
    /// <param name="index">Index into the fightConfigs list. Ignored if out of range.</param>
    public void SetFightConfigByIndex(int index)
    {
        if (FightConfigProvider.Instance == null)
        {
            Debug.LogWarning("MainMenuManager: FightConfigProvider.Instance is null. Cannot set fight config.", this);
            return;
        }
        if (fightConfigs == null || index < 0 || index >= fightConfigs.Count)
        {
            Debug.LogWarning($"MainMenuManager: Invalid fight config index {index} (list count: {fightConfigs?.Count ?? 0}).", this);
            return;
        }
        FightConfig config = fightConfigs[index];
        if (config == null)
        {
            Debug.LogWarning($"MainMenuManager: FightConfig at index {index} is null.", this);
            return;
        }
        FightConfigProvider.Instance.SetFightConfig(config);
    }

    private IEnumerator QuitGameCoroutine()
    {
        float duration = blackFader != null ? blackFader.FadeDuration : 0f;
        float startVolume = musicAudioSource != null ? GetMusicVolumeFromOptions() : 0f;

        if (blackFader != null)
            blackFader.FadeIn();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (musicAudioSource != null)
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (musicAudioSource != null)
            musicAudioSource.volume = 0f;

        if (blackFader != null)
            yield return new WaitUntil(() => !blackFader.IsFading);

#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private IEnumerator StartGameCoroutine()
    {
        float duration = blackFader != null ? blackFader.FadeDuration : 0f;
        float startVolume = musicAudioSource != null ? GetMusicVolumeFromOptions() : 0f;

        if (blackFader != null)
            blackFader.FadeIn();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (musicAudioSource != null)
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (musicAudioSource != null)
            musicAudioSource.volume = 0f;

        if (blackFader != null)
            yield return new WaitUntil(() => !blackFader.IsFading);

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogWarning("MainMenuManager: sceneToLoad is empty.", this);
    }

    private IEnumerator StartRogueliteRunCoroutine()
    {
        if (RogueliteRunState.Instance == null)
        {
            Debug.LogError("MainMenuManager: RogueliteRunState.Instance is null. Add RogueliteRunState to the scene.", this);
            yield break;
        }

        RogueliteRunState.Instance.StartRun();

        float duration = blackFader != null ? blackFader.FadeDuration : 0f;
        float startVolume = musicAudioSource != null ? GetMusicVolumeFromOptions() : 0f;

        if (blackFader != null)
            blackFader.FadeIn();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (musicAudioSource != null)
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (musicAudioSource != null)
            musicAudioSource.volume = 0f;

        if (blackFader != null)
            yield return new WaitUntil(() => !blackFader.IsFading);

        if (!string.IsNullOrEmpty(rogueliteMapSceneName))
            SceneManager.LoadScene(rogueliteMapSceneName);
        else
            Debug.LogWarning("MainMenuManager: rogueliteMapSceneName is empty.", this);
    }

    private IEnumerator ResumeRogueliteRunCoroutine()
    {
        if (!RunSaveService.HasValidRunSave())
        {
            Debug.LogWarning("MainMenuManager: No valid run save to resume.");
            yield break;
        }

        if (RogueliteRunState.Instance == null)
        {
            Debug.LogError("MainMenuManager: RogueliteRunState.Instance is null. Add RogueliteRunState to the scene.", this);
            yield break;
        }

        float duration = blackFader != null ? blackFader.FadeDuration : 0f;
        float startVolume = musicAudioSource != null ? GetMusicVolumeFromOptions() : 0f;

        if (blackFader != null)
            blackFader.FadeIn();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (musicAudioSource != null)
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        if (musicAudioSource != null)
            musicAudioSource.volume = 0f;

        if (blackFader != null)
            yield return new WaitUntil(() => !blackFader.IsFading);

        RunSaveService.ResumeRun();
    }

    private IEnumerator GoToTutoCoroutine()
    {
        if (mainUi != null)
        {
            mainUi.FadeOut();
            yield return new WaitUntil(() => mainUi == null || !mainUi.IsFading);
        }

        if (tutoUi != null)
            tutoUi.FadeIn();
    }

    private IEnumerator GoBackFromTutoCoroutine()
    {
        if (tutoUi != null)
        {
            tutoUi.FadeOut();
            yield return new WaitUntil(() => tutoUi == null || !tutoUi.IsFading);
        }

        if (mainUi != null)
            mainUi.FadeIn();
    }

    private IEnumerator GoToDifficultyCoroutine()
    {
        if (mainUi != null)
        {
            mainUi.FadeOut();
            yield return new WaitUntil(() => mainUi == null || !mainUi.IsFading);
        }

        if (difficultyUi != null)
            difficultyUi.FadeIn();
    }

    private IEnumerator GoBackFromDifficultyToMainCoroutine()
    {
        if (difficultyUi != null)
        {
            difficultyUi.FadeOut();
            yield return new WaitUntil(() => difficultyUi == null || !difficultyUi.IsFading);
        }

        if (mainUi != null)
            mainUi.FadeIn();
    }

    private IEnumerator GoBackFromDifficultyToTutoCoroutine()
    {
        if (difficultyUi != null)
        {
            difficultyUi.FadeOut();
            yield return new WaitUntil(() => difficultyUi == null || !difficultyUi.IsFading);
        }

        if (tutoUi != null)
            tutoUi.FadeIn();
    }

    private IEnumerator GoToOptionsCoroutine()
    {
        if (mainUi != null)
        {
            mainUi.FadeOut();
            yield return new WaitUntil(() => mainUi == null || !mainUi.IsFading);
        }

        if (optionsUi != null)
            optionsUi.FadeIn();
    }

    private IEnumerator GoBackFromOptionsCoroutine()
    {
        if (optionsUi != null)
        {
            optionsUi.FadeOut();
            yield return new WaitUntil(() => optionsUi == null || !optionsUi.IsFading);
        }

        if (mainUi != null)
            mainUi.FadeIn();
    }
}
