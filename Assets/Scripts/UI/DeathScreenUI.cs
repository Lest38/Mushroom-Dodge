using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI sporesEarnedText;
    [SerializeField] private GameObject newRecordText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        if (restartButton == null || menuButton == null)
        {
            Button[] allButtons = GetComponentsInChildren<Button>();

            if (allButtons.Length >= 2)
            {
                restartButton = allButtons[0];
                menuButton = allButtons[1];
                Debug.Log("Кнопки найдены автоматически");
            }
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() =>
            {
                Debug.Log("Нажата кнопка Restart");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
        else
        {
            Debug.LogWarning("Кнопка Restart не найдена!");
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() =>
            {
                Debug.Log("Нажата кнопка Menu");
                ReturnToMainMenu();
            });
        }
        else
        {
            Debug.LogWarning("Кнопка Menu не найдена!");
        }
    }

    void ReturnToMainMenu()
    {
        gameObject.SetActive(false);

        GameObject mainMenuObj = GameObject.Find("MainMenu");
        if (mainMenuObj != null)
        {
            mainMenuObj.SetActive(true);
            Debug.Log("MainMenu показан");
        }
        else
        {
            Debug.LogWarning("MainMenu не найден по имени!");

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                foreach (Transform child in canvas.transform)
                {
                    if (child.name.Contains("Menu") || child.name.Contains("Main"))
                    {
                        child.gameObject.SetActive(true);
                        Debug.Log($"Найден и показан: {child.name}");
                        break;
                    }
                }
            }
        }
    }

    public void ShowResults(float time, int score, int highScore, int earnedSpores)
    {
        ShowResults(time, score, highScore, earnedSpores, score > highScore);
    }

    public void ShowResults(float time, int score, int highScore, int earnedSpores, bool isNewRecord)
    {
        Debug.Log($"DeathScreenUI.ShowResults вызван");

        if (timeText != null)
        {
            timeText.text = $"Time: {time:F1}s";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"Record: {highScore}";
        }

        if (sporesEarnedText != null)
        {
            sporesEarnedText.text = $"+{earnedSpores} spores";
        }

        if (newRecordText != null)
        {
            newRecordText.SetActive(isNewRecord);
        }

        gameObject.SetActive(true);
    }
}