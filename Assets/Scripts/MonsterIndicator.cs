using UnityEngine;
using UnityEngine.UI;

public class MonsterIndicator : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform monsterTransform;
    [SerializeField] private Image arrowImage;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRectTransform;

    [Header("Visual Settings")]
    [SerializeField] private float indicatorRadius = 100f; // Now in UI units (pixels)
    [SerializeField] private float detectionRadius = 20f;  // World units
    [SerializeField] private float minOpacity = 0f;
    [SerializeField] private float maxOpacity = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private float minVolume = 0f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private bool useLogarithmicFalloff = true;

    private Color arrowColor;
    private bool isMonsterInRange;
    private RectTransform arrowRectTransform;

    private void Start() {
        // Get reference to arrow's RectTransform
        arrowRectTransform = arrowImage.rectTransform;

        // Setup audio source if not assigned
        if (audioSource != null) {
            audioSource.clip = warningSound;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        // Store initial color
        arrowColor = arrowImage.color;

        // Initially hide arrow
        SetArrowOpacity(0f);
    }

    private void Update() {
        if (monsterTransform == null) return;

        float distanceToMonster = Vector2.Distance(transform.position, monsterTransform.position);

        // Check if monster is in range
        if (distanceToMonster <= detectionRadius) {
            if (!isMonsterInRange) {
                isMonsterInRange = true;
                audioSource.Play();
            }

            UpdateIndicator(distanceToMonster);
            UpdateWarningSound(distanceToMonster);
        }
        else if (isMonsterInRange) {
            isMonsterInRange = false;
            SetArrowOpacity(0f);
            audioSource.Stop();
        }
    }

    private void UpdateIndicator(float distanceToMonster) {
        // Calculate direction to monster
        Vector2 directionToMonster = (monsterTransform.position - transform.position).normalized;

        // Calculate position on circle
        Vector2 indicatorPosition = directionToMonster * indicatorRadius;
        arrowRectTransform.anchoredPosition = indicatorPosition;

        // Rotate arrow to point at monster
        float angle = Mathf.Atan2(directionToMonster.y, directionToMonster.x) * Mathf.Rad2Deg - 90f;
        arrowRectTransform.rotation = Quaternion.Euler(0, 0, angle);

        // Calculate opacity based on distance
        float normalizedDistance = 1f - Mathf.Clamp01((distanceToMonster) / detectionRadius);
        float opacity = Mathf.Lerp(minOpacity, maxOpacity, normalizedDistance);

        SetArrowOpacity(opacity);
    }

    private void UpdateWarningSound(float distanceToMonster) {
        float normalizedDistance = 1f - Mathf.Clamp01(distanceToMonster / detectionRadius);
        float targetVolume;

        if (useLogarithmicFalloff) {
            targetVolume = Mathf.Lerp(minVolume, maxVolume,
                Mathf.Log10(normalizedDistance * 9f + 1f));
        }
        else {
            targetVolume = Mathf.Lerp(minVolume, maxVolume, normalizedDistance);
        }

        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 10f);
    }

    private void SetArrowOpacity(float opacity) {
        arrowColor.a = opacity;
        arrowImage.color = arrowColor;
    }

    public void SetDetectionRadius(float newRadius) {
        detectionRadius = newRadius;
    }
}
