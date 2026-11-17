using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 只有"已开始 + 未暂停 + 未结束 + 未退出"才算真正在跑
    public bool IsRunning => started && !isPaused && !isGameOver && !isQuit;

    private const string BestKey = "BestScore";

    // 场景重载后是否自动开始（"重新开始"用）
    private static bool autoStart;

    [Header("界面")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject quitPanel;
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

    // 运行时状态
    private float score;
    private bool started;
    private bool isPaused;
    private bool isGameOver;
    private bool isQuit;
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

        // 初始状态：只显示主菜单
        if (menuPanel != null) menuPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverText != null) gameOverText.SetActive(false);
        if (quitPanel != null) quitPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = false;

        // 从"重新开始"回来时跳过主菜单
        if (autoStart)
        {
            autoStart = false;
            StartGame();
        }
    }

    private void Update()
    {
        // ① 已退出（浏览器里显示致谢后彻底停住）
        if (isQuit) return;

        // ② 已结束
        if (isGameOver)
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
            else if (Keyboard.current.mKey.wasPressedThisFrame) BackToMenu();
            return;
        }

        // ③ 还没开始（主菜单）
        if (!started)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                StartGame();
            return;
        }

        // ④ 暂停开关
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
            return;
        }

        // ⑤ 暂停中：不推进分数
        if (isPaused) return;

        // ⑥ 正常进行
        score += Time.deltaTime * scorePerSecond;

        int shown = Mathf.FloorToInt(score);
        if (scoreText != null) scoreText.text = shown.ToString();

        // 每跨过 100 分响一次
        int hundred = shown / 100;
        if (hundred != lastHundred)
        {
            lastHundred = hundred;
            if (audioSource != null && pointSound != null)
                audioSource.PlayOneShot(pointSound);
        }
    }

    // ---------------- 状态切换（按钮通过 On Click 调用，必须 public） ----------------

    public void StartGame()
    {
        started = true;
        isPaused = false;
        isGameOver = false;
        isQuit = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (menuPanel != null) menuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void PauseGame()
    {
        if (!started || isGameOver || isPaused || isQuit) return;

        isPaused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void RestartGame()
    {
        autoStart = true;
        ReloadScene();
    }

    public void BackToMenu()
    {
        autoStart = false;
        ReloadScene();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        // 编辑器里没有"退出"，只能停掉 Play 模式
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // 浏览器里关不掉标签页，改为显示致谢面板
        isQuit = true;
        Time.timeScale = 0f;

        if (menuPanel != null) menuPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (quitPanel != null) quitPanel.SetActive(true);
#else
        Application.Quit();
#endif
    }

    private void ReloadScene()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------- 分数与死亡 ----------------

    public void AddScore(int points, Vector3 worldPos)
    {
        if (!IsRunning) return;

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
        isPaused = false;

        // 停掉三台"生产机器"
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
        if (pausePanel != null) pausePanel.SetActive(false);

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

        label.text = "GAME OVER\nSCORE " + finalScore + "\nBEST " + bestScore
                   + "\n\nR = Restart    M = Main Menu";
    }
}