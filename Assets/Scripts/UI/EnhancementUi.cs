using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnhancementUi : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    /// <summary>
    /// Updates the UI to represent the given enhancement at the specified level.
    /// Level is shown in parentheses after the title when level is 2 or more.
    /// </summary>
    /// <param name="definition">The enhancement definition (can be null to clear).</param>
    /// <param name="level">Enhancement level (e.g. 1 for first pick, 2 for first upgrade).</param>
    public void SetEnhancement(RogueliteEnhancementDefinition definition, int level)
    {
        if (definition == null)
        {
            if (image != null) image.enabled = false;
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            return;
        }

        if (image != null)
        {
            image.enabled = true;
            image.sprite = definition.Sprite;
        }

        if (titleText != null) titleText.text = definition.Id;
        if (descriptionText != null) descriptionText.text = "";

        StartCoroutine(SetLocalizedTextsWhenReady(definition, level));
    }

    private IEnumerator SetLocalizedTextsWhenReady(RogueliteEnhancementDefinition definition, int level)
    {
        var nameTable = new LocalizedString(RogueliteEnhancementDefinition.LocalizationTableName, definition.NameKey);
        var nameOp = nameTable.GetLocalizedStringAsync();
        yield return nameOp;
        if (nameOp.Status == AsyncOperationStatus.Succeeded && titleText != null)
        {
            string title = nameOp.Result;
            if (level >= 2) title += " (" + level + ")";
            titleText.text = title;
        }

        var descTable = new LocalizedString(RogueliteEnhancementDefinition.LocalizationTableName, definition.DescriptionKey);
        var descOp = descTable.GetLocalizedStringAsync();
        yield return descOp;
        if (descOp.Status == AsyncOperationStatus.Succeeded && descriptionText != null)
            descriptionText.text = descOp.Result;
    }
}
