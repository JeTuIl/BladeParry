using UnityEngine;
using UnityEngine.UI;

public class LifebarManager : MonoBehaviour
{
    [SerializeField] private Image lifeBarImage;
    [SerializeField] private Image redBarImage;

    [SerializeField] private int maxLifeValue = 100;
    [SerializeField] private int currentLifeValue = 100;
    [SerializeField] private float barSpriteMaxWidth = 200f;

    [SerializeField] private float redBarLerpSpeed = 5f;

    public int MaxLifeValue { get => maxLifeValue; set => maxLifeValue = value; }
    public int CurrentLifeValue { get => currentLifeValue; set => currentLifeValue = value; }

    private float _currentRedBarWidth;

    void Start()
    {
        if (redBarImage != null)
            _currentRedBarWidth = redBarImage.rectTransform.sizeDelta.x;
    }

    void Update()
    {
        if (lifeBarImage == null || redBarImage == null)
            return;

        if (maxLifeValue <= 0)
            return;

        int clampedLife = Mathf.Clamp(currentLifeValue, 0, maxLifeValue);
        float targetWidth = (clampedLife / (float)maxLifeValue) * barSpriteMaxWidth;

        SetBarWidth(lifeBarImage.rectTransform, targetWidth);

        _currentRedBarWidth = Mathf.Lerp(_currentRedBarWidth, targetWidth, redBarLerpSpeed * Time.deltaTime);
        SetBarWidth(redBarImage.rectTransform, _currentRedBarWidth);
    }

    private static void SetBarWidth(RectTransform rectTransform, float width)
    {
        Vector2 size = rectTransform.sizeDelta;
        size.x = width;
        rectTransform.sizeDelta = size;
    }
}
