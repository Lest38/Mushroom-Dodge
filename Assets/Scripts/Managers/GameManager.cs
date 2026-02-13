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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isGameRunning)
        {
            StartGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isGameRunning)
        {
            PlayerDied();
        }

        if (isGameRunning)
        {
            float oldTime = gameTime;
            int oldScore = score;

            gameTime += Time.deltaTime;
            score = Mathf.FloorToInt(gameTime * 10);

            if (gameTime < 5f && Mathf.Floor(gameTime) != Mathf.Floor(oldTime))
            {
                GameObject[] fireballs = GameObject.FindGameObjectsWithTag("Fireball");
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
            inGameUI = GameObject.Find("MainScreen");
            if (inGameUI != null) inGameUI.SetActive(true);
        }


        AudioManager.Instance.PlayGameMusic();

        if (player != null)
        {
            player.ResetPlayer();
        }
        else
        {
            FindPlayer();
            if (player != null) player.ResetPlayer();
        }

        StartCoroutine(ResetPlayerWithDelay());

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.StartSpawning();
        }
        else
        {
            SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
            if (spawnManager != null)
            {
                spawnManager.StartSpawning();
            }
        }

        UpdateUI();

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnGameStateChanged(true);
        }
    }

    private IEnumerator StartMusicWithDelay()
    {
        yield return new WaitForEndOfFrame();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }
    }

    private IEnumerator ResetPlayerWithDelay()
    {
        yield return new WaitForEndOfFrame();

        if (player != null)
        {
            player.ResetPlayer();
        }
        else
        {
            FindPlayer();
            if (player != null)
            {
                yield return new WaitForEndOfFrame();
                player.ResetPlayer();
            }
        }
    }

    public void PlayerDied()
    {
        if (!isGameRunning) return;

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

        bool isNewRecord = false;
        if (score > highScore)
        {
            highScore = score;
            isNewRecord = true;
        }

        SaveProgress();

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);

            DeathScreenUI deathUI = deathScreen.GetComponent<DeathScreenUI>();
            if (deathUI != null)
            {
                deathUI.ShowResults(gameTime, score, highScore, earnedSpores, isNewRecord);
            }
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

        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText == null || timeText == null || shieldCountText == null)
        {
            FindUIElements();

            if (scoreText == null || timeText == null)
            {
                return;
            }
        }

        if (!scoreText.gameObject.activeInHierarchy)
        {
            ActivateUIText(scoreText);
        }

        if (!timeText.gameObject.activeInHierarchy)
        {
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
            FindUIElements();
        }

        if (deathScreen == null)
        {
            deathScreen = GameObject.Find("DeathScreen");
        }

        if (mainMenu == null)
        {
            mainMenu = GameObject.Find("MainMenu");
        }

        if (inGameUI == null)
        {
            inGameUI = GameObject.Find("MainScreen");
        }

        isFindingUI = false;
    }

    private void FindUIElements()
    {

        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null) continue;

            if (text.gameObject.name == "ScoreText" ||
                text.gameObject.name.Contains("Score"))
            {
                scoreText = text;
            }
            else if (text.gameObject.name == "TimeText" ||
                     text.gameObject.name.Contains("Time"))
            {
                timeText = text;
            }
            else if (text.gameObject.name == "ShieldCountText" ||
                     text.gameObject.name.Contains("Shield"))
            {
                shieldCountText = text;
            }
        }
    }

    void ShowMainMenu()
    {
        ValidateUIReferences();

        if (mainMenu != null)
        {
            mainMenu.SetActive(true);
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
    }

    void SaveProgress()
    {

        try
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.SetInt("Spores", spores);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{e.Message}");
        }
    }

    void LoadProgress()
    {
        try
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
            spores = PlayerPrefs.GetInt("Spores", 0);
        }
        catch (System.Exception e)
        {
            highScore = 0;
            spores = 0;
        }
    }

    public bool IsGameRunning => isGameRunning;

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}