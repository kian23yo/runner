using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;

    [Header("生成间隔（秒）")]
    [SerializeField] private float minInterval = 1.5f;
    [SerializeField] private float maxInterval = 3f;

    [Header("可能出现的高度")]
    [SerializeField] private float[] heights = { 0f, 1.2f };

    private float timer;

    private void Start()
    {
        timer = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = Random.Range(minInterval, maxInterval);

        float y = heights[Random.Range(0, heights.Length)];
        Instantiate(coinPrefab, new Vector3(transform.position.x, y, 0f), Quaternion.identity);
    }
}
