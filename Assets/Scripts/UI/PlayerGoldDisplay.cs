using UnityEngine;
using TMPro;

/// <summary>
/// Displays the player's persistent gold in a TextMeshProUGUI. Refreshes when the component is enabled.
/// </summary>
public class PlayerGoldDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    private void OnEnable()
    {
        Refresh();
    }

    /// <summary>
    /// Updates the text with the current gold from PlayerCurrencyService. Call when gold may have changed (e.g. after winning a fight).
    /// </summary>
    public void Refresh()
    {
        if (goldText != null)
            goldText.text = PlayerCurrencyService.GetGold().ToString();
    }
}
