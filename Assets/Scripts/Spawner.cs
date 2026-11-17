using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("障碍")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("基础间隔（秒）")]
    [SerializeField] private float minInterval = 1.2f;
    [SerializeField] private float maxInterval = 2.2f;

    [Header("高度种类（纵向缩放）")]
    [SerializeField] private float[] heightScales = { 0.7f, 1f, 1.35f };

    [Header("各高度出现概率（顺序对应上面）")]
    [SerializeField] private float[] heightWeights = { 0.35f, 0.45f, 0.2f };

    [Header("基准高度（缩放 1 时的 Y）")]
    [SerializeField] private float baseY = -0.5f;

    [Header("半高（碰撞盒高度的一半，用于底部对齐）")]
    [SerializeField] private float halfHeight = 0.43f;

    private float timer;

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Spawn();
        ResetTimer();
    }

    private void Spawn()
    {
        float scaleY = PickScale();

        // 缩放是绕中心进行的，往上补偿一半增量，底面才不会陷进地里
        float y = baseY + (scaleY - 1f) * halfHeight;

        Vector3 pos = new Vector3(transform.position.x, y, 0f);
        GameObject go = Instantiate(obstaclePrefab, pos, Quaternion.identity);
        go.transform.localScale = new Vector3(1f, scaleY, 1f);
    }

    private float PickScale()
    {
        float total = 0f;
        for (int i = 0; i < heightWeights.Length; i++) total += heightWeights[i];

        float roll = Random.value * total;

        for (int i = 0; i < heightScales.Length; i++)
        {
            roll -= heightWeights[i];
            if (roll <= 0f) return heightScales[i];
        }
        return heightScales[0];
    }

    private void ResetTimer()
    {
        timer = Random.Range(minInterval, maxInterval);
    }
}