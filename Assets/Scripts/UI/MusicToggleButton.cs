using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicToggleButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Sprites")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnButtonClick);
        }

        UpdateVisuals();
    }

    void OnButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic();
            AudioManager.Instance.PlaySFX("ButtonClick");

            UpdateVisuals();
        }
    }

    void OnEnable()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (AudioManager.Instance == null) return;

        bool isOn = AudioManager.Instance.IsMusicEnabled();

        if (iconImage != null)
        {
            iconImage.sprite = isOn ? musicOnSprite : musicOffSprite;
            iconImage.color = isOn ? Color.green : Color.red;
        }

        if (statusText != null)
        {
            statusText.text = isOn ? "MUSIC: ON" : "MUSIC: OFF";
            statusText.color = isOn ? Color.green : Color.red;
        }
    }
}