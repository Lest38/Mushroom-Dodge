using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

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
    private bool isFindingUI = false;

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
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForEndOfFrame();

        FindPlayer();

        ValidateUIReferences();

        ShowMainMenu();
        UpdateUI();

        Debug.Log("GameManager инициализирован");
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
        lastUIUpdateTime = 0f;

        ValidateUIReferences();

        if (mainMenu != null) mainMenu.SetActive(false);
        if (deathScreen != null && deathScreen.activeSelf) deathScreen.SetActive(false);

        if (inGameUI != null) inGameUI.SetActive(true);
        else
        {
            Debug.LogWarning("inGameUI не найден, пытаемся найти...");
            inGameUI = GameObject.Find("MainScreen");
            if (inGameUI != null) inGameUI.SetActive(true);
        }

        if (player != null)
        {
            player.ResetPlayer();
        }
        else
        {
            FindPlayer();
            if (player != null) player.ResetPlayer();
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

        ValidateUIReferences();

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

        UpdateUI();
    }

    public void RestartGame()
    {
        Debug.Log("=== RESTART GAME ===");

        isGameRunning = false;
        gameTime = 0f;
        score = 0;
        lastUIUpdateTime = 0f;

        StopAllCoroutines();

        GameObject[] fireballs = GameObject.FindGameObjectsWithTag("Fireball");
        foreach (GameObject fb in fireballs)
        {
            if (fb != null) Destroy(fb);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        StartCoroutine(InitializeAfterRestart());
    }

    private IEnumerator InitializeAfterRestart()
    {
        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(0.1f);

        FindPlayer();
        ValidateUIReferences();

        ShowMainMenu();

        // Обновляем UI СРАЗУ
        UpdateUI();

        Debug.Log("Игра перезапущена, UI обновлен");
    }

    void UpdateUI()
    {
        if (scoreText == null || timeText == null || shieldCountText == null)
        {
            Debug.LogWarning("UI элементы не найдены, пытаемся найти...");
            FindUIElements();

            if (scoreText == null || timeText == null)
            {
                Debug.LogError("Критические UI элементы не найдены, пропускаем обновление");
                return;
            }
        }

        if (!scoreText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("ScoreText не активен в иерархии, активируем...");
            ActivateUIText(scoreText);
        }

        if (!timeText.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TimeText не активен в иерархии, активируем...");
            ActivateUIText(timeText);
        }

        if (isGameRunning)
        {
            scoreText.text = $"Score: {score}";
            timeText.text = $"Time: {gameTime:F1}s";
        }
        else
        {
            scoreText.text = $"Score: {score}";
            timeText.text = $"Time: {gameTime:F1}s";
        }

        if (shieldCountText != null)
        {
            int shieldCount = PowerUpManager.Instance != null ?
                PowerUpManager.Instance.GetShieldCount() : 0;
            shieldCountText.text = $"Shields: {shieldCount}";

            shieldCountText.gameObject.SetActive(shieldCount > 0);
        }

        Debug.Log($"UI обновлен: Score={score}, Time={gameTime:F1}, GameRunning={isGameRunning}");
    }

    private void ActivateUIText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        textComponent.gameObject.SetActive(true);

        Transform parent = textComponent.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(true);
            }
            parent = parent.parent;
        }
    }

    private void ValidateUIReferences()
    {
        if (isFindingUI) return;
        isFindingUI = true;

        if (scoreText == null || timeText == null)
        {
            Debug.Log("Критические UI элементы отсутствуют, запускаем поиск...");
            FindUIElements();
        }

        if (deathScreen == null)
        {
            deathScreen = GameObject.Find("DeathScreen");
            if (deathScreen != null) Debug.Log("Найден DeathScreen");
        }

        if (mainMenu == null)
        {
            mainMenu = GameObject.Find("MainMenu");
            if (mainMenu != null) Debug.Log("Найден MainMenu");
        }

        if (inGameUI == null)
        {
            inGameUI = GameObject.Find("MainScreen");
            if (inGameUI != null) Debug.Log("Найден MainScreen (inGameUI)");
        }

        isFindingUI = false;
    }

    private void FindUIElements()
    {
        Debug.Log("Поиск UI элементов в сцене...");

        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null) continue;

            if (text.gameObject.name == "ScoreText" ||
                text.gameObject.name.Contains("Score"))
            {
                scoreText = text;
                Debug.Log($"Найден ScoreText: {text.gameObject.name}");
            }
            else if (text.gameObject.name == "TimeText" ||
                     text.gameObject.name.Contains("Time"))
            {
                timeText = text;
                Debug.Log($"Найден TimeText: {text.gameObject.name}");
            }
            else if (text.gameObject.name == "ShieldCountText" ||
                     text.gameObject.name.Contains("Shield"))
            {
                shieldCountText = text;
                Debug.Log($"Найден ShieldCountText: {text.gameObject.name}");
            }
        }

        if (scoreText == null) Debug.LogError("ScoreText не найден!");
        if (timeText == null) Debug.LogError("TimeText не найден!");
        if (shieldCountText == null) Debug.Log("ShieldCountText не найден (не критично)");
    }

    void ShowMainMenu()
    {
        ValidateUIReferences();

        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MainMenu не найден при попытке показать");
        }

        if (deathScreen != null && deathScreen.activeSelf)
        {
            deathScreen.SetActive(false);
        }

        if (inGameUI != null)
        {
            inGameUI.SetActive(false);
        }
    }

    void FindPlayer()
    {
        player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("Player не найден на сцене! Будет повторная попытка позже.");
        }
        else
        {
            Debug.Log("Player найден!");
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
        Debug.Log($"Player назначен: {player != null}");
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}