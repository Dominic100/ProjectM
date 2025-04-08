using Inventory.Model;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Inventory;

[CreateAssetMenu]
public class KeyEquippableItemSO : EquiappableItemSO
{
    [SerializeField] private AudioClip keyUseSFX;
    public override bool PerformAction(GameObject character, List<ItemParameter> itemState = null) {
        KeyHandler handler = character.GetComponent<KeyHandler>();

        if(handler!=null) {
            handler.EquipKey(this);
            return true;
        }

        return false;
    }

    public void UseKey(GameObject character) {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(character.transform.position, 1f);

        foreach(var hitCollider in hitColliders) {
            Door door = hitCollider.GetComponent<Door>();

            if(door!=null && door.isDoorLocked()) {
                door.SetLockState(false);

                if(keyUseSFX!=null) {
                    AudioSource.PlayClipAtPoint(keyUseSFX, character.transform.position);
                }

                // Get the KeyHandler and clear the equipped key after use
                KeyHandler keyHandler = character.GetComponent<KeyHandler>();
                if (keyHandler != null) {
                    keyHandler.UnequipKey();
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
