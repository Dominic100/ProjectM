using Inventory.UI;
using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.Text;
using UnityEngine.UIElements;

namespace Inventory {
    public class InventoryController : MonoBehaviour {
        [SerializeField] private InventoryPage inventoryUI;
        [SerializeField] private InventorySO inventoryData;
        private EquiappableItemSO currentlyEquippedItem;

        public List<InvItem> initialItems = new List<InvItem>();

        [SerializeField] private AudioClip dropClip;
        [SerializeField] private AudioSource audioSource;

        private void Start() {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData() {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InvItem item in initialItems) {
                if(item.isEmpty) {
                    continue;
                }
                inventoryData.AddItem(item);
            }
        }

        public void UnequipCurrentItem() {
            if (currentlyEquippedItem != null) {
                // Create a new InvItem for the unequipped item
                InvItem unequippedItem = new InvItem {
                    item = currentlyEquippedItem,
                    quantity = 1
                    // Set other properties as needed
                };

                // Check if inventory has space before unequipping
                if (inventoryData.HasSpace()) {
                    // Add the item back to inventory
                    inventoryData.AddItem(unequippedItem);

                    // Notify handlers about unequip
                    if (currentlyEquippedItem is KeyEquippableItemSO) {
                        KeyHandler keyHandler = gameObject.GetComponent<KeyHandler>();
                        if (keyHandler != null) keyHandler.UnequipKey();
                    }
                    else if (currentlyEquippedItem is FinalKeyEquippableItemSO) {
                        KeyHandler keyHandler = gameObject.GetComponent<KeyHandler>();
                        if (keyHandler != null) keyHandler.UnequipFinalKey();
                    }
                    else if (currentlyEquippedItem is FlashbangEquippableItemSO) {
                        FlashbangHandler flashbangHandler = gameObject.GetComponent<FlashbangHandler>();
                        if (flashbangHandler != null) flashbangHandler.UnequipFlashbang();
                    }

                    currentlyEquippedItem = null;
                    audioSource.PlayOneShot(dropClip);
                }
                else {
                    Debug.Log("Cannot unequip - inventory is full!");
                    // Optionally: Play a different sound or show a message to the player
                    // You might want to add a UI message system to show this to the player
                    // For example:
                    // messageUI.ShowMessage("Inventory is full! Cannot unequip item.");
                }
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InvItem> inventoryState) {
            inventoryUI.ResetAllItems();

            foreach (var item in inventoryState) {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void PrepareUI() {
            inventoryUI.InitializeInventory(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleItemActionRequest(int itemIndex) {
            InvItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if(inventoryItem.isEmpty) {
                return;
            }    

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if(itemAction!=null) {           
                inventoryUI.ShowItemAction(itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
            }

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null) {
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
            }
        }

        private void DropItem(int itemIndex, int quantity) {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropClip);
        }

        public void PerformAction(int itemIndex) {
            InvItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty) {
                return;
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null) {
                // Track equipped item if it's equippable
                if (inventoryItem.item is EquiappableItemSO equippableItem) {
                    UnequipCurrentItem(); // Unequip current item first
                    currentlyEquippedItem = equippableItem;

                    // Remove item from inventory immediately when equipped
                    inventoryData.RemoveItem(itemIndex, 1);

                    // Perform the equip action
                    itemAction.PerformAction(gameObject, inventoryItem.itemState);
                    audioSource.PlayOneShot(itemAction.actionSFX);
                }
                else {
                    // For non-equippable items, keep the original behavior
                    IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
                    if (destroyableItem != null) {
                        inventoryData.RemoveItem(itemIndex, 1);
                    }

                    itemAction.PerformAction(gameObject, inventoryItem.itemState);
                    audioSource.PlayOneShot(itemAction.actionSFX);
                }

                if (inventoryData.GetItemAt(itemIndex).isEmpty)
                    inventoryUI.ResetSelection();
            }
        }

        public void ClearEquippedItem() {
            currentlyEquippedItem = null;
        }

        private void HandleDragging(int itemIndex) {
            InvItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty) {
                return;
            }
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2) {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex) {
            InvItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty) {
                inventoryUI.ResetSelection();
                return;
            }

            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage, item.Name, description);
        }

        private string PrepareDescription(InvItem inventoryItem) {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();

            for(int i=0; i<inventoryItem.itemState.Count; i++) {
                sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName} " + $": {inventoryItem.itemState[i].value} / {inventoryItem.item.DefaultParametersList[i].value}");
            }

            return sb.ToString();
        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.I)) {
                if (inventoryUI.isActiveAndEnabled == false) {
                    inventoryUI.Show();
                    foreach (var item in inventoryData.GetCurrentInventoryState()) {
                        inventoryUI.UpdateData(item.Key,
                            item.Value.item.ItemImage,
                            item.Value.quantity
                            );
                    }
                }
                else {
                    inventoryUI.Hide();
                }
            }

            if(Input.GetKeyDown(KeyCode.U)) {
                UnequipCurrentItem();
            }
        }
    }
}