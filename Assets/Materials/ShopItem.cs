using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public string description;
    public int cost;
    public bool isPurchased;
    public bool isEquipped;
    public Sprite icon;

    public enum ItemType { Shield, SlowTime, DoubleScore, ExtraLife }
    public ItemType itemType;

    public float effectDuration = 7f;
    public float effectValue = 1f;

    public int quantity = 1;
    public bool isConsumable = true;
}