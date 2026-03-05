using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Persists and retrieves player gold across runs. Gold is stored in a separate file from the roguelite run save.
/// </summary>
public static class PlayerCurrencyService
{
    private const string SaveFileName = "player_currency.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary>
    /// Loads gold from disk. Returns 0 if file is missing or invalid.
    /// </summary>
    public static int GetGold()
    {
        if (!File.Exists(SavePath))
            return 0;

        try
        {
            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<PlayerCurrencySaveData>(json);
            return data != null ? Mathf.Max(0, data.gold) : 0;
        }
        catch (Exception e)
        {
            Debug.LogWarning("PlayerCurrencyService: Failed to read or parse save file: " + e.Message);
            return 0;
        }
    }

    /// <summary>
    /// Adds gold to the current total (clamped to non-negative) and saves to disk.
    /// </summary>
    public static void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        int current = GetGold();
        int newTotal = Mathf.Max(0, current + amount);
        SaveGold(newTotal);
    }

    private static void SaveGold(int gold)
    {
        var data = new PlayerCurrencySaveData { gold = gold };
        try
        {
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("PlayerCurrencyService: Failed to write save file: " + e.Message);
        }
    }

    [Serializable]
    private class PlayerCurrencySaveData
    {
        public int gold;
    }
}
