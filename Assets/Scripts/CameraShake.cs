using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 0.18f;

    private Vector3 basePos;
    private float timer;
    private float duration;
    private float magnitude;

    private void Awake()
    {
        Instance = this;
        basePos = transform.localPosition;
    }

    public void Shake(float dur = -1f, float mag = -1f)
    {
        duration = dur < 0f ? defaultDuration : dur;
        magnitude = mag < 0f ? defaultMagnitude : mag;
        timer = duration;
    }

    private void LateUpdate()
    {
        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;

            float k = Mathf.Clamp01(timer / Mathf.Max(duration, 0.0001f));
            Vector2 offset = Random.insideUnitCircle * (magnitude * k);

            transform.localPosition = basePos + new Vector3(offset.x, offset.y, 0f);
        }
        else if (transform.localPosition != basePos)
        {
            transform.localPosition = basePos;
        }
    }
}
