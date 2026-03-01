using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ReGolithSystems.UI;

/// <summary>
/// Quality scores (1–3) for a single FightConfig. 1 = weakest in category, 3 = strongest.
/// Used to compare the three level options for future UI (e.g. difficulty indicators).
/// Serializable so it can be displayed in the inspector.
/// </summary>
[System.Serializable]
public class FightConfigQualityScores
{
    public int Durability;
    public int Strength;
    public int Speed;
}

/// <summary>
/// Builds level option buttons when RogueliteMapController has generated LevelOptions.
/// One button per FightConfig, randomly positioned in a container, with unique random sprites.
/// </summary>
public class MapUiManager : MonoBehaviour
{
    [SerializeField] private RogueliteMapController mapController;
    [Tooltip("Parent RectTransform under which level option buttons are instantiated.")]
    [SerializeField] private RectTransform buttonContainer;
    [Tooltip("Prefab with Button and Image (sprite assigned at runtime).")]
    [SerializeField] private GameObject buttonPrefab;
    [Tooltip("Sprites to assign to buttons; no duplicate is used. Should have at least as many entries as level options.")]
    [SerializeField] private Sprite[] randomSpriteList;

    [Header("Level detail panel")]
    [Tooltip("Fader for the level detail panel. Fades in when a level button is pressed, fade out via FadeOutLevelDetailPanel().")]
    [SerializeField] private UiFader levelDetailFader;
    [Tooltip("Toggles for Durability score (1–3). Index 0 = score 1, index 1 = score 2, index 2 = score 3.")]
    [SerializeField] private Toggle[] durabilityToggles;
    [Tooltip("Toggles for Strength score (1–3). Index 0 = score 1, index 1 = score 2, index 2 = score 3.")]
    [SerializeField] private Toggle[] strengthToggles;
    [Tooltip("Toggles for Speed score (1–3). Index 0 = score 1, index 1 = score 2, index 2 = score 3.")]
    [SerializeField] private Toggle[] speedToggles;

    [Header("Audio")]
    [Tooltip("Optional. Fades out map music when starting the selected level.")]
    [SerializeField] private MusciSwitchManager musicSwitchManager;

    [Header("Run progress")]
    [Tooltip("Optional. Displays fights completed / total fights in the run.")]
    [SerializeField] private LifebarManager runProgressLifebar;

    private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
    private int _lastSelectedLevelIndex = -1;

    [SerializeField] private FightConfigQualityScores[] _lastQualityScores;

    /// <summary>
    /// Last computed quality scores for the current level options (one per option). Valid after BuildLevelButtons.
    /// </summary>
    public IReadOnlyList<FightConfigQualityScores> LastQualityScores => _lastQualityScores;

    /// <summary>
    /// Evaluates Durability, Strength and Speed (1–3) for each FightConfig. Uses GameplayConfig data; ties are broken by option index then pause/speed so ranks are always 1, 2, 3.
    /// </summary>
    public static FightConfigQualityScores[] EvaluateQualityScores(FightConfig[] options)
    {
        if (options == null || options.Length == 0)
            return Array.Empty<FightConfigQualityScores>();

        int n = options.Length;
        var scores = new FightConfigQualityScores[n];
        for (int i = 0; i < n; i++)
            scores[i] = new FightConfigQualityScores();

        var durabilityEntries = new List<(int index, float value)>();
        var strengthEntries = new List<(int index, float value, float tieBreaker)>();
        var speedEntries = new List<(int index, float duration, float tieBreaker)>();

        for (int i = 0; i < n; i++)
        {
            GameplayConfig gc = options[i]?.GetGameplayConfig();
            if (gc == null)
            {
                durabilityEntries.Add((i, 0f));
                strengthEntries.Add((i, 0f, 0f));
                speedEntries.Add((i, float.MaxValue, i));
                continue;
            }

            float enemyLife = gc.EnemyStartLife;
            int comboSum = gc.FullLifeComboNumberOfAttaques + gc.EmptyLifeComboNumberOfAttaques;
            float pause = gc.PauseBetweenComboDuration;
            float fullTimePerAttack = gc.FullLifeDurationBetweenAttaque + gc.FullLifeWindUpDuration + gc.FullLifeWindDownDuration;
            float emptyTimePerAttack = gc.EmptyLifeDurationBetweenAttaque + gc.EmptyLifeWindUpDuration + gc.EmptyLifeWindDownDuration;
            float totalComboDuration = (gc.FullLifeComboNumberOfAttaques * fullTimePerAttack) + (gc.EmptyLifeComboNumberOfAttaques * emptyTimePerAttack);

            durabilityEntries.Add((i, enemyLife));
            strengthEntries.Add((i, comboSum, -pause));
            speedEntries.Add((i, totalComboDuration, i));
        }

        AssignRanksAscending(durabilityEntries, (e) => e.value, (e) => e.index, (rank, idx) => scores[idx].Durability = rank);
        AssignRanksAscending(strengthEntries, (e) => e.value, (e) => e.tieBreaker, (e) => e.index, (rank, idx) => scores[idx].Strength = rank);
        AssignRanksSpeed(speedEntries, (e) => e.duration, (e) => e.tieBreaker, (e) => e.index, n, (rank, idx) => scores[idx].Speed = rank);
        return scores;
    }

