using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

namespace Inventory.UI {
    public class InventoryPage : MonoBehaviour {
        [SerializeField] private InventoryItem itemPrefab;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private InventoryDescription itemDescription;
        [SerializeField] private MouseFollower mouseFollower;

        List<InventoryItem> listOfItems = new List<InventoryItem>();

        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
        public event Action<int, int> OnSwapItems;

        [SerializeField] private ItemActionPanel actionPanel;

        private void Awake() {
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }

        public void InitializeInventory(int inventorySize) {
            for (int i = 0; i < inventorySize; i++) {
                InventoryItem inventoryItemUI = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                inventoryItemUI.transform.SetParent(contentPanel);
                listOfItems.Add(inventoryItemUI);

                inventoryItemUI.OnItemClicked += HandleItemSelection;
                inventoryItemUI.OnItemBeginDrag += HandleBeginDrag;
                inventoryItemUI.OnItemDroppedOn += HandleSwap;
                inventoryItemUI.OnItemEndDrag += HandleEndDrag;
                inventoryItemUI.OnRightMouseBtnClick += HandleShowItemActions;

            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity) {
            if (listOfItems.Count > itemIndex) {
                listOfItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }

        private void HandleShowItemActions(InventoryItem inventoryItemUI) {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1) {
                return;
            }

            OnItemActionRequested?.Invoke(index);
        }

        private void HandleEndDrag(InventoryItem inventoryItemUI) {
            ResetDraggedItem();
        }

        private void HandleSwap(InventoryItem inventoryItemUI) {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1) {
                return;
            }

            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void ResetDraggedItem() {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDrag(InventoryItem inventoryItemUI) {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1) {
                return;
            }
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }

        public void CreateDraggedItem(Sprite sprite, int quantity) {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(InventoryItem inventoryItemUI) {
            int index = listOfItems.IndexOf(inventoryItemUI);

            if (index == -1) return;

            OnDescriptionRequested?.Invoke(index);
        }

        public void Show() {
            gameObject.SetActive(true);
            ResetSelection();

            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

        public void ResetSelection() {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }

        public void AddAction(string actionName, Action performAction) {
            actionPanel.AddButton(actionName, performAction);
        }

        public void ShowItemAction(int itemIndex) {
            actionPanel.Toggle(true);
            actionPanel.transform.position = listOfItems[itemIndex].transform.position;
        }

        private void DeselectAllItems() {
            foreach (InventoryItem item in listOfItems) {
                item.Deselect();
            }
            actionPanel.Toggle(false);
        }

        public void Hide() {
            actionPanel.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();

            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }

        public void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description) {
            itemDescription.SetDescription(itemImage, name, description);
            DeselectAllItems();
            listOfItems[itemIndex].Select();
        }

        public void ResetAllItems() {
            foreach (var item in listOfItems) {
                item.ResetData();
                item.Deselect();
            }
        }
    }
}