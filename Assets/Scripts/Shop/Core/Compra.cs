using UnityEngine;

public class InventoryPurchaseDebugger : MonoBehaviour
{
    private void OnEnable()
    {
        LootBoxEvents.OnLootBoxOpened += OnLootboxReward;
    }

    private void OnDisable()
    {
        LootBoxEvents.OnLootBoxOpened -= OnLootboxReward;
    }

    private void Update()
    {
        // Detect shop purchase state
        if (ShopPedestalRandomizer.currentPedestal != null &&
            ShopPedestalRandomizer.currentPedestal.isAwaitingDecision)
        {
            var ped = ShopPedestalRandomizer.currentPedestal;
            var item = ped.GetChosenItem();

            if (item != null)
            {
                Debug.Log("[DEBUG] Pedestal waiting for decision. Item: " + item.itemID);
            }
        }
    }

    private void OnLootboxReward(LootBoxSO box, BaseItemSO reward)
    {
        Debug.Log("[DEBUG] Lootbox reward: " + (reward != null ? reward.itemID : "NULL"));
    }

    public static void DebugPurchase(BaseItemSO item)
    {
        if (item == null)
        {
            Debug.LogError("[PURCHASE DEBUG] ERROR: chosenItem is NULL.");
            return;
        }

        Debug.Log("[PURCHASE DEBUG] Trying to add: " + item.itemID);

        var inv = InventoryManager.Instance;

        bool existsInCatalog = inv.GetItemSO(item.itemID) != null;
        Debug.Log("[PURCHASE DEBUG] Exists in catalog: " + existsInCatalog);

        if (!existsInCatalog)
        {
            Debug.LogError("[PURCHASE DEBUG] ERROR: Item not found in InventoryManager catalog. Check Resources/Items folders.");
            return;
        }

        // Check slot compatibility manually
        bool slotFound = false;

        foreach (var slot in inv.AllSlots)
        {
            if (IsSlotCompatible(slot, item))
            {
                slotFound = true;
                break;
            }
        }

        Debug.Log("[PURCHASE DEBUG] Slot compatible: " + slotFound);

        if (!slotFound)
        {
            Debug.LogError("[PURCHASE DEBUG] ERROR: No compatible slot found for item type.");
        }
    }

    private static bool IsSlotCompatible(ItemSlot slot, BaseItemSO item)
    {
        switch (slot.SlotType)
        {
            case SlotType.Dice:
            case SlotType.ActiveDice:
                return item is DiceSO;

            case SlotType.Consumable:
                return item is ConsumableSO;

            case SlotType.Permanent:
                return item is PermanentSO;

            default:
                return false;
        }
    }
}
