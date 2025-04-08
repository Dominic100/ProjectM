using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.Model {
    [CreateAssetMenu(fileName = "InventorySO", menuName = "ScriptableObjects/InventorySO")]
    public class InventorySO : ScriptableObject {
        [SerializeField]
        private List<InvItem> inventoryItems;

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, InvItem>> OnInventoryUpdated;

        public void Initialize() {
            inventoryItems = new List<InvItem>();
            for (int i = 0; i < Size; i++) {
                inventoryItems.Add(InvItem.GetEmptyItem());
            }
        }

        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null) {
            if(item.IsStackable==false) {
                for (int i = 0; i < inventoryItems.Count; i++) {
                    while(quantity>0 && IsInventoryFull()==false) {
                        quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                    }
                    InformAboutChange();
                    return quantity;
                }
            }

            quantity = AddStackableItem(item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null) {
            InvItem newItem = new InvItem {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState == null ? item.DefaultParametersList : itemState)
            };

            for (int i=0; i<inventoryItems.Count; i++) {
                if(inventoryItems[i].isEmpty) {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }

            return 0;
        }

        private bool IsInventoryFull() => inventoryItems.Where(item => item.isEmpty).Any() == false;

        private int AddStackableItem(ItemSO item, int quantity) {
            for(int i=0; i<inventoryItems.Count; i++) {
                if (inventoryItems[i].isEmpty) continue;

                if (inventoryItems[i].item.ID == item.ID) {
                    int amountPossibleToTake = inventoryItems[i].item.MaxStackSize - inventoryItems[i].quantity;

                    if(quantity > amountPossibleToTake) {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].item.MaxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }

            while(quantity>0 && IsInventoryFull()==false) {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity);
            }

            return quantity;
        }

        public Dictionary<int, InvItem> GetCurrentInventoryState() {
            Dictionary<int, InvItem> returnValue = new Dictionary<int, InvItem>();
            for (int i = 0; i < inventoryItems.Count; i++) {
                if (inventoryItems[i].isEmpty) continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InvItem GetItemAt(int itemIndex) {
            return inventoryItems[itemIndex];
        }

        public void AddItem(InvItem item) {
            AddItem(item.item, item.quantity);
        }

        public void SwapItems(int itemIndex_1, int itemIndex_2) {
            InvItem item1 =  inventoryItems[itemIndex_1];
            inventoryItems[itemIndex_1] = inventoryItems[itemIndex_2];
            inventoryItems[itemIndex_2] = item1;
            InformAboutChange();
        }

        private void InformAboutChange() {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        public void RemoveItem(int itemIndex, int amount) {
            if(inventoryItems.Count>itemIndex) {
                if (inventoryItems[itemIndex].isEmpty) return;

                int remainder = inventoryItems[itemIndex].quantity - amount;

                if (remainder <= 0) inventoryItems[itemIndex] = InvItem.GetEmptyItem();
                else inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeQuantity(remainder);

                InformAboutChange();
            }
        }

        public bool HasSpace() {
            return inventoryItems.Any(item => item.isEmpty);
        }
    }

    [Serializable]
    public struct InvItem {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;
        public bool isEmpty => item == null;

        public InvItem ChangeQuantity(int newQuantity) {
            return new InvItem {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState)
            };
        }

        public static InvItem GetEmptyItem()
            => new InvItem {
                item = null,
                quantity = 0,
                itemState = new List<ItemParameter>()
            };
    }
}