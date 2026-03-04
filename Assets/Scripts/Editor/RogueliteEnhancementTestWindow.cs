using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window to test roguelite enhancements in the fighting scene (Play mode).
/// Set enhancements and levels, pause the flow, and trigger specific test sequences (1 or N attacks, miss / normal parry / perfect parry).
/// Menu: BladeParry > Roguelite Enhancement Test.
/// </summary>
public class RogueliteEnhancementTestWindow : EditorWindow
{
    /// <summary>Progression config whose enhancement pool is used for the test.</summary>
    private RogueliteProgressionConfig _progressionConfig;

    /// <summary>Enhancement id to level (only entries with level > 0 are applied).</summary>
    private readonly Dictionary<string, int> _enhancementLevels = new Dictionary<string, int>();

    /// <summary>Scroll position for the enhancement list.</summary>
    private Vector2 _scrollPosition;

    /// <summary>Whether test mode (pause auto-combos) is enabled.</summary>
    private bool _pauseTestMode;

    /// <summary>Number of attacks (N) for "N attacks" test buttons.</summary>
    private int _comboSizeN = 3;

    /// <summary>Minimum combo size for N.</summary>
    private const int ComboSizeMin = 2;

    /// <summary>Maximum combo size for N.</summary>
    private const int ComboSizeMax = 100;

    /// <summary>Fight simulation: probability (0–1) that each attack is a miss.</summary>
    private float _simulationMissRate = 0.2f;

    /// <summary>Fight simulation: among parries, probability (0–1) of perfect parry.</summary>
    private float _simulationPerfectParryRate = 0.3f;

    /// <summary>Fight simulation: time scale (target FPS = 60 × speed).</summary>
    private float _simulationSpeed = 2f;

    /// <summary>Opens the Enhancement Test window (BladeParry menu).</summary>
    [MenuItem("BladeParry/Roguelite Enhancement Test")]
    public static void ShowWindow()
    {
        var window = GetWindow<RogueliteEnhancementTestWindow>("Enhancement Test");
        window.minSize = new Vector2(320, 400);
    }

    /// <summary>Draws the editor GUI: progression config, enhancement list, test mode, life buttons, and trigger buttons.</summary>
    private void OnGUI()
    {
        _progressionConfig = (RogueliteProgressionConfig)EditorGUILayout.ObjectField("Progression Config", _progressionConfig, typeof(RogueliteProgressionConfig), false);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("This tool only works in Play mode with the fighting scene loaded. Start a run from the main menu and enter a fight, or load the fight scene with RunState bootstrapped.", MessageType.Info);
            return;
        }

        if (_progressionConfig == null)
        {
            EditorGUILayout.HelpBox("Assign a Roguelite Progression Config to set the enhancement pool.", MessageType.Warning);
            return;
        }

