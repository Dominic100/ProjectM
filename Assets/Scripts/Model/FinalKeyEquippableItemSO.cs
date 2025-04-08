using Inventory.Model;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Inventory;

[CreateAssetMenu]
public class FinalKeyEquippableItemSO : EquiappableItemSO {
    [SerializeField] private AudioClip finalKeyUseSFX;

    public override bool PerformAction(GameObject character, List<ItemParameter> itemState = null) {
        KeyHandler handler = character.GetComponent<KeyHandler>();

        if (handler != null) {
            handler.EquipFinalKey(this);
            return true;
        }

        return false;
    }

    public void UseFinalKey(GameObject character) {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(character.transform.position, 1f);

        foreach (var hitCollider in hitColliders) {
            FinalDoor finalDoor = hitCollider.GetComponent<FinalDoor>();

            if (finalDoor != null && finalDoor.isDoorLocked()) {
                finalDoor.SetLockState(false);

                if (finalKeyUseSFX != null) {
                    AudioSource.PlayClipAtPoint(finalKeyUseSFX, character.transform.position);
                }

                // Get the KeyHandler and clear the equipped final key after use
                KeyHandler keyHandler = character.GetComponent<KeyHandler>();
                if (keyHandler != null) {
                    keyHandler.UnequipFinalKey();
                }

                // Clear the reference in InventoryController
                InventoryController inventoryController = character.GetComponent<InventoryController>();
                if (inventoryController != null) {
                    inventoryController.ClearEquippedItem();
                }

                return;
            }
        }
    }
}
