using UnityEngine;

public class FinalDoor : MonoBehaviour {
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private AudioClip unlockSound;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D doorCollider;
    private bool isLocked = true;
    private bool hasTriggeredWin = false;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        SetLockState(true);
    }

    public void SetLockState(bool locked) {
        isLocked = locked;
        if (locked) {
            if (doorCollider != null) {
                doorCollider.isTrigger = false;
            }

            if (spriteRenderer != null) {
                spriteRenderer.sprite = lockedSprite;
            }
        }
        else {
            if (doorCollider != null) {
                doorCollider.isTrigger = true;
            }

            if (spriteRenderer != null) {
                spriteRenderer.sprite = unlockedSprite;
            }

            // Play unlock sound when the final door is unlocked
            if (unlockSound != null && audioSource != null) {
                audioSource.PlayOneShot(unlockSound);
            }

            if (!isLocked && !hasTriggeredWin) {
                hasTriggeredWin = true;
                GameManager.Instance.WinGame();
            }
        }
    }

    public bool isDoorLocked() {
        return isLocked;
    }
}
