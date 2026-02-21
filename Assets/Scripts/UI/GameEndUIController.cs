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
    /// </summary>
    public void Quit()
    {
        if (string.IsNullOrEmpty(quitSceneName))
        {
            Debug.LogWarning("GameEndUIController.Quit: quitSceneName is not set.", this);
            return;
        }
        StartCoroutine(FadeToBlackThenLoadScene(quitSceneName));
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
