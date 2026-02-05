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
        // Если кнопки не назначены в Inspector - находим автоматически
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

        // Настраиваем кнопку Restart
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

        // Настраиваем кнопку Menu
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
        // Скрываем DeathScreen
        gameObject.SetActive(false);

        // Пытаемся найти и показать MainMenu
        GameObject mainMenuObj = GameObject.Find("MainMenu");
        if (mainMenuObj != null)
        {
            mainMenuObj.SetActive(true);
            Debug.Log("MainMenu показан");
        }
        else
        {
            Debug.LogWarning("MainMenu не найден по имени!");

            // Альтернативный поиск
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

        // Если GameManager доступен, можем вызвать его метод
        if (GameManager.Instance != null)
        {
            // Можем сбросить счет если нужно
            // GameManager.Instance.ResetScore(); // если есть такой метод
        }
    }

    // Метод с 4 параметрами
    public void ShowResults(float time, int score, int highScore, int earnedSpores)
    {
        ShowResults(time, score, highScore, earnedSpores, score > highScore);
    }

    // Метод с 5 параметрами
    public void ShowResults(float time, int score, int highScore, int earnedSpores, bool isNewRecord)
    {
        Debug.Log($"DeathScreenUI.ShowResults вызван");

        // Обновляем тексты
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

        // Показываем "Новый рекорд!" если нужно
        if (newRecordText != null)
        {
            newRecordText.SetActive(isNewRecord);
        }

        // Активируем панель
        gameObject.SetActive(true);
    }
}