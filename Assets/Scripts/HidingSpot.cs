using UnityEngine;

public class HidingSpot : MonoBehaviour {
    private MonsterAI monster;
    private bool playerIsHiding = false;

    [Header("Collider References")]
    [SerializeField] private CircleCollider2D triggerCollider;    // For player detection
    [SerializeField] private CircleCollider2D physicsCollider;    // For monster collision

    private void Start() {
        monster = FindFirstObjectByType<MonsterAI>();
        if (monster == null) {
            Debug.LogError("No MonsterAI found in scene!");
        }

        // Validate and setup trigger collider
        if (triggerCollider != null) {
            triggerCollider.isTrigger = true;
        }
        else {
            Debug.LogError("Trigger collider not assigned!");
        }

        // Validate and setup physics collider
        if (physicsCollider != null) {
            physicsCollider.isTrigger = false;
        }
        else {
            Debug.LogError("Physics collider not assigned!");
        }

        // Set up player collision ignore
        Collider2D playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
        if (playerCollider != null) {
            Physics2D.IgnoreCollision(playerCollider, physicsCollider);
        }
    }

    public void HandleTriggerEnter(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Debug.Log("Player entered hiding spot");
            playerIsHiding = true;
            if (monster != null) {
                monster.OnPlayerHide(transform.position);
            }
        }
    }

    public void HandleTriggerExit(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Debug.Log("Player exited hiding spot");
            playerIsHiding = false;
            if (monster != null) {
                monster.OnPlayerUnhide();
            }
        }
    }

    public bool IsPlayerHiding() {
        return playerIsHiding;
    }
}
