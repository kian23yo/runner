using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyX = -15f;

    [Header("数值")]
    [SerializeField] private int value = 10;

    [Header("旋转观感")]
    [SerializeField] private float spinSpeed = 4f;

    private void Update()
    {
        transform.position += Vector3.left * (speed * Time.deltaTime);

        float s = Mathf.Abs(Mathf.Cos(Time.time * spinSpeed));
        transform.localScale = new Vector3(Mathf.Lerp(0.25f, 1f, s), 1f, 1f);

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(value, transform.position);

        Destroy(gameObject);
    }
}
