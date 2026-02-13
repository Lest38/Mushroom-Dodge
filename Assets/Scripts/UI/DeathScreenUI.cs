using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        if (restartButton == null || menuButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>();
            if (buttons.Length >= 2)
            {
                restartButton = buttons[0];
                menuButton = buttons[1];
            }
        }

        restartButton?.onClick.AddListener(GameManager.Instance.RestartGame);
        menuButton?.onClick.AddListener(ReturnToMainMenu);
    }

    void ReturnToMainMenu()
    {
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameObject.Find("MainMenu")?.SetActive(true);
        }
    }

    public void ShowResults(float time, int score, int highScore, int earnedSpores, bool isNewRecord)
    {
        if (timeText != null) timeText.text = $"Time: {time:F1}s";
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (highScoreText != null) highScoreText.text = $"Record: {highScore}";
        if (sporesEarnedText != null) sporesEarnedText.text = $"+{earnedSpores} spores";
        if (newRecordText != null) newRecordText.SetActive(isNewRecord);

        gameObject.SetActive(true);
    }
}