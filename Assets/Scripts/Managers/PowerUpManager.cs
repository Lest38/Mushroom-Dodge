using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("UI элементы")]
    [SerializeField] private Image powerUpIcon;
    [SerializeField] private TextMeshProUGUI powerUpTimerText;
    [SerializeField] private TextMeshProUGUI powerUpCountText;
    [SerializeField] private Button usePowerUpButton;

    [Header("Эффекты")]
    [SerializeField] private GameObject shieldEffect;

    [Header("Настройки")]
    [SerializeField] private float shieldDuration = 10f;
    [SerializeField] private Sprite shieldSprite;

    private int shieldCount = 0;
    private float powerUpTimer = 0f;
    private bool isPowerUpActive = false;
    private PlayerController playerController;

    private const string COUNT_FORMAT = ".{0}";
    private const string BUTTON_FORMAT = "SHIELD ({0})";
    private const string TIMER_ACTIVE_FORMAT = "SHIELD {0}s";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        SetupButton();
        RefreshPowerUp();
    }

    void SetupButton()
    {
        if (usePowerUpButton == null) return;

        usePowerUpButton.onClick.RemoveAllListeners();
        usePowerUpButton.onClick.AddListener(UsePowerUp);
        usePowerUpButton.gameObject.SetActive(false);
    }

    public void RefreshPowerUp()
    {
        shieldCount = 0;

        if (ShopManager.Instance != null)
        {
            var shieldItem = ShopManager.Instance.GetEquippedItem(ShopItem.ItemType.Shield);
            if (shieldItem?.isPurchased == true && shieldItem.isEquipped)
            {
                shieldCount = shieldItem.quantity;
                if (powerUpIcon != null)
                    powerUpIcon.sprite = shieldItem.icon != null ? shieldItem.icon : shieldSprite;
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        bool hasShield = shieldCount > 0;

        powerUpIcon?.gameObject.SetActive(hasShield);

        if (powerUpCountText != null)
        {
            powerUpCountText.gameObject.SetActive(hasShield);
            if (hasShield)
                powerUpCountText.text = string.Format(COUNT_FORMAT, shieldCount);
        }

        if (usePowerUpButton != null)
        {
            bool showButton = hasShield && !isPowerUpActive && GameManager.Instance?.IsGameRunning == true;
            usePowerUpButton.gameObject.SetActive(showButton);
            usePowerUpButton.interactable = showButton;

            var buttonText = usePowerUpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && showButton)
                buttonText.text = string.Format(BUTTON_FORMAT, shieldCount);
        }

        if (powerUpTimerText != null)
        {
            powerUpTimerText.gameObject.SetActive(isPowerUpActive);
            if (isPowerUpActive)
                powerUpTimerText.text = string.Format(TIMER_ACTIVE_FORMAT, (int)powerUpTimer);
        }
    }

    public void UsePowerUp()
    {
        if (shieldCount <= 0 || isPowerUpActive) return;
        if (GameManager.Instance?.IsGameRunning != true) return;

        isPowerUpActive = true;
        powerUpTimer = shieldDuration;

        UpdateUI();

        ActivateShield();

        ShopManager.Instance?.ConsumeShield();

        CancelInvoke(nameof(EndPowerUp));
        Invoke(nameof(EndPowerUp), shieldDuration);
    }

    void ActivateShield()
    {
        shieldEffect?.SetActive(true);
        playerController?.EnableShield(shieldDuration);
    }

    void EndPowerUp()
    {
        isPowerUpActive = false;
        powerUpTimer = 0f;
        shieldEffect?.SetActive(false);

        RefreshPowerUp();

        UpdateUI();
    }

    void Update()
    {
        if (isPowerUpActive && powerUpTimer > 0)
        {
            powerUpTimer -= Time.deltaTime;

            if (powerUpTimerText != null)
                powerUpTimerText.text = string.Format(TIMER_ACTIVE_FORMAT, (int)powerUpTimer);

            if (powerUpTimer <= 0) EndPowerUp();
        }
    }

    public void OnGameStateChanged(bool isGameRunning) => UpdateUI();
    public bool IsShieldActive() => isPowerUpActive;
    public int GetShieldCount() => shieldCount;
}