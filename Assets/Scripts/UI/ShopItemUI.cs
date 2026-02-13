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

    public void Initialize(ShopItem item)
    {
        itemData = item;

        if (itemIcon != null && item.icon != null)
            itemIcon.sprite = item.icon;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        UpdateStatus();
    }

    public void SetSelected(bool isSelected)
    {
        if (background == null) return;

        background.color = isSelected ? selectedColor :
            (itemData.isPurchased ? purchasedColor : normalColor);
    }

    public void UpdateStatus()
    {
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
            bool showQuantity = itemData.isConsumable && itemData.isPurchased;
            itemQuantityText.gameObject.SetActive(showQuantity);
            if (showQuantity)
                itemQuantityText.text = $"x{itemData.quantity}";
        }

        if (purchasedBadge != null)
            purchasedBadge.SetActive(itemData.isPurchased);

        if (equippedBadge != null)
            equippedBadge.SetActive(itemData.isEquipped);

        if (background != null)
            background.color = itemData.isPurchased ? purchasedColor : normalColor;
    }
}