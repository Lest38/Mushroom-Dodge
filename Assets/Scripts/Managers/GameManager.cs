using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameTime = 0f;
    public float lastUIUpdateTime = 0f;
    public int score = 0;
    public int highScore = 0;
    public int spores = 0;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private GameObject mainMenu;

    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject powerUpButton;
    [SerializeField] private TextMeshProUGUI shieldCountText;

    private bool isGameRunning = false;
    private PlayerController player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadProgress();
    }

    void Start()
    {
        FindPlayer();
        ShowMainMenu();

        UpdateUI();
    }

    void FindPlayer()
    {
        player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("Player не найден на сцене!");
        }
        else
        {
            Debug.Log("Player найден!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isGameRunning)
        {
            StartGame();
            Debug.Log("Игра запущена по Space!");
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isGameRunning)
        {
            PlayerDied();
            Debug.Log("Игра завершена по Escape (тест)");
        }

        if (isGameRunning)
        {
            float oldTime = gameTime;
            int oldScore = score;

            gameTime += Time.deltaTime;
            score = Mathf.FloorToInt(gameTime * 10);

            if (gameTime < 5f && Mathf.Floor(gameTime) != Mathf.Floor(oldTime))
            {

                if (SpawnManager.Instance != null)
                {
                    Debug.Log("SpawnManager Instance существует");
                }

                GameObject[] fireballs = GameObject.FindGameObjectsWithTag("Fireball");
                Debug.Log($"Файерболов на сцене: {fireballs.Length}");
            }

            if (gameTime - lastUIUpdateTime > 0.5f)
            {
                UpdateUI();
                lastUIUpdateTime = gameTime;
            }

            if (DifficultyManager.Instance != null)
            {
                DifficultyManager.Instance.UpdateDifficulty(gameTime);
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("=== START GAME ===");

        isGameRunning = true;
        gameTime = 0f;
        score = 0;

        if (mainMenu != null) mainMenu.SetActive(false);
        if (deathScreen != null && deathScreen.activeSelf) deathScreen.SetActive(false);

        if (inGameUI != null) inGameUI.SetActive(true);

        if (player != null)
        {
            player.gameObject.SetActive(true);
            player.ResetPlayer();
        }
        else
        {
            FindPlayer();
            if (player != null)
            {
                player.gameObject.SetActive(true);
                player.ResetPlayer();
            }
        }

        Debug.Log("Запуск спавна файерболов...");

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StartSpawning();
            Debug.Log("SpawnManager найден и запущен");
        }
        else
        {
            Debug.LogError("SpawnManager Instance не найден!");

            SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
            if (spawnManager != null)
            {
                Debug.Log("SpawnManager найден вручную");
                spawnManager.StartSpawning();
            }
            else
            {
                Debug.LogError("SpawnManager не найден на сцене!");
            }
        }

        UpdateUI();

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnGameStateChanged(true);
        }

        Debug.Log("Игра запущена! Ожидайте файерболы через 1.5 секунды...");
    }

    public void PlayerDied()
    {
        if (!isGameRunning) return;

        Debug.Log("=== PLAYER DIED ===");

        isGameRunning = false;

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StopSpawning();
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnGameStateChanged(false);
        }

        GameObject[] fireballs = GameObject.FindGameObjectsWithTag("Fireball");
        foreach (GameObject fb in fireballs)
        {
            if (fb != null) Destroy(fb);
        }

        int earnedSpores = Mathf.Max(1, score / 10);
        spores += earnedSpores;
        Debug.Log($"Начислено спор: {earnedSpores}, всего: {spores}");

        bool isNewRecord = false;
        if (score > highScore)
        {
            highScore = score;
            isNewRecord = true;
            Debug.Log($"НОВЫЙ РЕКОРД: {highScore}");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("NewRecord");
            }
        }

        SaveProgress();

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            Debug.Log("DeathScreen показан");

            DeathScreenUI deathUI = deathScreen.GetComponent<DeathScreenUI>();
            if (deathUI != null)
            {
                deathUI.ShowResults(gameTime, score, highScore, earnedSpores, isNewRecord);
            }
        }
        else
        {
            Debug.LogError("DeathScreen ссылка не назначена!");
        }

        Debug.Log($"Игра окончена. Время: {gameTime:F1}s, Очки: {score}, Рекорд: {highScore}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        if (inGameUI != null)
        {
            inGameUI.SetActive(false);
        }

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnGameStateChanged(false);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateUI()
    {
        Debug.Log($"UpdateUI: Time={gameTime:F1}, Score={score}, isGameRunning={isGameRunning}");

        if (scoreText == null)
        {
            Debug.LogError("scoreText ссылка NULL!");
            return;
        }

        if (!scoreText.gameObject.activeSelf)
        {
            Debug.Log("Включаю ScoreText...");
            scoreText.gameObject.SetActive(true);
        }

        if (!scoreText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("ScoreText не активен в иерархии! Проверьте родительские объекты.");

            Transform parent = scoreText.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.Log($"Включаю родителя: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }
        }

        if (timeText == null)
        {
            Debug.LogError("timeText ссылка NULL!");
            return;
        }

        if (!timeText.gameObject.activeSelf)
        {
            Debug.Log("Включаю TimeText...");
            timeText.gameObject.SetActive(true);
        }

        if (!timeText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TimeText не активен в иерархии!");

            Transform parent = timeText.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    Debug.Log($"Включаю родителя: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }
        }

        if (shieldCountText != null)
        {
            int shieldCount = PowerUpManager.Instance != null ?
                PowerUpManager.Instance.GetShieldCount() : 0;
            shieldCountText.text = $"Shields: {shieldCount}";
            shieldCountText.gameObject.SetActive(shieldCount > 0);
        }

        scoreText.text = $"Score: {score}";
        timeText.text = $"Time: {gameTime:F1}s";

        Debug.Log($"UI обновлен: Score={score}, Time={gameTime:F1}");
    }

    void ShowMainMenu()
    {
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }
    }

    void SaveProgress()
    {
        try
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.SetInt("Spores", spores);
            PlayerPrefs.Save();

            Debug.Log($"Сохранено: HighScore={highScore}, Spores={spores}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка сохранения: {e.Message}");
        }
    }

    void LoadProgress()
    {
        try
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            spores = PlayerPrefs.GetInt("Spores", 0);

            Debug.Log($"Загружено: HighScore={highScore}, Spores={spores}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки: {e.Message}");
            highScore = 0;
            spores = 0;
        }
    }

    public bool IsGameRunning => isGameRunning;

    public void DebugInfo()
    {
        Debug.Log($"=== DEBUG INFO ===");
        Debug.Log($"IsGameRunning: {isGameRunning}");
        Debug.Log($"GameTime: {gameTime:F1}");
        Debug.Log($"Score: {score}");
        Debug.Log($"HighScore: {highScore}");
        Debug.Log($"ScoreText назначен: {scoreText != null}");
        Debug.Log($"TimeText назначен: {timeText != null}");
    }
}