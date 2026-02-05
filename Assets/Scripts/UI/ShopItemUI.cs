using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemCostText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private GameObject purchasedBadge;
    [SerializeField] private GameObject equippedBadge;
    [SerializeField] private Image background;

    [Header("÷вета")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color purchasedColor = Color.green;
    [SerializeField] private Color selectedColor = Color.yellow;

    private ShopItem itemData;
    private ShopManager shopManager;

    public void Initialize(ShopItem item, ShopManager manager)
    {
        itemData = item;
        shopManager = manager;

        if (itemIcon != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
        }

        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }

        if (itemCostText != null)
        {
            if (item.isPurchased && !item.isConsumable)
            {
                itemCostText.text = "PURCHASED";
                itemCostText.color = Color.green;
            }
            else
            {
                itemCostText.text = $"{item.cost}";
                itemCostText.color = Color.white;
            }
        }

        if (itemQuantityText != null)
        {
            if (item.isConsumable && item.isPurchased)
            {
                itemQuantityText.text = $"x{item.quantity}";
                itemQuantityText.gameObject.SetActive(true);
            }
            else
            {
                itemQuantityText.gameObject.SetActive(false);
            }
        }

        if (purchasedBadge != null)
        {
            purchasedBadge.SetActive(item.isPurchased);
        }

        if (equippedBadge != null)
        {
            equippedBadge.SetActive(item.isEquipped);
        }

        if (background != null)
        {
            background.color = item.isPurchased ? purchasedColor : normalColor;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null)
        {
            background.color = isSelected ? selectedColor :
                (itemData.isPurchased ? purchasedColor : normalColor);
        }
    }

    public void UpdateStatus()
    {
        if (purchasedBadge != null)
        {
            purchasedBadge.SetActive(itemData.isPurchased);
        }

        if (equippedBadge != null)
        {
            equippedBadge.SetActive(itemData.isEquipped);
        }

        if (itemCostText != null)
        {
            if (itemData.isPurchased && !itemData.isConsumable)
            {
                itemCostText.text = "PURCHASED";
                itemCostText.color = Color.green;
            }
            else
            {
                itemCostText.text = $"{itemData.cost}";
                itemCostText.color = Color.white;
            }
        }

        if (itemQuantityText != null)
        {
            if (itemData.isConsumable && itemData.isPurchased)
            {
                itemQuantityText.text = $"x{itemData.quantity}";
                itemQuantityText.gameObject.SetActive(true);
            }
            else
            {
                itemQuantityText.gameObject.SetActive(false);
            }
        }

        if (background != null)
        {
            background.color = itemData.isPurchased ? purchasedColor : normalColor;
        }
    }
}