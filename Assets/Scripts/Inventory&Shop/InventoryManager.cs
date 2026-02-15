using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] itemSlots;
    public UseItem useItem;
    public int gold;
    public TMP_Text goldText;
    public GameObject lootPrefab;
    public Transform player;

    private void Start()
    {
        foreach(var slot in itemSlots)
        {
            slot.UpdateUI();
        }
    }

    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }
    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }

    public void AddItem(ItemSO itemSO, int quantity)
    {
        if(itemSO.isGold)
        {
            gold += quantity;
            goldText.text = gold.ToString();
            return;
        }

        // 先尝试物品堆叠
        foreach(var slot in itemSlots)
        {
            if(slot.itemSO == itemSO && slot.quantity <= itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;

                slot.UpdateUI();

                if(quantity <= 0)
                {
                    return;
                }

            }
        }

        // 无法堆叠的, 分配一个新的槽位
        foreach(var slot in itemSlots)
        {
            if(slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);
                slot.itemSO = itemSO;
                slot.quantity = quantity;
                slot.UpdateUI();
                return;
            }

        }

        // 若所有槽位均无法加入物品，则丢弃
        if (quantity > 0)
        {
            DropLoot(itemSO, quantity);
        }
        
    }

    public void DropItem(InventorySlot slot)
    {
        // 点击一次丢弃一个物品
        DropLoot(slot.itemSO, 1);
        slot.quantity--;
        if(slot.quantity <= 0)
        {
            slot.itemSO = null;
        }
        slot.UpdateUI();
    }


    private void DropLoot(ItemSO itemSO, int quantity)
    {
        Loot loot = Instantiate(lootPrefab, player.position, Quaternion.identity).GetComponent<Loot>();
        loot.Initialize(itemSO, quantity);
    }

    public void UseItem(InventorySlot slot)
    {
        if(slot.itemSO != null && slot.quantity >= 0)
        {
            useItem.ApplyItemEffects(slot.itemSO);
            slot.quantity--;
            if(slot.quantity <= 0)
            {
                slot.itemSO = null;
            }
            slot.UpdateUI();
        }
    } 
}
