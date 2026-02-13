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
    [SerializeField] private GameObject slowTimeEffect;

    [Header("Настройки")]
    [SerializeField] private float shieldDuration = 10f;
    [SerializeField] private Sprite shieldSprite;

    private bool hasShieldItem = false;
    private int shieldCount = 0;
    private float powerUpTimer = 0f;
    private bool isPowerUpActive = false;
    private PlayerController playerController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();

        InitializeUI();

        Invoke(nameof(LoadEquippedPowerUp), 0.1f);
    }

    void InitializeUI()
    {
        if (usePowerUpButton == null)
        {
            return;
        }

        usePowerUpButton.onClick.RemoveAllListeners();
        usePowerUpButton.onClick.AddListener(UsePowerUp);

        usePowerUpButton.gameObject.SetActive(false);
        usePowerUpButton.interactable = false;
    }

    public void Reinitialize()
    {
        playerController = FindObjectOfType<PlayerController>();

        LoadEquippedPowerUp();

        isPowerUpActive = false;
        powerUpTimer = 0f;

        UpdateUI();
    }

    void LoadEquippedPowerUp()
    {
        hasShieldItem = false;
        shieldCount = 0;

        if (ShopManager.Instance != null)
        {
            ShopItem shieldItem = ShopManager.Instance.GetEquippedItem(ShopItem.ItemType.Shield);

            if (shieldItem != null && shieldItem.isPurchased && shieldItem.isEquipped)
            {
                hasShieldItem = true;
                shieldCount = shieldItem.quantity;

                if (powerUpIcon != null)
                {
                    powerUpIcon.sprite = shieldItem.icon != null ? shieldItem.icon : shieldSprite;
                }
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (powerUpIcon != null)
        {
            powerUpIcon.gameObject.SetActive(hasShieldItem && shieldCount > 0);
        }

        if (usePowerUpButton != null)
        {
            bool showButton = hasShieldItem && shieldCount > 0 && !isPowerUpActive;
            usePowerUpButton.gameObject.SetActive(showButton);

            bool canUse = showButton &&
                         GameManager.Instance != null &&
                         GameManager.Instance.IsGameRunning;

            usePowerUpButton.interactable = canUse;

            TextMeshProUGUI buttonText = usePowerUpButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"USE SHIELD ({shieldCount})";
            }
        }

        if (powerUpCountText != null)
        {
            powerUpCountText.gameObject.SetActive(hasShieldItem && shieldCount > 0);
            if (hasShieldItem && shieldCount > 0)
            {
                powerUpCountText.text = $"x{shieldCount}";
            }
        }

        if (powerUpTimerText != null)
        {
            powerUpTimerText.gameObject.SetActive(isPowerUpActive);
            if (isPowerUpActive)
            {
                powerUpTimerText.text = $"SHIELD: {(int)powerUpTimer}s";
            }
        }
    }

    public void UsePowerUp()
    {
        if (!hasShieldItem || shieldCount <= 0)
        {
            return;
        }

        if (isPowerUpActive)
        {
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            return;
        }

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ConsumeShield();
        }

        shieldCount--;

        isPowerUpActive = true;
        powerUpTimer = shieldDuration;

        UpdateUI();

        ActivateShieldEffect();

        CancelInvoke(nameof(EndPowerUp));
        Invoke(nameof(EndPowerUp), powerUpTimer);
    }

    void ActivateShieldEffect()
    {

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);
        }

        if (playerController != null)
        {
            playerController.EnableShield(powerUpTimer);
        }
    }

    void EndPowerUp()
    {
        isPowerUpActive = false;
        powerUpTimer = 0f;

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);
        }

        UpdateUI();
    }

    public void OnShieldUsed()
    {
        EndPowerUp();
    }

    public void OnShieldTimeEnded()
    {
        EndPowerUp();
    }

    private void Update()
    {
        if (isPowerUpActive && powerUpTimer > 0)
        {
            powerUpTimer -= Time.deltaTime;

            if (powerUpTimerText != null && powerUpTimerText.gameObject.activeSelf)
            {
                powerUpTimerText.text = $"SHIELD: {(int)powerUpTimer}s";
            }

            if (powerUpTimer <= 0)
            {
                EndPowerUp();
            }
        }
    }

    public void OnGameStateChanged(bool isGameRunning)
    {
        UpdateUI();
    }

    public void RefreshPowerUp()
    {
        LoadEquippedPowerUp();
    }

    public bool IsShieldActive()
    {
        return isPowerUpActive;
    }

    public int GetShieldCount()
    {
        return shieldCount;
    }
}