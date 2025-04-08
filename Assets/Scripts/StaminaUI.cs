using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour {
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Slider staminaSlider;

    private void Update() {
        staminaSlider.value = playerMovement.GetStaminaPercentage();
    }
}
