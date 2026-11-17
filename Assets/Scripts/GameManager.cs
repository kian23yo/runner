using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("界面")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private Canvas floatCanvas;

    [Header("要停下的东西")]
    [SerializeField] private GroundScroller groundScroller;
    [SerializeField] private Spawner spawner;
    [SerializeField] private CoinSpawner coinSpawner;

    [Header("分数")]
    [SerializeField] private float scorePerSecond = 10f;

    [Header("音效")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip pointSound;
    [SerializeField] private AudioClip coinSound;

    private float score;
    private bool isGameOver;
    private int lastHundred;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        score += Time.deltaTime * scorePerSecond;
        int shown = Mathf.FloorToInt(score);
        scoreText.text = shown.ToString();

        int hundred = shown / 100;
        if (hundred != lastHundred)
        {
            lastHundred = hundred;
            if (audioSource != null && pointSound != null)
                audioSource.PlayOneShot(pointSound);
        }
    }

    public void AddScore(int points, Vector3 worldPos)
    {
        if (isGameOver) return;

        score += points;

        if (audioSource != null && coinSound != null)
            audioSource.PlayOneShot(coinSound);

        SpawnFloatText("+" + points, worldPos);

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.08f, 0.06f);
    }

    private void SpawnFloatText(string content, Vector3 worldPos)
    {
        if (floatCanvas == null) return;

        GameObject go = new GameObject("FloatText");
        go.transform.SetParent(floatCanvas.transform, false);

        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = content;
        label.fontSize = 56f;
        label.color = new Color(1f, 0.85f, 0.2f, 1f);
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240f, 80f);

        Vector3 screen = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            floatCanvas.transform as RectTransform, screen, null, out Vector2 local);
        rect.anchoredPosition = local;

        go.AddComponent<FloatText>();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (groundScroller != null) groundScroller.enabled = false;
        if (spawner != null) spawner.enabled = false;
        if (coinSpawner != null) coinSpawner.enabled = false;
        if (gameOverText != null) gameOverText.SetActive(true);
        if (audioSource != null && hurtSound != null)
            audioSource.PlayOneShot(hurtSound);
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.35f, 0.3f);

        Time.timeScale = 0f;
    }
}