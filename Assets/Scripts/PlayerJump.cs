using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [Header("跳跃参数")]
    [SerializeField] private float jumpForce = 12f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.15f;

    private Rigidbody2D rb;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // ① 每帧重新判断：脚下有没有地面
        isGrounded = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, checkRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;  // 忽略自己
            if (hit.isTrigger) continue;                  // 忽略触发器（障碍物）
            isGrounded = true;
            break;
        }

        // ② 站在地上 + 按了空格 → 起跳
        if (isGrounded && Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}
