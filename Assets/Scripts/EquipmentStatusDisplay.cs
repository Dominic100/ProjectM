using UnityEngine;
using UnityEngine.UI;

public class EquipmentStatusDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image flashbangIcon;
    [SerializeField] private Image keyIcon;
    [SerializeField] private Image finalKeyIcon;

    public void UpdateFlashbangStatus(bool hasFlashbang)
    {
        if (flashbangIcon != null) {
            flashbangIcon.gameObject.SetActive(hasFlashbang);
        }  
    }

    public void UpdateKeyStatus(bool hasKey)
    {
        if (keyIcon != null) {
            keyIcon.gameObject.SetActive(hasKey);
        }
    }

    public void UpdateFinalKeyStatus(bool hasFinalKey)
    {
        if (finalKeyIcon != null) {
            finalKeyIcon.gameObject.SetActive(hasFinalKey);
        }
    }
}
