using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndUIController : MonoBehaviour
{
    [SerializeField] private GameplayLoopController gameplayLoopController;
    [SerializeField] private CanvasGroup endGameCanvasGroup;
    [SerializeField] private GameObject winLogo;
    [SerializeField] private GameObject lossLogo;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private List<GameObject> gameObjectsToDisableOnGameEnd = new List<GameObject>();

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
}
