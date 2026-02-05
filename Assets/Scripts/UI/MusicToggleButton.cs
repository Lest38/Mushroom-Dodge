using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicToggleButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Sprites")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [Header("Colors")]
    [SerializeField] private Color enabledColor = Color.green;
    [SerializeField] private Color disabledColor = Color.red;

    [Header("Texts")]
    [SerializeField] private string musicOnText = "MUSIC: ON";
    [SerializeField] private string musicOffText = "MUSIC: OFF";

    void Start()
    {
        if (toggleButton == null)
        {
            toggleButton = GetComponent<Button>();
            if (toggleButton == null)
            {
                toggleButton = GetComponentInChildren<Button>();
            }
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(OnToggleButtonClick);
            toggleButton.onClick.AddListener(() => {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX("ButtonClick");
            });
        }
        else
        {
            Debug.LogError(" нопка не найдена на MusicToggleButton!");
        }

        UpdateVisuals();
    }

    void OnToggleButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic();
            UpdateVisuals();
        }
        else
        {
            Debug.LogError("AudioManager не найден!");
        }
    }

    void UpdateVisuals()
    {
        if (AudioManager.Instance == null) return;

        bool isMusicEnabled = AudioManager.Instance.IsMusicEnabled();

        if (iconImage != null)
        {
            iconImage.sprite = isMusicEnabled ? musicOnSprite : musicOffSprite;
            iconImage.color = isMusicEnabled ? enabledColor : disabledColor;
        }

        if (statusText != null)
        {
            statusText.text = isMusicEnabled ? musicOnText : musicOffText;
            statusText.color = isMusicEnabled ? enabledColor : disabledColor;
        }

        if (toggleButton != null)
        {
            ColorBlock colors = toggleButton.colors;
            colors.normalColor = isMusicEnabled ? enabledColor : disabledColor;
            colors.highlightedColor = isMusicEnabled ?
                Color.Lerp(enabledColor, Color.white, 0.3f) :
                Color.Lerp(disabledColor, Color.white, 0.3f);
            toggleButton.colors = colors;
        }
    }

    void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            UpdateVisuals();
        }
    }
}