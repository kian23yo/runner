using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyX = -15f;

    private void Update()
    {
        // 每帧向左挪一点
        transform.position += Vector3.left * (speed * Time.deltaTime);

        // 跑出屏幕左边就销毁
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}
