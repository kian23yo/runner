using UnityEngine;
using UnityEngine.UI;

public class PopIn : MonoBehaviour
{
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float overshoot = 1.12f;

    private RectTransform rect;
    private Graphic graphic;
    private float elapsed;
    private bool playing;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
        playing = true;
        Apply(0f);
    }

    private void Update()
    {
        if (!playing) return;

        elapsed += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(elapsed / duration);
        Apply(k);

        if (k >= 1f) playing = false;
    }

    private void Apply(float k)
    {
        // 0~70% 从 startScale 冲到 overshoot，70%~100% 回落到 1
        float s = k < 0.7f
            ? Mathf.Lerp(startScale, overshoot, k / 0.7f)
            : Mathf.Lerp(overshoot, 1f, (k - 0.7f) / 0.3f);

        rect.localScale = Vector3.one * s;

        if (graphic != null)
        {
            Color c = graphic.color;
            c.a = Mathf.Clamp01(k * 2f);
            graphic.color = c;
        }
    }
}