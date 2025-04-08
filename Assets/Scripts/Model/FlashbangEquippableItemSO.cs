using Inventory;
using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FlashbangEquippableItemSO : EquiappableItemSO {
    [SerializeField] private float stunRadius = 5f;
    [SerializeField] private CharacterStatMovementModifierSO movementModifier;
    [SerializeField] private float stunDuration = 5f;
    [SerializeField] private AudioClip flashbangSFX;

    public override bool PerformAction(GameObject character, List<ItemParameter> itemState = null) {
        // Try to get the flashbang handler
        FlashbangHandler handler = character.GetComponent<FlashbangHandler>();

        Debug.Log("hello");

        if (handler != null) {
            handler.EquipFlashbang(this);
            Debug.Log("Hello Again");
            return true;
        }

        return false;
    }

    public void UseFlashbang(GameObject character) {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(character.transform.position, stunRadius);

        Debug.Log($"Found {hitColliders.Length} colliders in radius {stunRadius}");

        foreach (var hitCollider in hitColliders) {
            Debug.Log($"Checking collider on object: {hitCollider.gameObject.name}");

            // Check if it's a monster
            MonsterAI monster = hitCollider.GetComponent<MonsterAI>();
            Debug.Log($"MonsterAI component is {(monster == null ? "null" : "not null")}");

            if (monster != null) {
                Debug.Log("Found monster, applying stun effect");
                movementModifier.AffectCharacter(hitCollider.gameObject, stunDuration);
            }
        }

        if (flashbangSFX != null) {
            AudioSource.PlayClipAtPoint(flashbangSFX, character.transform.position);
        }

        // Visual debug of the circle in Scene view (2D)
        Debug.DrawLine(character.transform.position, character.transform.position + Vector3.right * stunRadius, Color.red, 2f);
        Debug.DrawLine(character.transform.position, character.transform.position + Vector3.left * stunRadius, Color.red, 2f);
        Debug.DrawLine(character.transform.position, character.transform.position + Vector3.up * stunRadius, Color.red, 2f);
        Debug.DrawLine(character.transform.position, character.transform.position + Vector3.down * stunRadius, Color.red, 2f);

        // Clear the equipped flashbang after use
        FlashbangHandler flashbangHandler = character.GetComponent<FlashbangHandler>();
        if (flashbangHandler != null) {
            flashbangHandler.UnequipFlashbang();
        }

        // Clear the reference in InventoryController
        InventoryController inventoryController = character.GetComponent<InventoryController>();
        if (inventoryController != null) {
            inventoryController.ClearEquippedItem();
        }
    }
}
