using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReGolithSystems.UI;

/// <summary>
/// Pause menu for the fight scene: shows/hides the pause UI via a fader and displays the player's owned enhancements.
/// When opening, sets Time.timeScale to 0 (pauses the game); when closing or quitting, restores it to 1.
/// Uses unscaled-time fades so the pause UI animates correctly while the game is paused.
/// Use this component on the fight scene's pause menu panel; wire buttons to FadeInPauseUI, FadeOutPauseUI, and the quit methods.
/// </summary>
public class PauseMenuFight : MonoBehaviour
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

    [Header("Quit flow")]
    /// <summary>Fader used to fade the screen to black before loading the quit scene.</summary>
    [Tooltip("Fader used to fade the screen to black before loading the quit scene.")]
    [SerializeField] private UiFader blackFader;
    /// <summary>Music switch manager; fades out music when quitting.</summary>
    [Tooltip("Music switch manager; fades out music when quitting.")]
    [SerializeField] private MusciSwitchManager musicSwitchManager;
    /// <summary>Duration in seconds for music fade-out when quitting.</summary>
    [Tooltip("Duration in seconds for music fade-out when quitting.")]
    [SerializeField] private float musicFadeOutDuration = 1f;
    /// <summary>Scene to load when quitting the fight (e.g. back to map). Used by FadeToBlackAndQuit.</summary>
    [Tooltip("Scene to load when quitting the fight (e.g. back to map). Used by FadeToBlackAndQuit.")]
    [SerializeField] private string quitSceneName = "WorldMap";
    /// <summary>Scene to load when quitting to main menu (abandon run). Used by FadeToBlackAndQuitToMainMenu.</summary>
    [Tooltip("Scene to load when quitting to main menu (abandon run). Used by FadeToBlackAndQuitToMainMenu.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Pauses the game (Time.timeScale = 0), populates the enhancements list, then fades in the pause UI using unscaled time.
    /// Call when opening the pause menu (e.g. from a pause button).
    /// </summary>
    public void FadeInPauseUI()
    {
        Time.timeScale = 0f;
        PopulateOwnedEnhancements();
        if (pauseUiFader != null)
            pauseUiFader.FadeInUnscaled();
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
            Debug.LogWarning("PauseMenuFight: enhancementUiPrefab has no EnhancementUi component.", this);
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
    /// Restores the game (Time.timeScale = 1) and fades out the pause UI. Call when closing the pause menu (e.g. Resume button).
    /// </summary>
    public void FadeOutPauseUI()
    {
        Time.timeScale = 1f;
        if (pauseUiFader != null)
            pauseUiFader.FadeOut();
    }

    /// <summary>
    /// Starts the quit flow: fades the screen to black, fades out music, then loads <see cref="quitSceneName"/> (e.g. WorldMap).
    /// Use for a "Back to map" or "Quit fight" button.
    /// </summary>
    public void FadeToBlackAndQuit()
    {
        StartCoroutine(FadeToBlackFadeOutMusicThenLoadScene(quitSceneName));
    }

    /// <summary>
    /// Starts the quit-to-main-menu flow: fades the screen to black, fades out music, then loads <see cref="mainMenuSceneName"/>.
    /// Use for a "Quit run" / "Main menu" button.
    /// </summary>
    public void FadeToBlackAndQuitToMainMenu()
    {
        StartCoroutine(FadeToBlackFadeOutMusicThenLoadScene(mainMenuSceneName));
    }

    /// <summary>
    /// Coroutine: fades to black and fades out music in parallel, waits for the longer of the two
    /// durations, then loads the given scene.
    /// </summary>
    /// <param name="sceneName">Scene to load after the transition.</param>
    /// <returns>Enumerator for use as a coroutine.</returns>
    private IEnumerator FadeToBlackFadeOutMusicThenLoadScene(string sceneName)
    {
        Time.timeScale = 1f;

        if (blackFader != null)
            blackFader.FadeIn();
        if (musicSwitchManager != null)
            musicSwitchManager.Fadeout(musicFadeOutDuration);

        float waitTime = blackFader != null ? blackFader.FadeDuration : 0f;
        if (musicFadeOutDuration > waitTime)
            waitTime = musicFadeOutDuration;
        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
