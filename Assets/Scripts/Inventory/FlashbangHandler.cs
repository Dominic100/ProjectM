using UnityEngine;

public class FlashbangHandler : MonoBehaviour {
    [SerializeField] private EquipmentStatusDisplay statusDisplay;
    private FlashbangEquippableItemSO equippedFlashbang;
    private bool hasFlashbang = false;
    private FlashEffect flashEffect;

    private void Start() {
        flashEffect = FindFirstObjectByType<FlashEffect>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E) && hasFlashbang) {
            UseFlashbang();
        }
    }

    public void EquipFlashbang(FlashbangEquippableItemSO flashbang) {
        equippedFlashbang = flashbang;
        hasFlashbang = true;
        statusDisplay?.UpdateFlashbangStatus(true);
        Debug.Log("Flashbang equipped");
    }

    public void UnequipFlashbang() {
        equippedFlashbang = null;
        hasFlashbang = false;
        statusDisplay?.UpdateFlashbangStatus(false);
        Debug.Log("Flashbang unequipped");
    }

    private void UseFlashbang() {
        Debug.Log("UseFlashbang called in handler.");
        if (equippedFlashbang != null) {
            if(flashEffect!=null) {
                flashEffect.Flash();
            }

            equippedFlashbang.UseFlashbang(gameObject);
            hasFlashbang = false;
            equippedFlashbang = null;
            Debug.Log("Flashbang used and cleared");
        }
        else {
            Debug.Log("equippedFlashbang is null");
        }
    }

    public bool HasFlashbang() {
        return hasFlashbang;
    }
}
