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

    [Header("音效")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;

    private Rigidbody2D rb;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = false;
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, checkRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.isTrigger) continue;
            isGrounded = true;
            break;
        }

        if (isGrounded && Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (audioSource != null && jumpSound != null)
                audioSource.PlayOneShot(jumpSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}