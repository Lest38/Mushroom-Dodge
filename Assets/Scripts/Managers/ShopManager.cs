using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Настройки магазина")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform shopItemsContainer;

    [Header("UI элементы")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI sporesCountText;
    [SerializeField] private Button closeShopButton;
    [SerializeField] private Button openShopButton;

    [Header("Выбранный предмет")]
    [SerializeField] private ShopItem selectedItem;
    [SerializeField] private TextMeshProUGUI selectedItemName;
    [SerializeField] private TextMeshProUGUI selectedItemDescription;
    [SerializeField] private TextMeshProUGUI selectedItemCost;
    [SerializeField] private TextMeshProUGUI selectedItemQuantity;
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button useButton;

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
        LoadShopData();

        InitializeUI();

        UpdateSporesDisplay();

        shopPanel.SetActive(false);
    }

    void InitializeUI()
    {
        if (openShopButton != null)
        {
            openShopButton.onClick.AddListener(OpenShop);
        }

        if (closeShopButton != null)
        {
            closeShopButton.onClick.AddListener(CloseShop);
        }

        CreateShopItems();

        if (shopItems.Count > 0)
        {
            SelectItem(shopItems[0]);
        }
    }

    void CreateShopItems()
    {
        foreach (Transform child in shopItemsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in shopItems)
        {
            GameObject itemUI = Instantiate(shopItemPrefab, shopItemsContainer);

            ShopItemUI itemUIComponent = itemUI.GetComponent<ShopItemUI>();
            if (itemUIComponent != null)
            {
                itemUIComponent.Initialize(item, this);
            }

            Button itemButton = itemUI.GetComponent<Button>();
            if (itemButton != null)
            {
                itemButton.onClick.AddListener(() => SelectItem(item));
            }
        }
    }

    public void SelectItem(ShopItem item)
    {
        selectedItem = item;

        if (selectedItemName != null) selectedItemName.text = item.itemName;
        if (selectedItemDescription != null) selectedItemDescription.text = item.description;
        if (selectedItemCost != null) selectedItemCost.text = $"Cost: {item.cost} spores";
        if (selectedItemIcon != null) selectedItemIcon.sprite = item.icon;

        if (selectedItemQuantity != null)
        {
            if (item.isConsumable && item.isPurchased)
            {
                selectedItemQuantity.text = $"Quantity: {item.quantity}";
                selectedItemQuantity.gameObject.SetActive(true);
            }
            else
            {
                selectedItemQuantity.gameObject.SetActive(false);
            }
        }

        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (selectedItem == null) return;

        if (buyButton != null)
        {
            bool canBuy = false;

            if (selectedItem.isConsumable)
            {
                canBuy = GameManager.Instance.spores >= selectedItem.cost;
                buyButton.GetComponentInChildren<TextMeshProUGUI>().text = "BUY";
            }
            else
            {
                canBuy = !selectedItem.isPurchased &&
                        GameManager.Instance.spores >= selectedItem.cost;
                buyButton.GetComponentInChildren<TextMeshProUGUI>().text =
                    selectedItem.isPurchased ? "PURCHASED" : "BUY";
            }

            buyButton.interactable = canBuy;
        }

        if (useButton != null)
        {
            if (selectedItem.isConsumable)
            {
                useButton.interactable = selectedItem.isPurchased &&
                                       selectedItem.quantity > 0;
                useButton.GetComponentInChildren<TextMeshProUGUI>().text =
                    selectedItem.quantity > 0 ? "EQUIP" : "OUT OF STOCK";
            }
            else
            {
                useButton.interactable = selectedItem.isPurchased && !selectedItem.isEquipped;
                useButton.GetComponentInChildren<TextMeshProUGUI>().text =
                    selectedItem.isEquipped ? "EQUIPPED" : "EQUIP";
            }
        }
    }

    public void BuySelectedItem()
    {
        if (selectedItem == null) return;

        if (GameManager.Instance.spores >= selectedItem.cost)
        {
            GameManager.Instance.spores -= selectedItem.cost;

            if (selectedItem.isConsumable)
            {
                if (!selectedItem.isPurchased)
                {
                    selectedItem.isPurchased = true;
                    selectedItem.quantity = 1;
                }
                else
                {
                    selectedItem.quantity++;
                }
            }
            else
            {
                selectedItem.isPurchased = true;
            }

            SaveShopData();

            UpdateSporesDisplay();
            UpdateButtons();

            if (selectedItemQuantity != null && selectedItem.isConsumable)
            {
                selectedItemQuantity.text = $"Quantity: {selectedItem.quantity}";
            }

            Debug.Log($"Куплен предмет: {selectedItem.itemName}, количество: {selectedItem.quantity}");

            if (selectedItem.itemType == ShopItem.ItemType.Shield && selectedItem.quantity == 1)
            {
                EquipSelectedItem();
            }
        }
        else
        {
            Debug.Log("Недостаточно спор!");
        }
    }

    public void EquipSelectedItem()
    {
        if (selectedItem == null || !selectedItem.isPurchased) return;

        if (selectedItem.isConsumable && selectedItem.quantity <= 0)
        {
            Debug.LogWarning($"Нет {selectedItem.itemName} для экипировки!");
            return;
        }

        foreach (ShopItem item in shopItems)
        {
            if (item.itemType == selectedItem.itemType)
            {
                item.isEquipped = false;
            }
        }

        selectedItem.isEquipped = true;

        SaveShopData();

        UpdateButtons();

        CreateShopItems();

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.RefreshPowerUp();
        }

        Debug.Log($"Экипирован предмет: {selectedItem.itemName}, осталось: {selectedItem.quantity}");
    }

    public void ConsumeShield()
    {
        ShopItem shieldItem = GetEquippedItem(ShopItem.ItemType.Shield);

        if (shieldItem != null && shieldItem.isConsumable && shieldItem.quantity > 0)
        {
            shieldItem.quantity--;

            if (shieldItem.quantity <= 0)
            {
                shieldItem.isEquipped = false;
                Debug.Log("Щиты закончились!");
            }

            SaveShopData();

            if (selectedItem == shieldItem)
            {
                UpdateButtons();
                if (selectedItemQuantity != null)
                {
                    selectedItemQuantity.text = $"Quantity: {shieldItem.quantity}";
                }
            }

            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.RefreshPowerUp();
            }

            Debug.Log($"Использован щит. Осталось: {shieldItem.quantity}");
        }
    }

    public int GetShieldCount()
    {
        ShopItem shieldItem = GetEquippedItem(ShopItem.ItemType.Shield);
        return (shieldItem != null && shieldItem.isPurchased) ? shieldItem.quantity : 0;
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdateSporesDisplay();
        Debug.Log("Магазин открыт");
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        Debug.Log("Магазин закрыт");
    }

    void UpdateSporesDisplay()
    {
        if (sporesCountText != null)
        {
            sporesCountText.text = $"Spores: {GameManager.Instance.spores}";
        }
    }

    void SaveShopData()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            PlayerPrefs.SetInt($"ShopItem_{i}_Purchased", shopItems[i].isPurchased ? 1 : 0);
            PlayerPrefs.SetInt($"ShopItem_{i}_Equipped", shopItems[i].isEquipped ? 1 : 0);
            PlayerPrefs.SetInt($"ShopItem_{i}_Quantity", shopItems[i].quantity);
        }
        PlayerPrefs.Save();
    }

    void LoadShopData()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItems[i].isPurchased = PlayerPrefs.GetInt($"ShopItem_{i}_Purchased", 0) == 1;
            shopItems[i].isEquipped = PlayerPrefs.GetInt($"ShopItem_{i}_Equipped", 0) == 1;
            shopItems[i].quantity = PlayerPrefs.GetInt($"ShopItem_{i}_Quantity",
                shopItems[i].isConsumable ? 0 : 1);
        }
    }

    public ShopItem GetEquippedItem(ShopItem.ItemType type)
    {
        foreach (ShopItem item in shopItems)
        {
            if (item.itemType == type && item.isEquipped && item.isPurchased)
            {
                return item;
            }
        }
        return null;
    }


    public void Reinitialize()
    {
        Debug.Log("ShopManager реинициализация");

        LoadShopData();

        UpdateSporesDisplay();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

}