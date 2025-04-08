using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private bool isAutoLockDoor = false;
    [SerializeField] private float autoLockDelay = 0.5f;
    [SerializeField] private AudioClip autoLockSound;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D doorCollider;
    private bool isLocked = true;
    private bool hasPlayerPassed = false;

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
            if(doorCollider!=null) {
                doorCollider.isTrigger = true;
            }

            if(spriteRenderer!=null) {
                spriteRenderer.sprite = unlockedSprite;
            }

            if(isAutoLockDoor) {
                hasPlayerPassed = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (isAutoLockDoor && other.CompareTag("Player") && !hasPlayerPassed && !isLocked) {
            hasPlayerPassed = true;
            Invoke("AutoLockDoor", autoLockDelay);
        }
    }

    private void AutoLockDoor() {
        if (!isAutoLockDoor) return;

        SetLockState(true);

        if(autoLockSound!=null && audioSource!=null) {
            audioSource.PlayOneShot(autoLockSound);
        }
    }

    public bool isDoorLocked() {
        return isLocked;
    }
}
