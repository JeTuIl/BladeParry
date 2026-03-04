using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a row of star images whose colors reflect a numeric score.
/// Each image is colored as "filled" or "unfilled" depending on whether its index is less than or equal to the current score.
/// </summary>
public class StarScoreManager : MonoBehaviour
{
    /// <summary>Ordered list of star images to color. Index 0 is the first star; color is set by <see cref="SetScore"/>.</summary>
    [SerializeField] private List<Image> starImages = new List<Image>();

    /// <summary>Color applied to star images whose index is less than or equal to the score.</summary>
    [SerializeField] private Color filledColor = Color.yellow;

    /// <summary>Color applied to star images whose index is greater than the score.</summary>
    [SerializeField] private Color unfilledColor = Color.gray;

    /// <summary>
    /// Sets each star image's color based on the score: filled color for indices 0..score, unfilled for the rest.
    /// </summary>
    /// <param name="score">Number of "filled" stars (0-based: first (score+1) images get filledColor).</param>
    public void SetScore(int score)
    {
        if (starImages == null)
            return;

        for (int i = 0; i < starImages.Count; i++)
        {
            if (starImages[i] != null)
                starImages[i].color = i <= score ? filledColor : unfilledColor;
        }
    }
}
