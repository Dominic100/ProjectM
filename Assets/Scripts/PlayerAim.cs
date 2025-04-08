using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerAim : MonoBehaviour {
    [SerializeField] private Light2D flashlight;

    [Header("Rotation Settings")]
    [SerializeField] private float keyRotationSpeed = 300f; // Degrees per second for key rotation

    [Header("Flashlight Base Stats")]
    [SerializeField] private float baseIntensity = 2f;
    [SerializeField] private float baseInnerRadius = 3f;
    [SerializeField] private float baseOuterRadius = 8f;
    [SerializeField] private float baseInnerAngle = 55f;
    [SerializeField] private float baseOuterAngle = 105f;

    [Header("Upgrade Settings")]
    [SerializeField] private float angleUpgradeMultiplier = 1.5f;
    [SerializeField] private float radiusUpgradeMultiplier = 1.5f;
    [SerializeField] private UpgradeStatusDisplay upgradeUI;

    private float currentAngle;
    private int currentUpgradeLevel = 0;
    private bool isInGravitySection = false;

    private void Start() {
        if (flashlight == null) flashlight = GetComponentInChildren<Light2D>();
        currentAngle = transform.eulerAngles.z;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SetBaseStats();
    }

    private void Update() {
        if (!isInGravitySection) {
            HandleKeyAiming();
            HandleFlashlight();
        }        
    }

    public void EnterGravitySection() {
        isInGravitySection = true;

        if (flashlight != null) {
            flashlight.enabled = false;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
    }

    public void ExitGravitySection() {
        isInGravitySection = false;

        if (flashlight != null) {
            flashlight.enabled = true;
        }
    }

    private void HandleKeyAiming() {
        float rotationInput = 0f;

        // Process rotation keys independently of other inputs
        if (Input.GetKey(KeyCode.N)) // < key
        {
            rotationInput += 1f; // Counterclockwise
        }
        if (Input.GetKey(KeyCode.M)) // > key
        {
            rotationInput -= 1f; // Clockwise
        }

        // Apply rotation if there's input
        if (rotationInput != 0) {
            currentAngle += rotationInput * keyRotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }

    private void HandleFlashlight() {
        if (Input.GetKeyDown(KeyCode.F)) {
            ToggleFlashlight();
        }
    }

    public void ToggleFlashlight() {
        if (flashlight != null) {
            flashlight.enabled = !flashlight.enabled;
        }
    }

    private void SetBaseStats() {
        flashlight.intensity = baseIntensity;
        flashlight.pointLightInnerRadius = baseInnerRadius;
        flashlight.pointLightOuterRadius = baseOuterRadius;
        flashlight.pointLightInnerAngle = baseInnerAngle;
        flashlight.pointLightOuterAngle = baseOuterAngle;
    }

    public void UpgradeFlashlight() {
        if (currentUpgradeLevel >= 3) return; // Max level reached

        currentUpgradeLevel++;

        switch (currentUpgradeLevel) {
            case 1: // Angle upgrade
                flashlight.pointLightInnerAngle = baseInnerAngle * angleUpgradeMultiplier;
                flashlight.pointLightOuterAngle = baseOuterAngle * angleUpgradeMultiplier;
                Debug.Log("Flashlight Angle Upgraded");
                break;

            case 2: // Range upgrade
                flashlight.pointLightInnerRadius = baseInnerRadius * radiusUpgradeMultiplier;
                flashlight.pointLightOuterRadius = baseOuterRadius * radiusUpgradeMultiplier;
                Debug.Log("Flashlight Range Upgraded");
                break;

            case 3: // 360-degree vision
                flashlight.pointLightInnerAngle = 360f;
                flashlight.pointLightOuterAngle = 360f;
                // Reset rotation to prevent issues with 360-degree light
                transform.rotation = Quaternion.identity;
                Debug.Log("Flashlight 360 Upgrade");
                break;
        }

        // Update UI if available
        if (upgradeUI != null) {
            upgradeUI.UpdateUpgradeStatus(currentUpgradeLevel);
        }
    }

    public int GetCurrentUpgradeLevel() {
        return currentUpgradeLevel;
    }
}