        var pool = _progressionConfig.EnhancementPool;
        if (pool == null || pool.Count == 0)
        {
            EditorGUILayout.HelpBox("Progression config has no enhancement pool.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Enhancements (toggle + level)", EditorStyles.boldLabel);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(160));
        foreach (var def in pool)
        {
            if (def == null) continue;
            string id = def.Id;
            if (!_enhancementLevels.TryGetValue(id, out int level))
                level = 0;
            bool selected = level > 0;
            EditorGUILayout.BeginHorizontal();
            bool newSelected = EditorGUILayout.Toggle(selected, GUILayout.Width(20));
            EditorGUILayout.LabelField(id, GUILayout.Width(160));
            if (newSelected)
            {
                int maxLevel = def.MaxLevel;
                int newLevel = EditorGUILayout.IntSlider(level > 0 ? level : 1, 1, maxLevel, GUILayout.Width(120));
                _enhancementLevels[id] = newLevel;
            }
            else
                _enhancementLevels[id] = 0;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply"))
            ApplyEnhancements();
        if (GUILayout.Button("Clear"))
            ClearEnhancements();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Test flow", EditorStyles.boldLabel);
        _pauseTestMode = EditorGUILayout.Toggle("Pause gameplay (test mode)", _pauseTestMode);
        SyncTestMode();

        _comboSizeN = Mathf.Clamp(EditorGUILayout.IntField("Combo size N", _comboSizeN), ComboSizeMin, ComboSizeMax);

        var glc = Object.FindFirstObjectByType<GameplayLoopController>();
        if (glc == null)
        {
            EditorGUILayout.HelpBox("GameplayLoopController not found in the active scene. Load the fighting scene.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Status: " + (glc.IsTestMode() ? "Paused (waiting for trigger)" : "Running"));

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Life (test)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Heal player"))
            glc.HealPlayerToFull();
        if (GUILayout.Button("Heal enemy"))
            glc.HealEnemyToFull();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Trigger", EditorStyles.boldLabel);

        if (GUILayout.Button("1 attack → No parry"))
            TriggerTest(1, ParryOutcome.Miss);
        if (GUILayout.Button("1 attack → Normal parry"))
            TriggerTest(1, ParryOutcome.NormalParry);
        if (GUILayout.Button("1 attack → Perfect parry"))
            TriggerTest(1, ParryOutcome.PerfectParry);
        if (GUILayout.Button($"N ({_comboSizeN}) attacks → No parry (any)"))
            TriggerTest(_comboSizeN, ParryOutcome.Miss);
        if (GUILayout.Button($"N ({_comboSizeN}) attacks → Normal parry (every)"))
            TriggerTest(_comboSizeN, ParryOutcome.NormalParry);
        if (GUILayout.Button($"N ({_comboSizeN}) attacks → Perfect parry (every)"))
            TriggerTest(_comboSizeN, ParryOutcome.PerfectParry);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Fight simulation", EditorStyles.boldLabel);
        _simulationMissRate = Mathf.Clamp01(EditorGUILayout.Slider("Miss rate", _simulationMissRate, 0f, 1f));
        _simulationPerfectParryRate = Mathf.Clamp01(EditorGUILayout.Slider("Perfect parry rate (of parries)", _simulationPerfectParryRate, 0f, 1f));
        _simulationSpeed = Mathf.Max(0.25f, EditorGUILayout.FloatField("Simulation speed (time scale)", _simulationSpeed));
        EditorGUILayout.HelpBox("Target FPS = 60 × speed (e.g. 60 for 1, 120 for 2). In Editor, if FPS stays at 60, disable \"VSync (Game view only)\" in the Game view toolbar.", MessageType.None);
        bool simulationActive = glc.IsFightSimulationActive();
        EditorGUILayout.LabelField("Simulation: " + (simulationActive ? "Running" : "Stopped"));
        EditorGUI.BeginDisabledGroup(simulationActive);
        if (GUILayout.Button("Start simulation"))
        {
            int targetFps = Mathf.RoundToInt(60f * _simulationSpeed);
            Application.targetFrameRate = targetFps;
            QualitySettings.vSyncCount = 0;
            DisableGameViewVSync();
            glc.SetTestMode(true);
            _pauseTestMode = true;
            glc.StartFightSimulation(_simulationMissRate, _simulationPerfectParryRate, _simulationSpeed);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(!simulationActive);
        if (GUILayout.Button("Stop simulation"))
            glc.StopFightSimulation();
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>Syncs the window's pause toggle with GameplayLoopController test mode.</summary>
    private void SyncTestMode()
    {
        if (!Application.isPlaying) return;
        var glc = Object.FindFirstObjectByType<GameplayLoopController>();
        if (glc != null && glc.IsTestMode() != _pauseTestMode)
            glc.SetTestMode(_pauseTestMode);
    }

    /// <summary>Applies the selected enhancement levels to RogueliteRunState (creates run state if missing, restarts run with pool).</summary>
    private void ApplyEnhancements()
    {
        var runState = RogueliteRunState.Instance;
        if (runState == null)
        {
            var go = new GameObject("RogueliteRunState");
            go.AddComponent<RogueliteRunState>();
            runState = RogueliteRunState.Instance;
        }

        runState.SetEnhancementPool(_progressionConfig.EnhancementPool);
        int seed = RogueliteRunState.IsRunActive && runState.GetRunSeed() != 0 ? runState.GetRunSeed() : Random.Range(1, int.MaxValue);
        runState.EndRun();
        runState.StartRun(seed);
        runState.SetEnhancementPool(_progressionConfig.EnhancementPool);

        var pool = _progressionConfig.EnhancementPool;
        foreach (var def in pool)
        {
            if (def == null) continue;
            if (!_enhancementLevels.TryGetValue(def.Id, out int level) || level < 1)
                continue;
            runState.AddOrUpgradeEnhancement(def.Id, level);
            if (def.EffectType == RogueliteEnhancementEffectType.MaxHealthBonus)
                runState.ApplyMaxHealthBonusForEnhancement(def, 0, level);
        }

        Debug.Log("RogueliteEnhancementTestWindow: Applied enhancements to run state.");
    }

    /// <summary>Clears run state and restarts run with no enhancements; clears the level dictionary.</summary>
    private void ClearEnhancements()
    {
        var runState = RogueliteRunState.Instance;
        if (runState == null) return;
        int seed = runState.GetRunSeed() != 0 ? runState.GetRunSeed() : Random.Range(1, int.MaxValue);
        runState.EndRun();
        runState.StartRun(seed);
        runState.SetEnhancementPool(_progressionConfig.EnhancementPool);
        _enhancementLevels.Clear();
        Debug.Log("RogueliteEnhancementTestWindow: Cleared enhancements.");
    }

    /// <summary>Disables the Editor Game view's "VSync (Game view only)" so targetFrameRate is honored. Uses reflection for internal GameView type.</summary>
    private static void DisableGameViewVSync()
    {
        try
        {
            System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gameViewType == null) return;
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType, false, "Game", false);
            if (gameView == null) return;
            PropertyInfo vSyncProp = gameViewType.GetProperty("vSyncEnabled", BindingFlags.Public | BindingFlags.Instance);
            if (vSyncProp != null)
                vSyncProp.SetValue(gameView, false);
        }
        catch (System.Exception) { /* ignore if reflection fails across Unity versions */ }
    }

    /// <summary>Enables test mode, enqueues the given outcome for each attack, and starts one combo with the given attack count.</summary>
    /// <param name="attackCount">Number of attacks in the combo.</param>
    /// <param name="outcome">Outcome to apply for each attack (Miss, NormalParry, or PerfectParry).</param>
    private void TriggerTest(int attackCount, ParryOutcome outcome)
    {
        if (!Application.isPlaying) return;
        var glc = Object.FindFirstObjectByType<GameplayLoopController>();
        if (glc == null)
        {
            Debug.LogWarning("RogueliteEnhancementTestWindow: GameplayLoopController not found.");
            return;
        }

        glc.SetTestMode(true);
        _pauseTestMode = true;

        glc.ClearTestOutcomes();
        var outcomes = new List<ParryOutcome>();
        for (int i = 0; i < attackCount; i++)
            outcomes.Add(outcome);
        glc.EnqueueTestOutcomes(outcomes);
        glc.StartTestCombo(attackCount);
    }
}
