using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using ReGolithSystems.UI;

/// <summary>
/// Manages the enhancement choice panel on the level-selection map. Shows 3 options when fightsCompleted >= 1;
/// on selection adds/upgrades the enhancement, saves the run, then transitions to level selection via UiFader.
/// Wire onChoiceComplete to RogueliteMapController.ShowLevelSelectionAfterEnhancement (or to MapUiManager.BuildLevelButtons).
/// </summary>
public class RogueliteEnhancementChoiceController : MonoBehaviour
{
    [Header("Config")]
    /// <summary>Progression config for enhancement pool.</summary>
    [SerializeField] private RogueliteProgressionConfig progressionConfig;
    [Tooltip("Panel root (Canvas or panel GameObject). Shown when enhancement choice is required, hidden after transition.")]
    [SerializeField] private GameObject panelRoot;
    [Tooltip("Optional. Level selection UI root. Hidden (via fader) when showing enhancements; re-enabled when choice is complete so MapUiManager.BuildLevelButtons can run.")]
    [SerializeField] private GameObject levelSelectionRoot;
    [Tooltip("Fader for the enhancement choice panel. Fades in when showing choices, fades out when one is selected. Should be on panelRoot or a child with CanvasGroup.")]
    [SerializeField] private UiFader enhancementPanelFader;
    [Tooltip("Optional. Fader for the level selection UI. Used only to hide it (SetInstant 0) when showing the enhancement panel; level selection is not faded in here—MapUiManager level generation runs after onChoiceComplete.")]
    [SerializeField] private UiFader levelSelectionFader;

    [Header("Choice slots (3)")]
    /// <summary>Images for the three enhancement choice slots.</summary>
    [SerializeField] private Image[] slotImages = new Image[3];

    /// <summary>Name labels for the three slots.</summary>
    [SerializeField] private TMP_Text[] slotNameTexts = new TMP_Text[3];

    /// <summary>Description labels for the three slots.</summary>
    [SerializeField] private TMP_Text[] slotDescTexts = new TMP_Text[3];

    /// <summary>Buttons for the three slots.</summary>
    [SerializeField] private Button[] slotButtons = new Button[3];

    [Header("Level selection after choice")]
    [Tooltip("Optional. When set, ShowLevelSelectionAfterEnhancement is called when the player picks an enhancement, so level buttons are built even if onChoiceComplete is not wired. Assign the same RogueliteMapController that owns onLevelOptionsReady.")]
    [SerializeField] private RogueliteMapController mapControllerForLevelSelection;

    [Header("Choice selection (variety-based steps)")]
    [Tooltip("Number of the 3 choices that are upgrades of already-owned enhancements when the player has 0 or 1 distinct enhancements. Higher variety uses the fields below.")]
    [SerializeField] [Range(0, 3)] private int upgradeSlotsWhenVariety0To1 = 0;
    [Tooltip("Number of upgrade choices when the player has 2 or 3 distinct enhancements.")]
    [SerializeField] [Range(0, 3)] private int upgradeSlotsWhenVariety2To3 = 1;
    [Tooltip("Number of upgrade choices when the player has 4 or 5 distinct enhancements (target 4–6 enhancements).")]
    [SerializeField] [Range(0, 3)] private int upgradeSlotsWhenVariety4To5 = 2;
    [Tooltip("Number of upgrade choices when the player has 6 or more distinct enhancements; favors leveling up over adding new types.")]
    [SerializeField] [Range(0, 3)] private int upgradeSlotsWhenVariety6Plus = 3;

    [Header("Events")]
    [Tooltip("Invoked when the player has chosen an enhancement. Wire to map controller to show level selection, or assign mapControllerForLevelSelection above.")]
    [SerializeField] private UnityEvent onChoiceComplete;

    /// <summary>Number of enhancement choices offered (3).</summary>
    private const int ChoiceCount = 3;

    /// <summary>Current three choices (definition and level to set when selected).</summary>
    private readonly (RogueliteEnhancementDefinition definition, int levelToSet)[] _currentChoices = new (RogueliteEnhancementDefinition, int)[ChoiceCount];

    /// <summary>Returns true if the enhancement choice panel should be shown (fightsCompleted >= 1, pool has definitions, and player has not already chosen for this fight—e.g. after resuming a run).</summary>
    /// <returns>True when at least one fight has been won and the number of enhancement picks is less than fights completed.</returns>
    public static bool ShouldShowEnhancementChoice()
    {
        if (RogueliteRunState.Instance == null) return false;
        int fightsCompleted = RogueliteRunState.Instance.GetFightsCompleted();
        if (fightsCompleted < 1) return false;
        // After resuming a run: if player already has as many enhancement picks as fights won, don't show again
        int enhancementPicksCount = 0;
        foreach (var kv in RogueliteRunState.Instance.GetOwnedEnhancements())
            enhancementPicksCount += kv.Value;
        if (enhancementPicksCount >= fightsCompleted) return false;
        return true;
    }

