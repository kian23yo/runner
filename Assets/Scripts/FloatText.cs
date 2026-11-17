using UnityEngine;
using TMPro;

public class FloatText : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float riseSpeed = 90f;
    [SerializeField] private float fadeStart = 0.35f;

    private TextMeshProUGUI label;
    private RectTransform rect;
    private Color baseColor;
    private float elapsed;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
        baseColor = label.color;
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;

        rect.anchoredPosition += Vector2.up * (riseSpeed * Time.unscaledDeltaTime);

        float k = Mathf.InverseLerp(fadeStart, lifeTime, elapsed);
        Color c = baseColor;
        c.a = 1f - k;
        label.color = c;

        if (elapsed >= lifeTime)
            Destroy(gameObject);
    }
}
