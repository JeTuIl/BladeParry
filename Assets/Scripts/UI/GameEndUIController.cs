using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ReGolithSystems.UI;

/// <summary>
/// Subscribes to GameplayLoopController.GameEnded; shows win or loss UI, disables specified objects, and fades in the end-game canvas.
/// </summary>
public class GameEndUIController : MonoBehaviour
{
    /// <summary>Source of the GameEnded event.</summary>
    [SerializeField] private GameplayLoopController gameplayLoopController;

    /// <summary>Fader used to fade the scene to black (e.g. before restart or quit).</summary>
    [SerializeField] private UiFader blackFader;

    /// <summary>Name of the scene to load when Quit is called.</summary>
    [SerializeField] private string quitSceneName;

    /// <summary>Optional music switch manager; when set, ContinueRun will fade out music.</summary>
    [SerializeField] private MusciSwitchManager musicSwitchManager;
    /// <summary>Duration in seconds for music fade-out when continuing run.</summary>
    [SerializeField] private float musicFadeOutDuration = 1.5f;

    [Header("Roguelite run")]
    [Tooltip("Scene to load when continuing a run after a win (e.g. WorldMap).")]
    [SerializeField] private string rogueliteMapSceneName = "WorldMap";
    [Tooltip("Total fights in a run; used to decide Map vs main menu after win.")]
    [SerializeField] private int rogueliteTotalFightsInRun = 10;

    /// <summary>Last result from GameEnded; used by ContinueRun.</summary>
    private GameEndResult _lastGameEndResult;

    /// <summary>Canvas group for the end-game screen; alpha is animated from 0 to 1.</summary>
    [SerializeField] private CanvasGroup endGameCanvasGroup;

    /// <summary>Logo or panel shown when the player wins.</summary>
    [SerializeField] private GameObject winLogo;

    /// <summary>Logo or panel shown when the player loses.</summary>
    [SerializeField] private GameObject lossLogo;

    /// <summary>Duration of the fade-in in seconds.</summary>
    [SerializeField] private float fadeInDuration = 1f;

    /// <summary>GameObjects to disable when the game ends (e.g. HUD).</summary>
    [SerializeField] private List<GameObject> gameObjectsToDisableOnGameEnd = new List<GameObject>();

    /// <summary>
    /// Subscribes to GameEnded on the gameplay loop controller.
    /// </summary>
    private void Start()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded += HandleGameEnd;
    }

    /// <summary>
    /// Unsubscribes from GameEnded when the component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (gameplayLoopController != null)
            gameplayLoopController.GameEnded -= HandleGameEnd;
    }

    /// <summary>
    /// Disables configured objects, shows the end-game canvas with win or loss logo, and starts the fade-in.
    /// </summary>
    /// <param name="result">Whether the player won or lost.</param>
    private void HandleGameEnd(GameEndResult result)
    {
        _lastGameEndResult = result;
        if (endGameCanvasGroup == null)
            return;

        foreach (GameObject go in gameObjectsToDisableOnGameEnd)
        {
            if (go != null)
                go.SetActive(false);
        }

        GameObject canvasRoot = endGameCanvasGroup.gameObject;
        canvasRoot.SetActive(true);
        endGameCanvasGroup.alpha = 0f;

        if (result == GameEndResult.PlayerWins)
        {
            if (winLogo != null) winLogo.SetActive(true);
            if (lossLogo != null) lossLogo.SetActive(false);
        }
        else
        {
            if (winLogo != null) winLogo.SetActive(false);
            if (lossLogo != null) lossLogo.SetActive(true);
        }

        StartCoroutine(FadeInCoroutine());
    }

    /// <summary>
    /// Increases the end-game canvas group alpha from 0 to 1 over fadeInDuration.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator FadeInCoroutine()
    {
        if (fadeInDuration <= 0f)
        {
            if (endGameCanvasGroup != null)
                endGameCanvasGroup.alpha = 1f;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration && endGameCanvasGroup != null)
        {
            elapsed += Time.deltaTime;
            endGameCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        if (endGameCanvasGroup != null)
            endGameCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Fades the scene to black, then reloads the current scene.
    /// </summary>
    public void Restart()
    {
        StartCoroutine(FadeToBlackThenLoadScene(SceneManager.GetActiveScene().name));
    }

    /// <summary>
    /// Fades the scene to black, then loads the scene specified by quitSceneName.
    /// If a roguelite run is active, ends the run before loading.
    /// </summary>
    public void Quit()
    {
        if (string.IsNullOrEmpty(quitSceneName))
        {
            Debug.LogWarning("GameEndUIController.Quit: quitSceneName is not set.", this);
            return;
        }
        if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null)
            RogueliteRunState.Instance.EndRun();
        StartCoroutine(FadeToBlackThenLoadScene(quitSceneName));
    }

    /// <summary>
    /// Continues a roguelite run after game end: on win, completes the fight and loads Map or main menu; on loss, ends run and loads main menu.
    /// Call from a "Continue" button when in roguelite flow.
    /// </summary>
    public void ContinueRun()
    {
        if (musicSwitchManager != null)
            musicSwitchManager.Fadeout(musicFadeOutDuration);

        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
        {
            if (!string.IsNullOrEmpty(quitSceneName))
                StartCoroutine(FadeToBlackThenLoadScene(quitSceneName));
            return;
        }

        if (_lastGameEndResult == GameEndResult.PlayerWins)
        {
            RogueliteRunState.Instance.CompleteFight();
            int fightsCompleted = RogueliteRunState.Instance.GetFightsCompleted();
            if (fightsCompleted >= rogueliteTotalFightsInRun)
            {
                RogueliteRunState.Instance.EndRun();
                if (!string.IsNullOrEmpty(quitSceneName))
                    StartCoroutine(FadeToBlackThenLoadScene(quitSceneName));
            }
            else
            {
                if (!string.IsNullOrEmpty(rogueliteMapSceneName))
                    StartCoroutine(FadeToBlackThenLoadScene(rogueliteMapSceneName));
                else
                    Debug.LogWarning("GameEndUIController: rogueliteMapSceneName is not set.", this);
            }
        }
        else
        {
            RogueliteRunState.Instance.EndRun();
            if (!string.IsNullOrEmpty(quitSceneName))
                StartCoroutine(FadeToBlackThenLoadScene(quitSceneName));
        }
    }

    /// <summary>
    /// Fades to black using blackFader, then loads the given scene.
    /// </summary>
    private IEnumerator FadeToBlackThenLoadScene(string sceneName)
    {
        if (blackFader != null)
        {
            blackFader.FadeIn();
            float duration = blackFader.FadeDuration;
            if (duration > 0f)
                yield return new WaitForSeconds(duration);
        }

        SceneManager.LoadScene(sceneName);
    }
}
