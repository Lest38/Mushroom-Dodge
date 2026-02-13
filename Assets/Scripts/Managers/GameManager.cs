using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameTime = 0f;
    public int score = 0;
    public int highScore = 0;
    public int spores = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private TextMeshProUGUI shieldCountText;

    private bool isGameRunning = false;
    private PlayerController player;
    private float lastUIUpdateTime = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitializeAfterDelay());
    }

    IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        FindPlayer();
        FindUIElements();
        ShowMainMenu();
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isGameRunning)
            StartGame();

        if (Input.GetKeyDown(KeyCode.Escape) && isGameRunning)
            PlayerDied();

        if (isGameRunning)
        {
            gameTime += Time.deltaTime;
            score = Mathf.FloorToInt(gameTime * 10);

            if (gameTime - lastUIUpdateTime > 0.5f)
            {
                UpdateUI();
                lastUIUpdateTime = gameTime;
            }

            DifficultyManager.Instance?.UpdateDifficulty(gameTime);
        }
    }

    public void StartGame()
    {
        isGameRunning = true;
        gameTime = 0f;
        score = 0;
        lastUIUpdateTime = 0f;

        mainMenu?.SetActive(false);
        deathScreen?.SetActive(false);
        inGameUI?.SetActive(true);

        AudioManager.Instance?.PlayGameMusic();

        if (player == null) FindPlayer();
        player?.ResetPlayer();

        SpawnManager.Instance?.StartSpawning();

        UpdateUI();
        PowerUpManager.Instance?.OnGameStateChanged(true);
    }

    public void PlayerDied()
    {
        if (!isGameRunning) return;
        isGameRunning = false;

        SpawnManager.Instance?.StopSpawning();
        PowerUpManager.Instance?.OnGameStateChanged(false);

        foreach (GameObject fb in GameObject.FindGameObjectsWithTag("Fireball"))
            Destroy(fb);

        int earnedSpores = Mathf.Max(1, score / 10);
        spores += earnedSpores;

        bool isNewRecord = score > highScore;
        if (isNewRecord) highScore = score;

        SaveProgress();

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            deathScreen.GetComponent<DeathScreenUI>()?.ShowResults(gameTime, score, highScore, earnedSpores, isNewRecord);
        }

        inGameUI?.SetActive(false);
        UpdateUI();
    }

    public void RestartGame()
    {
        isGameRunning = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartCoroutine(InitializeAfterRestart());
    }

    IEnumerator InitializeAfterRestart()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        FindPlayer();
        FindUIElements();
        ShowMainMenu();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText == null || timeText == null)
            FindUIElements();

        if (scoreText != null && timeText != null)
        {
            scoreText.text = $"Score: {score}";
            timeText.text = $"Time: {gameTime:F1}s";
        }

        if (shieldCountText != null)
        {
            int shieldCount = PowerUpManager.Instance?.GetShieldCount() ?? 0;
            shieldCountText.text = $"Shields: {shieldCount}";
            shieldCountText.gameObject.SetActive(shieldCount > 0);
        }
    }

    void FindUIElements()
    {
        if (scoreText == null)
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

        if (timeText == null)
            timeText = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();

        if (shieldCountText == null)
            shieldCountText = GameObject.Find("ShieldCountText")?.GetComponent<TextMeshProUGUI>();

        deathScreen ??= GameObject.Find("DeathScreen");
        mainMenu ??= GameObject.Find("MainMenu");
        inGameUI ??= GameObject.Find("MainScreen");
    }

    void ShowMainMenu()
    {
        mainMenu?.SetActive(true);
        deathScreen?.SetActive(false);
        inGameUI?.SetActive(false);
    }

    void FindPlayer() => player = FindObjectOfType<PlayerController>();

    void SaveProgress()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.SetInt("Spores", spores);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        spores = PlayerPrefs.GetInt("Spores", 0);
    }

    public bool IsGameRunning => isGameRunning;
}