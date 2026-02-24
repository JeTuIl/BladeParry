using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReGolithSystems.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UiFader blackFader;
    [SerializeField] private UiFader mainUi;
    [SerializeField] private UiFader tutoUi;
    [SerializeField] private UiFader difficultyUi;
    [SerializeField] private UiFader optionsUi;
    [SerializeField] private List<FightConfig> fightConfigs = new List<FightConfig>();
    [SerializeField] private string sceneToLoad;
    [SerializeField] private AudioSource musicAudioSource;

    private void Start()
    {
        ApplyMusicVolumeFromOptions();
    }

    private static float GetMusicVolumeFromOptions()
    {
        return OptionManager.Instance != null ? OptionManager.Instance.GetMusicVolume() : 1f;
    }

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
