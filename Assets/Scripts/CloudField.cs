using System.Collections.Generic;
using UnityEngine;

public class CloudField : MonoBehaviour
{
    [Header("云")]
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private int count = 7;

    [Header("速度")]
    [SerializeField] private float worldSpeed = 5f;
    [SerializeField] private float speedFactor = 0.5f;

    [Header("高度范围")]
    [SerializeField] private float minY = 1.4f;
    [SerializeField] private float maxY = 4.2f;

    [Header("大小范围")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.4f;

    [Header("间距（越大越疏）")]
    [SerializeField] private float spacingFactor = 1.35f;

    [Header("开局就铺满天空（勾上则开场有云，但开场那一下会有云出现）")]
    [SerializeField] private bool prefillScreen = false;

    private readonly List<Transform> clouds = new List<Transform>();
    private readonly List<float> scales = new List<float>();

    private Camera cam;
    private float halfWidthBase = 0.8f;

    private float CamHalf => cam.orthographicSize * cam.aspect;
    private float CamLeft => cam.transform.position.x - CamHalf;
    private float CamRight => cam.transform.position.x + CamHalf;

    private void Start()
    {
        cam = Camera.main;
        if (cam == null || cloudPrefab == null) return;

        SpriteRenderer sr = cloudPrefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            halfWidthBase = sr.sprite.bounds.size.x * 0.5f;

        if (prefillScreen)
            SpawnPrefilled();
        else
            SpawnOffscreenQueue();
    }

    // 开局铺满可视区（云已经在画面里，进游戏就能看到）
    private void SpawnPrefilled()
    {
        float left = CamLeft - 1f;
        float right = CamRight + 4f;
        float slot = (right - left) / count;

        for (int i = 0; i < count; i++)
        {
            float s = Random.Range(minScale, maxScale);
            float x = left + slot * (i + 0.5f) + Random.Range(-slot * 0.15f, slot * 0.15f);
            float y = Mathf.Lerp(minY, maxY, (i + 0.5f) / count) + Random.Range(-0.35f, 0.35f);
            Create(x, y, s);
        }
    }

    // 开局全部排在屏幕右边界之外，依次飘进来（永不在画面中凭空出现）
    private void SpawnOffscreenQueue()
    {
        float cursor = CamRight + 0.5f;   // 第一朵的初始位置：紧贴右边界外
        float lastHalf = 0f;

        for (int i = 0; i < count; i++)
        {
            float s = Random.Range(minScale, maxScale);
            float half = halfWidthBase * s;

            if (i > 0)
                cursor += (lastHalf + half) * spacingFactor;

            float y = Mathf.Lerp(minY, maxY, (i + 0.5f) / count) + Random.Range(-0.35f, 0.35f);
            Create(cursor, y, s);

            lastHalf = half;
        }
    }

    private void Create(float x, float y, float scale)
    {
        GameObject c = Instantiate(cloudPrefab, transform);
        c.transform.position = new Vector3(x, y, 0f);
        c.transform.localScale = Vector3.one * scale;

        clouds.Add(c.transform);
        scales.Add(scale);
    }

    private void Update()
    {
        if (cam == null || clouds.Count == 0) return;

        float step = worldSpeed * speedFactor * Time.deltaTime;
        for (int i = 0; i < clouds.Count; i++)
            clouds[i].position += Vector3.left * step;

        for (int i = 0; i < clouds.Count; i++)
        {
            Transform t = clouds[i];
            float half = halfWidthBase * scales[i];

            // 整朵云都跑出左边界了，才回收
            if (t.position.x + half < CamLeft - 1f)
            {
                float maxX = float.MinValue;
                float maxW = 0f;

                for (int j = 0; j < clouds.Count; j++)
                {
                    if (j == i) continue;

                    if (clouds[j].position.x > maxX)
                    {
                        maxX = clouds[j].position.x;
                        maxW = halfWidthBase * scales[j];
                    }
                }

                float notOverlap = maxX + (maxW + half) * spacingFactor;
                float offScreen = CamRight + half + 0.5f;
                float targetX = Mathf.Max(notOverlap, offScreen);

                t.position = new Vector3(targetX, t.position.y, 0f);
            }
        }
    }
}