    /// <summary>Sort by primary ascending, then tieBreaker ascending, then index; assign rank 1..n by order.</summary>
    private static void AssignRanksAscending(List<(int index, float value)> entries, Func<(int index, float value), float> primary, Func<(int index, float value), float> tieBreaker, Action<int, int> assign)
    {
        var ordered = entries.OrderBy(e => primary(e)).ThenBy(e => tieBreaker(e)).ThenBy(e => e.index).ToList();
        for (int r = 0; r < ordered.Count; r++)
            assign(r + 1, ordered[r].index);
    }

    private static void AssignRanksAscending(List<(int index, float value, float tieBreaker)> entries, Func<(int index, float value, float tieBreaker), float> primary, Func<(int index, float value, float tieBreaker), float> secondary, Func<(int index, float value, float tieBreaker), int> indexSelector, Action<int, int> assign)
    {
        var ordered = entries.OrderBy(e => primary(e)).ThenBy(e => secondary(e)).ThenBy(e => indexSelector(e)).ToList();
        for (int r = 0; r < ordered.Count; r++)
            assign(r + 1, ordered[r].index);
    }

    /// <summary>Sort by primary (duration) ascending: shortest = fastest = highest rank. Assigns rank n to first, 1 to last.</summary>
    private static void AssignRanksSpeed(List<(int index, float duration, float tieBreaker)> entries, Func<(int index, float duration, float tieBreaker), float> primary, Func<(int index, float duration, float tieBreaker), float> secondary, Func<(int index, float duration, float tieBreaker), int> indexSelector, int n, Action<int, int> assign)
    {
        var ordered = entries.OrderBy(e => primary(e)).ThenBy(e => secondary(e)).ThenBy(e => indexSelector(e)).ToList();
        for (int r = 0; r < ordered.Count; r++)
            assign(n - r, ordered[r].index);
    }

