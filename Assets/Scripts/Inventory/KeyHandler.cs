using UnityEngine;

public class KeyHandler : MonoBehaviour
{
    [SerializeField] private EquipmentStatusDisplay statusDisplay;
    private KeyEquippableItemSO equippedKey;
    private FinalKeyEquippableItemSO equippedFinalKey;
    private bool hasKey = false;
    private bool hasFinalKey = false;   

    private void Update() {
        if(Input.GetKeyDown(KeyCode.R) && hasKey) {
            UseKey();
        }
        if(Input.GetKeyDown(KeyCode.R) && hasFinalKey) {
            UseFinalKey();
        }
    }

    public void EquipKey(KeyEquippableItemSO key) {
        equippedKey = key;
        hasKey = true;
        statusDisplay?.UpdateKeyStatus(true);
        Debug.Log("Key Equipped");
    }

    public void UnequipKey() {
        equippedKey = null;
        hasKey = false;
        statusDisplay?.UpdateKeyStatus(false);
        Debug.Log("Key Unequipped");
    }

    private void UseKey() {
        if(equippedKey != null) {
            equippedKey.UseKey(gameObject);
        }
    }

    public void EquipFinalKey(FinalKeyEquippableItemSO finalKey) {
        equippedFinalKey = finalKey;
        hasFinalKey = true;
        statusDisplay?.UpdateFinalKeyStatus(true);
        Debug.Log("Final Key Equipped");
    }

    public void UnequipFinalKey() {
        equippedFinalKey = null;
        hasFinalKey = false;
        statusDisplay?.UpdateFinalKeyStatus(false);
        Debug.Log("Final Key Unequipped");
    }

    private void UseFinalKey() {
        if(equippedFinalKey != null) {
            equippedFinalKey.UseFinalKey(gameObject);
        }
    }
}
