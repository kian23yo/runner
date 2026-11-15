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

    [Header("要停下的东西")]
    [SerializeField] private GroundScroller groundScroller;
    [SerializeField] private Spawner spawner;

    [Header("分数")]
    [SerializeField] private float scorePerSecond = 10f;

    private float score;
    private bool isGameOver;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isGameOver)
        {
            // 死了之后仍然能按 R —— 因为 timeScale=0 只冻结"时间"，不冻结 Update
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;                                  // 先把时间恢复，否则新场景也是冻结的
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        score += Time.deltaTime * scorePerSecond;
        scoreText.text = Mathf.FloorToInt(score).ToString();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (groundScroller != null) groundScroller.enabled = false;
        if (spawner != null) spawner.enabled = false;
        if (gameOverText != null) gameOverText.SetActive(true);

        Time.timeScale = 0f;
    }
}
