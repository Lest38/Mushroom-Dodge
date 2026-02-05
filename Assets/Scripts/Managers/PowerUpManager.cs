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
        Debug.Log("=== PowerUpManager Start ===");

        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController не найден!");
        }

        InitializeUI();

        Invoke(nameof(LoadEquippedPowerUp), 0.1f);
    }

    void InitializeUI()
    {
        Debug.Log("Инициализация UI PowerUp");

        if (usePowerUpButton == null)
        {
            Debug.LogError("UsePowerUpButton не назначена!");
            return;
        }

        usePowerUpButton.onClick.RemoveAllListeners();
        usePowerUpButton.onClick.AddListener(UsePowerUp);

        usePowerUpButton.gameObject.SetActive(false);
        usePowerUpButton.interactable = false;
    }

    public void Reinitialize()
    {
        Debug.Log("PowerUpManager реинициализация");

        playerController = FindObjectOfType<PlayerController>();

        LoadEquippedPowerUp();

        isPowerUpActive = false;
        powerUpTimer = 0f;

        UpdateUI();
    }

    void LoadEquippedPowerUp()
    {
        Debug.Log("=== Загрузка экипированного бонуса ===");

        hasShieldItem = false;
        shieldCount = 0;

        if (ShopManager.Instance != null)
        {
            ShopItem shieldItem = ShopManager.Instance.GetEquippedItem(ShopItem.ItemType.Shield);

            if (shieldItem != null && shieldItem.isPurchased && shieldItem.isEquipped)
            {
                hasShieldItem = true;
                shieldCount = shieldItem.quantity;

                Debug.Log($"Бонус загружен из магазина: {shieldItem.itemName}, количество: {shieldCount}");

                if (powerUpIcon != null)
                {
                    powerUpIcon.sprite = shieldItem.icon != null ? shieldItem.icon : shieldSprite;
                }
            }
            else
            {
                Debug.Log("В магазине нет экипированного щита или щиты закончились");
            }
        }
        else
        {
            Debug.LogWarning("ShopManager не найден");
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        Debug.Log($"Обновление UI PowerUp: есть щит={hasShieldItem}, количество={shieldCount}, активен={isPowerUpActive}");

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
        Debug.Log("=== НАЖАТА КНОПКА POWER UP ===");

        if (!hasShieldItem || shieldCount <= 0)
        {
            Debug.LogWarning("Нет щитов для использования!");
            return;
        }

        if (isPowerUpActive)
        {
            Debug.LogWarning("Щит уже активен!");
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            Debug.LogWarning("Игра не запущена!");
            return;
        }

        Debug.Log($"Активация щита. Осталось щитов: {shieldCount}");

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
        Debug.Log($"Активация эффекта щита на {powerUpTimer} секунд");

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);
            Debug.Log("Визуальный эффект щита включен");
        }

        if (playerController != null)
        {
            playerController.EnableShield(powerUpTimer);
            Debug.Log("Щит передан игроку");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Shield");
        }
    }

    void EndPowerUp()
    {
        Debug.Log("=== Завершение бонуса ===");

        isPowerUpActive = false;
        powerUpTimer = 0f;

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);
        }

        UpdateUI();

        Debug.Log("Бонус завершен");
    }

    public void OnShieldUsed()
    {
        Debug.Log("Щит использован для защиты от файербола");
        EndPowerUp();
    }

    public void OnShieldTimeEnded()
    {
        Debug.Log("Время щита истекло");
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
        Debug.Log($"PowerUpManager: состояние игры изменилось на {isGameRunning}");
        UpdateUI();
    }

    public void RefreshPowerUp()
    {
        Debug.Log("Обновление бонуса из магазина");
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