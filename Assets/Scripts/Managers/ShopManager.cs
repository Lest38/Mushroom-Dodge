using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Настройки магазина")]
    [SerializeField] private List<ShopItem> shopItems = new();
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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadShopData();
        SetupButtons();
        CreateShopItems();
        shopPanel.SetActive(false);
        UpdateSporesDisplay();

        if (shopItems.Count > 0) SelectItem(shopItems[0]);
    }

    void SetupButtons()
    {
        openShopButton?.onClick.AddListener(OpenShop);
        closeShopButton?.onClick.AddListener(CloseShop);
        buyButton?.onClick.AddListener(BuySelectedItem);
        useButton?.onClick.AddListener(EquipSelectedItem);
    }

    void CreateShopItems()
    {
        foreach (Transform child in shopItemsContainer) Destroy(child.gameObject);

        foreach (var item in shopItems)
        {
            var itemUI = Instantiate(shopItemPrefab, shopItemsContainer);
            itemUI.GetComponent<ShopItemUI>()?.Initialize(item);
            itemUI.GetComponent<Button>()?.onClick.AddListener(() => SelectItem(item));
        }
    }

    public void SelectItem(ShopItem item)
    {
        selectedItem = item;

        if (selectedItemName != null) selectedItemName.text = item.itemName;
        if (selectedItemDescription != null) selectedItemDescription.text = item.description;
        if (selectedItemCost != null) selectedItemCost.text = $"Cost: {item.cost} spores";

        if (selectedItemQuantity != null)
        {
            bool showQuantity = item.isConsumable && item.isPurchased;
            selectedItemQuantity.gameObject.SetActive(showQuantity);
            if (showQuantity) selectedItemQuantity.text = $"Quantity: {item.quantity}";
        }

        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (selectedItem == null) return;

        if (buyButton != null)
        {
            bool isConsumable = selectedItem.isConsumable;
            bool hasEnoughSpores = GameManager.Instance.spores >= selectedItem.cost;

            bool canBuy = isConsumable
                ? hasEnoughSpores
                : !selectedItem.isPurchased && hasEnoughSpores;

            buyButton.interactable = canBuy;
            var btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();

            if (!isConsumable && selectedItem.isPurchased)
                btnText.text = "PURCHASED";
            else
                btnText.text = "BUY";
        }

        if (useButton != null)
        {
            var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();

            if (selectedItem.isConsumable)
            {
                bool hasItem = selectedItem.isPurchased && selectedItem.quantity > 0;
                useButton.interactable = hasItem;
                btnText.text = hasItem ? "EQUIP" : "OUT OF STOCK";
            }
            else
            {
                bool canEquip = selectedItem.isPurchased && !selectedItem.isEquipped;
                useButton.interactable = canEquip;
                btnText.text = selectedItem.isEquipped ? "EQUIPPED" : "EQUIP";
            }
        }
    }

    public void BuySelectedItem()
    {
        if (selectedItem == null) return;
        if (GameManager.Instance.spores < selectedItem.cost) return;

        GameManager.Instance.spores -= selectedItem.cost;

        if (selectedItem.isConsumable)
        {
            selectedItem.isPurchased = true;
            selectedItem.quantity++;
        }
        else
        {
            selectedItem.isPurchased = true;
        }

        SaveShopData();
        UpdateSporesDisplay();
        UpdateButtons();

        if (selectedItem.itemType == ShopItem.ItemType.Shield && selectedItem.quantity == 1)
            EquipSelectedItem();
    }

    public void EquipSelectedItem()
    {
        if (selectedItem == null || !selectedItem.isPurchased) return;
        if (selectedItem.isConsumable && selectedItem.quantity <= 0) return;

        foreach (var item in shopItems.Where(i => i.itemType == selectedItem.itemType))
            item.isEquipped = false;

        selectedItem.isEquipped = true;
        SaveShopData();

        UpdateButtons();
        CreateShopItems();
        PowerUpManager.Instance?.RefreshPowerUp();
    }

    public void ConsumeShield()
    {
        var shieldItem = GetEquippedItem(ShopItem.ItemType.Shield);
        if (shieldItem?.isConsumable != true || shieldItem.quantity <= 0) return;

        shieldItem.quantity--;

        if (shieldItem.quantity <= 0)
        {
            shieldItem.isEquipped = false;
        }

        SaveShopData();

        if (selectedItem == shieldItem)
        {
            UpdateButtons();
            if (selectedItemQuantity != null)
                selectedItemQuantity.text = $"Quantity: {shieldItem.quantity}";
        }

        PowerUpManager.Instance?.RefreshPowerUp();
    }

    public int GetShieldCount() => GetEquippedItem(ShopItem.ItemType.Shield)?.quantity ?? 0;

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        UpdateSporesDisplay();
    }

    public void CloseShop() => shopPanel.SetActive(false);

    void UpdateSporesDisplay()
    {
        if (sporesCountText != null)
            sporesCountText.text = $"Spores: {GameManager.Instance.spores}";
    }

    void SaveShopData()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            var item = shopItems[i];
            PlayerPrefs.SetInt($"ShopItem_{i}_Purchased", item.isPurchased ? 1 : 0);
            PlayerPrefs.SetInt($"ShopItem_{i}_Equipped", item.isEquipped ? 1 : 0);
            PlayerPrefs.SetInt($"ShopItem_{i}_Quantity", item.quantity);
        }
        PlayerPrefs.Save();
    }

    void LoadShopData()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            var item = shopItems[i];
            item.isPurchased = PlayerPrefs.GetInt($"ShopItem_{i}_Purchased", 0) == 1;
            item.isEquipped = PlayerPrefs.GetInt($"ShopItem_{i}_Equipped", 0) == 1;
            item.quantity = PlayerPrefs.GetInt($"ShopItem_{i}_Quantity", item.isConsumable ? 0 : 1);
        }
    }

    public ShopItem GetEquippedItem(ShopItem.ItemType type) =>
        shopItems.FirstOrDefault(i => i.itemType == type && i.isEquipped && i.isPurchased);

    public void Reinitialize()
    {
        LoadShopData();
        UpdateSporesDisplay();
        shopPanel?.SetActive(false);
    }
}