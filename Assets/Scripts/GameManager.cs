using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 给别的脚本查询"现在能不能操作"
    public bool IsRunning => started;

    private const string BestKey = "BestScore";

    [Header("界面")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private GameObject titleText;
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
    private bool started;
    private bool isGameOver;
    private int lastHundred;
    private int bestScore;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        bestScore = PlayerPrefs.GetInt(BestKey, 0);
        RefreshBestText();

        // 开局冻结，等玩家按空格
        if (titleText != null) titleText.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Update()
    {
        // 阶段一：还没开始
        if (!started)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                BeginRun();
            return;
        }

        // 阶段二：已结束
        if (isGameOver)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        // 阶段三：进行中
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

    private void BeginRun()
    {
        started = true;
        Time.timeScale = 1f;
        if (titleText != null) titleText.SetActive(false);
    }

    public void AddScore(int points, Vector3 worldPos)
    {
        if (!started || isGameOver) return;

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

        int finalScore = Mathf.FloorToInt(score);

        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            PlayerPrefs.SetInt(BestKey, bestScore);
            PlayerPrefs.Save();
        }

        RefreshBestText();
        ShowResult(finalScore);

        if (gameOverText != null) gameOverText.SetActive(true);

        if (audioSource != null && hurtSound != null)
            audioSource.PlayOneShot(hurtSound);
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.35f, 0.3f);

        Time.timeScale = 0f;
    }

    private void RefreshBestText()
    {
        if (bestText != null)
            bestText.text = "BEST " + bestScore;
    }

    private void ShowResult(int finalScore)
    {
        if (gameOverText == null) return;

        TMP_Text label = gameOverText.GetComponent<TMP_Text>();
        if (label == null) return;

        label.text = "GAME OVER\nSCORE " + finalScore + "\nBEST " + bestScore + "\n\nPress R to Restart";
    }
}