    /// <summary>
    /// Called when level options are ready. Instantiate one button per FightConfig, position randomly in container, assign unique sprites.
    /// Wire this to RogueliteMapController's onLevelOptionsReady in the inspector.
    /// </summary>
    public void BuildLevelButtons()
    {
        if (mapController == null)
        {
            Debug.LogWarning("MapUiManager: mapController is not set.", this);
            return;
        }

        FightConfig[] levelOptions = mapController.LevelOptions;
        if (levelOptions == null || levelOptions.Length == 0)
        {
            Debug.LogWarning("MapUiManager: No level options to display.", this);
            return;
        }

        if (buttonContainer == null)
        {
            Debug.LogError("MapUiManager: buttonContainer is not set.", this);
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("MapUiManager: buttonPrefab is not set.", this);
            return;
        }

        if (randomSpriteList == null || randomSpriteList.Length == 0)
        {
            Debug.LogWarning("MapUiManager: randomSpriteList is empty; button images will not be set.", this);
        }

        if (runProgressLifebar != null)
        {
            int completed = RogueliteRunState.Instance != null ? RogueliteRunState.Instance.GetFightsCompleted() : 0;
            int total = mapController.TotalFightsInRun;
            if (total > 0)
            {
                runProgressLifebar.MaxLifeValue = total;
                runProgressLifebar.CurrentLifeValue = completed;
            }
        }

        ClearSpawnedButtons();

        _lastQualityScores = EvaluateQualityScores(levelOptions);
        if (levelOptions.Length == 1)
            _lastQualityScores[0] = new FightConfigQualityScores { Durability = 3, Strength = 3, Speed = 3 };

        List<int> availableSpriteIndices = new List<int>();
        if (randomSpriteList != null)
        {
            for (int i = 0; i < randomSpriteList.Length; i++)
                availableSpriteIndices.Add(i);
        }

        Rect containerRect = buttonContainer.rect;
        List<Rect> placedRects = new List<Rect>();

        for (int i = 0; i < levelOptions.Length; i++)
        {
            FightConfig config = levelOptions[i];
            if (config == null)
                continue;

            GameObject instance = Instantiate(buttonPrefab, buttonContainer);
            _spawnedButtons.Add(instance);

            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Rect childRect = rectTransform.rect;
                Vector2 randomPosition = GetRandomPositionInsideContainerAvoidingOverlap(containerRect, childRect, placedRects);
                rectTransform.anchoredPosition = randomPosition;
                Rect rectInContainer = RectFromCenterAndSize(randomPosition, childRect.width, childRect.height);
                placedRects.Add(rectInContainer);
            }

            Image image = instance.GetComponent<Image>();
            if (image != null && availableSpriteIndices.Count > 0)
            {
                int pickIndex = UnityEngine.Random.Range(0, availableSpriteIndices.Count);
                int spriteIndex = availableSpriteIndices[pickIndex];
                availableSpriteIndices.RemoveAt(pickIndex);
                if (randomSpriteList[spriteIndex] != null)
                    image.sprite = randomSpriteList[spriteIndex];
            }

            Button button = instance.GetComponent<Button>();
            if (button != null)
            {
                FightConfig capturedConfig = config;
                button.onClick.AddListener(() => OnLevelButtonPressed(capturedConfig));
            }
        }
    }

    /// <summary>
    /// Called when the user presses one of the generated level buttons. Fades in the level detail panel and sets toggles from the selected level's quality scores.
    /// </summary>
    public void OnLevelButtonPressed(FightConfig fightConfig)
    {
        FightConfig[] levelOptions = mapController.LevelOptions;
        int levelIndex = -1;
        for (int i = 0; i < levelOptions.Length; i++)
        {
            if (levelOptions[i] == fightConfig)
            {
                levelIndex = i;
                break;
            }
        }

        _lastSelectedLevelIndex = levelIndex;

        if (levelDetailFader != null)
            levelDetailFader.FadeIn();

        if (_lastQualityScores != null && levelIndex >= 0 && levelIndex < _lastQualityScores.Length)
        {
            FightConfigQualityScores scores = _lastQualityScores[levelIndex];
            SetTogglesFromScore(durabilityToggles, scores.Durability);
            SetTogglesFromScore(strengthToggles, scores.Strength);
            SetTogglesFromScore(speedToggles, scores.Speed);
        }
    }

    /// <summary>
    /// Fades out the level detail panel. Call this when closing the panel (e.g. from a button).
    /// </summary>
    public void FadeOutLevelDetailPanel()
    {
        if (levelDetailFader != null)
            levelDetailFader.FadeOut();
    }

    /// <summary>
    /// Starts the last selected level (the one chosen when the user pressed a level button). Call after confirming (e.g. after closing the detail panel).
    /// </summary>
    public void StartLastSelectedLevel()
    {
        if (_lastSelectedLevelIndex < 0 || mapController == null)
            return;
        if (musicSwitchManager != null)
            musicSwitchManager.Fadeout(1f);
        mapController.SelectLevel(_lastSelectedLevelIndex);
    }

    private static void SetTogglesFromScore(Toggle[] toggles, int score)
    {
        if (toggles == null)
            return;
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i] != null)
                toggles[i].isOn = (i + 1) <= score;
        }
    }

    private void ClearSpawnedButtons()
    {
        foreach (GameObject go in _spawnedButtons)
        {
            if (go != null)
                Destroy(go);
        }
        _spawnedButtons.Clear();
    }

    private const int MaxPlacementAttempts = 100;

    private static Vector2 GetRandomPositionInsideContainerAvoidingOverlap(Rect containerRect, Rect childRect, List<Rect> existingRects)
    {
        float halfChildW = childRect.width * 0.5f;
        float halfChildH = childRect.height * 0.5f;
        float minX = containerRect.xMin + halfChildW;
        float maxX = containerRect.xMax - halfChildW;
        float minY = containerRect.yMin + halfChildH;
        float maxY = containerRect.yMax - halfChildH;

        if (minX > maxX) minX = maxX = (containerRect.xMin + containerRect.xMax) * 0.5f;
        if (minY > maxY) minY = maxY = (containerRect.yMin + containerRect.yMax) * 0.5f;

        for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            float x = UnityEngine.Random.Range(minX, maxX);
            float y = UnityEngine.Random.Range(minY, maxY);
            Rect candidate = RectFromCenterAndSize(new Vector2(x, y), childRect.width, childRect.height);

            bool overlaps = false;
            for (int i = 0; i < existingRects.Count; i++)
            {
                if (candidate.Overlaps(existingRects[i]))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
                return new Vector2(x, y);
        }

        return new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
    }

    private static Rect RectFromCenterAndSize(Vector2 center, float width, float height)
    {
        return new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height);
    }
}