    /// <summary>Shows the panel and fills the 3 slots with choices. Call from map controller when ShouldShowEnhancementChoice is true. Uses UiFader for animated transition.</summary>
    public void ShowAndPopulate()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        if (levelSelectionRoot != null) levelSelectionRoot.SetActive(true);

        if (levelSelectionFader != null) levelSelectionFader.SetInstant(0f, false);
        if (enhancementPanelFader != null) enhancementPanelFader.SetInstant(0f, false);

        var pool = progressionConfig != null ? progressionConfig.EnhancementPool : null;
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("RogueliteEnhancementChoiceController: No enhancement pool. Hiding panel and completing.");
            NotifyChoiceComplete();
            return;
        }

        if (enhancementPanelFader != null)
            enhancementPanelFader.FadeIn();

        int fightsCompleted = RogueliteRunState.Instance != null ? RogueliteRunState.Instance.GetFightsCompleted() : 0;
        int seed = RogueliteRunState.Instance != null ? RogueliteRunState.Instance.GetRunSeed() : 0;
        PickThreeChoices(pool, seed, fightsCompleted);

        for (int i = 0; i < ChoiceCount; i++)
        {
            var (def, level) = _currentChoices[i];
            int index = i;
            if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null)
            {
                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnSlotSelected(index));
            }

            if (def == null)
            {
                if (slotImages != null && i < slotImages.Length && slotImages[i] != null) slotImages[i].enabled = false;
                if (slotNameTexts != null && i < slotNameTexts.Length && slotNameTexts[i] != null) slotNameTexts[i].text = "";
                if (slotDescTexts != null && i < slotDescTexts.Length && slotDescTexts[i] != null) slotDescTexts[i].text = "";
                if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null) slotButtons[i].interactable = false;
                continue;
            }

            if (slotImages != null && i < slotImages.Length && slotImages[i] != null)
            {
                slotImages[i].enabled = true;
                slotImages[i].sprite = def.Sprite;
            }
            if (slotNameTexts != null && i < slotNameTexts.Length && slotNameTexts[i] != null)
                slotNameTexts[i].text = def.Id + (level > 1 ? " +" + level : "");
            if (slotDescTexts != null && i < slotDescTexts.Length && slotDescTexts[i] != null)
                slotDescTexts[i].text = def.Id + " desc";
            if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null)
                slotButtons[i].interactable = true;

            StartCoroutine(SetLocalizedTextsWhenReady(i, def, level));
        }
    }

    /// <summary>Loads localized name and description for a slot and updates the UI when ready.</summary>
    /// <param name="slotIndex">Slot index (0, 1, or 2).</param>
    /// <param name="def">Enhancement definition for name/description keys.</param>
    /// <param name="level">Level to display.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private System.Collections.IEnumerator SetLocalizedTextsWhenReady(int slotIndex, RogueliteEnhancementDefinition def, int level)
    {
        var table = new LocalizedString(RogueliteEnhancementDefinition.LocalizationTableName, def.NameKey);
        var nameOp = table.GetLocalizedStringAsync();
        yield return nameOp;
        if (nameOp.Status == AsyncOperationStatus.Succeeded && slotNameTexts != null && slotIndex < slotNameTexts.Length && slotNameTexts[slotIndex] != null)
        {
            string name = nameOp.Result;
            if (level > 1) name += " (Lv." + level + ")";
            slotNameTexts[slotIndex].text = name;
        }

        var descTable = new LocalizedString(RogueliteEnhancementDefinition.LocalizationTableName, def.DescriptionKey);
        var descOp = descTable.GetLocalizedStringAsync();
        yield return descOp;
        if (descOp.Status == AsyncOperationStatus.Succeeded && slotDescTexts != null && slotIndex < slotDescTexts.Length && slotDescTexts[slotIndex] != null)
            slotDescTexts[slotIndex].text = descOp.Result;
    }

    /// <summary>Picks three enhancements: a configurable number are upgrades of owned (not max-level), the rest are new. Seed derived from run seed and fights completed.</summary>
    /// <param name="pool">Enhancement definition pool.</param>
    /// <param name="runSeed">Run seed for reproducibility.</param>
    /// <param name="fightsCompleted">Fights completed (used in seed).</param>
    private void PickThreeChoices(IReadOnlyList<RogueliteEnhancementDefinition> pool, int runSeed, int fightsCompleted)
    {
        UnityEngine.Random.InitState(runSeed + fightsCompleted * 31 + 17);

        var upgradeCandidates = new List<(RogueliteEnhancementDefinition def, int levelToSet)>();
        var newCandidates = new List<(RogueliteEnhancementDefinition def, int levelToSet)>();

        int variety = RogueliteRunState.Instance != null ? RogueliteRunState.Instance.GetOwnedEnhancements().Count : 0;

        for (int i = 0; i < pool.Count; i++)
        {
            var def = pool[i];
            if (def == null) continue;
            int currentLevel = RogueliteRunState.Instance != null ? RogueliteRunState.Instance.GetEnhancementLevel(def.Id) : 0;
            if (currentLevel >= def.MaxLevel) continue;

            int levelToSet = currentLevel > 0 ? currentLevel + 1 : 1;
            if (currentLevel > 0)
                upgradeCandidates.Add((def, levelToSet));
            else
                newCandidates.Add((def, levelToSet));
        }

        int numUpgradeSlots = GetUpgradeSlotsForVariety(variety);
        numUpgradeSlots = Mathf.Min(numUpgradeSlots, upgradeCandidates.Count);
        int numNewSlots = ChoiceCount - numUpgradeSlots;
        if (numNewSlots > newCandidates.Count)
        {
            numNewSlots = newCandidates.Count;
            numUpgradeSlots = Mathf.Min(ChoiceCount - numNewSlots, upgradeCandidates.Count);
        }

        for (int i = 0; i < ChoiceCount; i++)
            _currentChoices[i] = (null, 0);

        for (int i = 0; i < numUpgradeSlots; i++)
        {
            int idx = UnityEngine.Random.Range(0, upgradeCandidates.Count);
            _currentChoices[i] = upgradeCandidates[idx];
            upgradeCandidates.RemoveAt(idx);
        }

        for (int i = 0; i < numNewSlots; i++)
        {
            int idx = UnityEngine.Random.Range(0, newCandidates.Count);
            _currentChoices[numUpgradeSlots + i] = newCandidates[idx];
            newCandidates.RemoveAt(idx);
        }
    }

    /// <summary>Returns the configured number of upgrade slots (0–3) for the given variety (distinct enhancements owned).</summary>
    private int GetUpgradeSlotsForVariety(int variety)
    {
        if (variety <= 1) return upgradeSlotsWhenVariety0To1;
        if (variety <= 3) return upgradeSlotsWhenVariety2To3;
        if (variety <= 5) return upgradeSlotsWhenVariety4To5;
        return upgradeSlotsWhenVariety6Plus;
    }

    /// <summary>Handler when the player selects one of the three enhancement slots. Applies enhancement, saves run, and transitions to level selection.</summary>
    /// <param name="index">Slot index (0, 1, or 2).</param>
    private void OnSlotSelected(int index)
    {
        if (index < 0 || index >= ChoiceCount) return;
        var (def, level) = _currentChoices[index];
        if (def == null) return;

        if (RogueliteRunState.Instance != null)
        {
            int oldLevel = RogueliteRunState.Instance.GetEnhancementLevel(def.Id);
            RogueliteRunState.Instance.AddOrUpgradeEnhancement(def.Id, level);
            RogueliteRunState.Instance.ApplyMaxHealthBonusForEnhancement(def, oldLevel, level);
        }

        if (RogueliteRunState.Instance != null)
        {
            RunSaveService.SaveRun(
                RogueliteRunState.Instance.GetRunSeed(),
                RogueliteRunState.Instance.GetFightsCompleted(),
                RogueliteRunState.Instance.TryGetPlayerLifeForNextFight(out float life) ? life : -1f,
                progressionConfig != null ? progressionConfig.TotalFightsInRun : 10,
                "WorldMap");
        }

        StartCoroutine(TransitionToLevelSelection());
    }

    /// <summary>Call when enhancement choice is done (or skipped). Fades out the enhancement panel, then invokes onChoiceComplete so MapUiManager level generation (BuildLevelButtons) can start. Level selection is not faded in here.</summary>
    public void NotifyChoiceComplete()
    {
        StartCoroutine(TransitionToLevelSelection());
    }

    /// <summary>Fades out the enhancement panel, then invokes onChoiceComplete and optionally mapControllerForLevelSelection.</summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator TransitionToLevelSelection()
    {
        if (enhancementPanelFader != null)
        {
            enhancementPanelFader.FadeOut();
            float duration = enhancementPanelFader.FadeDuration;
            if (duration > 0f)
                yield return new WaitForSeconds(duration);
        }

        if (panelRoot != null) panelRoot.SetActive(false);
        if (levelSelectionRoot != null) levelSelectionRoot.SetActive(true);

        if (mapControllerForLevelSelection != null)
            mapControllerForLevelSelection.ShowLevelSelectionAfterEnhancement();
        onChoiceComplete?.Invoke();
    }
}
