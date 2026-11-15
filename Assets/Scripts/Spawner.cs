using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("生成什么")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("节奏（秒）")]
    [SerializeField] private float minInterval = 1.2f;
    [SerializeField] private float maxInterval = 2.2f;

    private float timer;

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        timer -= Time.deltaTime;      // 倒计时

        if (timer <= 0f)
        {
            Spawn();
            ResetTimer();
        }
    }

    private void Spawn()
    {
        Instantiate(obstaclePrefab, transform.position, Quaternion.identity);
    }

    private void ResetTimer()
    {
        timer = Random.Range(minInterval, maxInterval);
    }
}