using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Image speedBar;

    void Start()
    {
        if (speedText == null)
            speedText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (DifficultyManager.Instance != null && GameManager.Instance != null &&
            GameManager.Instance.IsGameRunning)
        {
            float speedPercent = DifficultyManager.Instance.GetCurrentSpeedPercent();
            float multiplier = DifficultyManager.Instance.GetCurrentMultiplier();

            speedText.text = $"Speed: {speedPercent:F0}%\nx{multiplier:F2}";

            // Μενÿεμ φβες
            if (speedPercent < 33)
                speedText.color = Color.green;
            else if (speedPercent < 66)
                speedText.color = Color.yellow;
            else
                speedText.color = Color.red;
        }
        else
        {
            speedText.text = "Speed: -";
            speedText.color = Color.gray;
        }
    }
}