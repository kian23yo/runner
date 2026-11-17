using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("速度")]
    [SerializeField] private float worldSpeed = 5f;
    [SerializeField] private float speedFactor = 0.25f;

    private Camera cam;
    private float width;

    private void Awake()
    {
        cam = Camera.main;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        width = sr != null ? sr.bounds.size.x : 20f;
    }

    private void Update()
    {
        if (cam == null || transform.childCount == 0) return;

        float step = worldSpeed * speedFactor * Time.deltaTime;
        float camLeft = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform seg = transform.GetChild(i);
            seg.position += Vector3.left * step;

            if (seg.position.x + width * 0.5f < camLeft)
            {
                float maxX = float.MinValue;
                for (int j = 0; j < transform.childCount; j++)
                    if (transform.GetChild(j).position.x > maxX)
                        maxX = transform.GetChild(j).position.x;

                seg.position = new Vector3(maxX + width, seg.position.y, seg.position.z);
            }
        }
    }
}