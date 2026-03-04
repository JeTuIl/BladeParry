using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReGolithSystems.UI;

/// <summary>
/// Controls the pause menu on the world map: shows/hides the pause UI via a fader and provides
/// a quit flow that fades to black, fades out music, then loads the main menu scene.
/// When opening the pause menu, displays UI for all enhancements currently owned by the player.
/// </summary>
public class PauseMenuWorldMap : MonoBehaviour
{
    /// <summary>Fader for the pause UI. Used to fade in when opening the pause menu and fade out when closing.</summary>
    [Tooltip("Fader for the pause UI. Used to fade in when opening the pause menu and fade out when closing.")]
    [SerializeField] private UiFader pauseUiFader;

    [Header("Enhancements list")]
    /// <summary>RectTransform under which enhancement UI elements are instantiated. Cleared and repopulated each time the pause menu fades in.</summary>
    [Tooltip("RectTransform under which enhancement UI elements are instantiated. Cleared and repopulated each time the pause menu fades in.")]
    [SerializeField] private RectTransform enhancementsContainer;
    /// <summary>Prefab for a single enhancement row/card. Must have an EnhancementUi component; SetEnhancement is called with each owned enhancement.</summary>
    [Tooltip("Prefab for a single enhancement row/card. Must have an EnhancementUi component; SetEnhancement is called with each owned enhancement.")]
    [SerializeField] private GameObject enhancementUiPrefab;

    [Header("Quit to main menu")]
    /// <summary>Fader used to fade the screen to black before loading the main menu.</summary>
    [Tooltip("Fader used to fade the screen to black before loading the main menu.")]
    [SerializeField] private UiFader blackFader;
    /// <summary>Music switch manager; fades out music when quitting to main menu.</summary>
    [Tooltip("Music switch manager; fades out music when quitting to main menu.")]
    [SerializeField] private MusciSwitchManager musicSwitchManager;
    /// <summary>Duration in seconds for music fade-out when quitting to main menu.</summary>
    [Tooltip("Duration in seconds for music fade-out when quitting to main menu.")]
    [SerializeField] private float musicFadeOutDuration = 1f;
    /// <summary>Name of the main menu scene to load.</summary>
    [Tooltip("Name of the main menu scene to load.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Populates the enhancements container with the player's owned enhancements, then fades in the pause UI.
    /// Call when opening the pause menu.
    /// </summary>
    public void FadeInPauseUI()
    {
        PopulateOwnedEnhancements();
        if (pauseUiFader != null)
            pauseUiFader.FadeIn();
    }

    /// <summary>
    /// Clears <see cref="enhancementsContainer"/> and instantiates one <see cref="enhancementUiPrefab"/> per owned enhancement,
    /// setting each via <see cref="EnhancementUi.SetEnhancement"/> with the definition and level from the current run state.
    /// </summary>
    private void PopulateOwnedEnhancements()
    {
        if (enhancementsContainer == null || enhancementUiPrefab == null) return;
        if (RogueliteRunState.Instance == null) return;

        for (int i = enhancementsContainer.childCount - 1; i >= 0; i--)
            Destroy(enhancementsContainer.GetChild(i).gameObject);

        var enhancementUi = enhancementUiPrefab.GetComponent<EnhancementUi>();
        if (enhancementUi == null)
            enhancementUi = enhancementUiPrefab.GetComponentInChildren<EnhancementUi>();
        if (enhancementUi == null)
        {
            Debug.LogWarning("PauseMenuWorldMap: enhancementUiPrefab has no EnhancementUi component.", this);
            return;
        }

        IReadOnlyDictionary<string, int> owned = RogueliteRunState.Instance.GetOwnedEnhancements();
        if (owned == null || owned.Count == 0) return;

        foreach (var kv in owned)
        {
            RogueliteEnhancementDefinition def = RogueliteRunState.Instance.GetEnhancementDefinition(kv.Key);
            int level = kv.Value;
            GameObject instance = Instantiate(enhancementUiPrefab, enhancementsContainer);
            var ui = instance.GetComponent<EnhancementUi>() ?? instance.GetComponentInChildren<EnhancementUi>();
            if (ui != null)
                ui.SetEnhancement(def, level);
        }
    }

    /// <summary>
    /// Fades out the pause UI. Call when closing the pause menu.
    /// </summary>
    public void FadeOutPauseUI()
    {
        if (pauseUiFader != null)
            pauseUiFader.FadeOut();
    }

    /// <summary>
    /// Starts the quit-to-main-menu flow: fades the screen to black, fades out music,
    /// then loads the main menu scene after both transitions complete.
    /// </summary>
    public void FadeToBlackAndQuitToMainMenu()
    {
        StartCoroutine(FadeToBlackFadeOutMusicThenLoadMainMenu());
    }

    /// <summary>
    /// Coroutine: fades to black and fades out music in parallel, waits for the longer of the two
    /// durations, then loads <see cref="mainMenuSceneName"/>.
    /// </summary>
    /// <returns>Enumerator for use as a coroutine.</returns>
    private IEnumerator FadeToBlackFadeOutMusicThenLoadMainMenu()
    {
        if (blackFader != null)
            blackFader.FadeIn();
        if (musicSwitchManager != null)
            musicSwitchManager.Fadeout(musicFadeOutDuration);

        float waitTime = blackFader != null ? blackFader.FadeDuration : 0f;
        if (musicFadeOutDuration > waitTime)
            waitTime = musicFadeOutDuration;
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }
}
