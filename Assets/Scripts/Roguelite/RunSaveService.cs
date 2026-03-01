using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Saves and loads roguelite run state to a JSON file under persistentDataPath.
/// Provides HasValidRunSave(), SaveRun(), ResumeRun(), and ClearSave() for main menu and game flow.
/// </summary>
public static class RunSaveService
{
    private const string SaveFileName = "roguelite_run_save.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary>
    /// Returns true if the save file exists and deserializes to a valid run (supported version, non-empty target scene, fightsCompleted >= 0).
    /// Use from main menu to show or enable the Resume button.
    /// </summary>
    public static bool HasValidRunSave()
    {
        if (!File.Exists(SavePath))
            return false;

        try
        {
            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<RogueliteRunSaveData>(json);
            return IsValid(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning("RunSaveService: Failed to read or parse save file: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Saves the current run state to disk. Call after a fight (win) or when on the map.
    /// </summary>
    /// <param name="runSeed">Run seed.</param>
    /// <param name="fightsCompleted">Fights completed.</param>
    /// <param name="playerLifeAfterLastFight">Player life after last fight (-1 if not set).</param>
    /// <param name="totalFightsInRun">Total fights in run.</param>
    /// <param name="targetSceneName">Scene to load on resume (e.g. WorldMap).</param>
    /// <param name="extensionsJson">Optional JSON string for extensible data.</param>
    public static void SaveRun(
        int runSeed,
        int fightsCompleted,
        float playerLifeAfterLastFight,
        int totalFightsInRun,
        string targetSceneName,
        string extensionsJson = null)
    {
        var data = new RogueliteRunSaveData
        {
            version = RogueliteRunSaveData.CurrentVersion,
            runSeed = runSeed,
            fightsCompleted = fightsCompleted,
            playerLifeAfterLastFight = playerLifeAfterLastFight,
            totalFightsInRun = totalFightsInRun,
            targetSceneName = targetSceneName ?? string.Empty,
            extensionsJson = extensionsJson
        };

        try
        {
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("RunSaveService: Failed to write save file: " + e.Message);
        }
    }

    /// <summary>
    /// Loads the save, applies state to RogueliteRunState, and loads the target scene.
    /// Call from main menu after fade (e.g. from MainMenuManager.ResumeRogueliteRun).
    /// Does nothing if save is invalid or RogueliteRunState.Instance is null.
    /// </summary>
    public static void ResumeRun()
    {
        if (RogueliteRunState.Instance == null)
        {
            Debug.LogError("RunSaveService.ResumeRun: RogueliteRunState.Instance is null. Ensure MainMenu or bootstrap has RogueliteRunState.");
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("RunSaveService.ResumeRun: No save file found.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<RogueliteRunSaveData>(json);
            if (!IsValid(data))
            {
                Debug.LogWarning("RunSaveService.ResumeRun: Save data invalid.");
                return;
            }

            RogueliteRunState.Instance.LoadState(data.runSeed, data.fightsCompleted, data.playerLifeAfterLastFight);
            SceneManager.LoadScene(data.targetSceneName);
        }
        catch (Exception e)
        {
            Debug.LogError("RunSaveService.ResumeRun: " + e.Message);
        }
    }

    /// <summary>
    /// Deletes the run save file. Call when the run ends (loss or run complete).
    /// </summary>
    public static void ClearSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("RunSaveService.ClearSave: " + e.Message);
        }
    }

    private static bool IsValid(RogueliteRunSaveData data)
    {
        if (data == null)
            return false;
        if (data.version != RogueliteRunSaveData.CurrentVersion)
            return false;
        if (string.IsNullOrEmpty(data.targetSceneName))
            return false;
        if (data.fightsCompleted < 0)
            return false;
        return true;
    }
}
