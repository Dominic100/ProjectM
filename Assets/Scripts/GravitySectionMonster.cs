using System.Linq;
using UnityEngine;

public class GravitySectionMonster : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float ladderCheckRadius = 1f;
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private LayerMask floorLayer;

    [Header("Chase Settings")]
    [SerializeField] private float detectionRadius = 60f;
    [SerializeField] private float minDistanceToPlayer = 0.5f;

    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AudioSource footstepAudio;
    [SerializeField] private Ladder[] ladders;

    [Header("Audio Settings")]
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float maxHearingDistance = 10f;

    private Rigidbody2D rb;
    private bool isOnLadder;
    private Ladder currentLadder;
    //private bool xDir;
    private bool isChasing;
    private bool isGrounded;
    private float ladderTopX=0;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            Debug.LogError("No Rigidbody2D found!");
            return;
        }

        rb.sharedMaterial = new PhysicsMaterial2D {
            friction = 0f,
            bounciness = 0f
        };

        Debug.Log("Initializing monster:");
        Debug.Log($"Initial position: {transform.position}");
        Debug.Log($"Rigidbody2D constraints: {rb.constraints}");
        Debug.Log($"Ground layer mask: {groundLayer.value}");

        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        Physics2D.gravity = new Vector2(9.81f, 0f);

        Debug.Log($"Gravity set to: {Physics2D.gravity}");
    }

    private void Update() {
        if (playerTransform == null) {
            Debug.LogWarning("No player transform assigned!");
            return;
        }

        isGrounded = IsGrounded();
        Debug.Log($"Is Grounded: {isGrounded}");

        CheckPlayerDistance();
        Debug.Log($"Is Chasing: {isChasing}, Distance to player: {Vector2.Distance(transform.position, playerTransform.position)}");

        UpdateFootsteps();

        if (playerTransform != null) {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < 2.5f) {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void FixedUpdate() {
        if (!isChasing || playerTransform == null) {
            Debug.Log("Not chasing or no player - skipping movement");
            return;
        }

        //xDir = (playerTransform.position.x - transform.position.x > 0); 

        if (isOnLadder) {
            Debug.Log("Handling ladder movement");
            HandleLadderMovement();
        }
        else {
            Debug.Log("Handling ground movement");
            HandleGroundMovement();
        }
    }

    private void HandleGroundMovement() {
        if (!isGrounded) {
            Debug.Log("Not grounded - skipping ground movement");
            return;
        }

        Vector2 directionToPlayer = playerTransform.position - transform.position;
        Debug.Log($"Direction to player: {directionToPlayer}");

        // Check if we need to use a ladder
        if (Mathf.Abs(directionToPlayer.x) > 1f) {
            Debug.Log("Looking for ladder - height difference detected");
            Ladder nearestLadder = FindNearestLadder();
            if (nearestLadder != null) {
                Debug.Log($"Found ladder at {nearestLadder.transform.position}");
                MoveTowardsLadder(nearestLadder);
                return;
            }
            else {
                Debug.Log("No suitable ladder found");
            }
        }

        // Move vertically towards player (since gravity is rightward)
        float moveDirection = Mathf.Sign(directionToPlayer.y);
        Vector2 newVelocity = new Vector2(rb.linearVelocity.x, moveSpeed * moveDirection);
        Debug.Log($"Setting velocity: {newVelocity}, Move Direction: {moveDirection}");
        rb.linearVelocity = newVelocity;
    }

    private void HandleLadderMovement() {
        if (currentLadder == null) {
            Debug.Log("No current ladder - exiting ladder movement");
            ExitLadder();
            return;
        }

        Vector2 directionToPlayer = playerTransform.position - transform.position;
        Vector2 movement = Vector2.zero;

        // Get the top position of the ladder (maximum x position in right gravity)
        Debug.Log($"Movement : {Mathf.Sign(playerTransform.position.x - transform.position.x)}");
        if (playerTransform.position.x - transform.position.x > 1f) {
            ladderTopX = currentLadder.transform.position.x + (currentLadder.GetLadderHeight() / 2 - 0.7f);
        }
        else if(playerTransform.position.x - transform.position.x < -1f) {
            ladderTopX = currentLadder.transform.position.x - (currentLadder.GetLadderHeight() / 2 - 0.7f);
        }

        Debug.Log($"Ladder Height: {currentLadder.GetLadderHeight()}, Ladder Top X: {ladderTopX}");
        float distanceToTop = ladderTopX - transform.position.x;

        // First priority: Move to the top of the ladder
        if (Mathf.Abs(distanceToTop) > 1f) {
            Debug.Log($"Moving to ladder top. Distance to top: {distanceToTop}");
            movement.x = Mathf.Sign(distanceToTop);
        }
        // Only allow horizontal movement when at the top
        else {
            Debug.Log("At ladder top, allowing horizontal movement");
            float verticalDistanceToPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);

            if (verticalDistanceToPlayer <= 3f) {
                if (Mathf.Abs(directionToPlayer.x) > 0.1f) {
                    // Close enough horizontally - move towards player
                    movement.y = Mathf.Sign(directionToPlayer.y) * 0.5f;
                    Debug.Log($"Moving towards player. Vertical distance: {verticalDistanceToPlayer}");
                }
            }
            else {
                // Find nearest ladder excluding current one
                Ladder nextLadder = FindNearestLadderExcludingCurrent(currentLadder);
                if (nextLadder != null) {
                    float directionToNextLadderY = nextLadder.transform.position.y - transform.position.y;
                    movement.y = Mathf.Sign(directionToNextLadderY) * 0.5f;
                    Debug.Log($"Moving towards next ladder at {nextLadder.transform.position}");
                }
                else {
                    // If no other ladder found, move towards current ladder's position as fallback
                    float directionToLadderY = currentLadder.transform.position.y - transform.position.y;
                    movement.y = Mathf.Sign(directionToLadderY) * 0.5f;
                    Debug.Log("No other ladder found, moving towards current ladder position");
                }
            }
        }

        movement = movement.normalized;
        Vector2 newVelocity = movement * climbSpeed;
        Debug.Log($"Ladder movement velocity: {newVelocity}, Direction to player: {directionToPlayer}");
        rb.linearVelocity = newVelocity;

        if (!currentLadder.IsWithinLadderBounds(transform.position)) {
            Debug.Log("Outside ladder bounds - exiting ladder");
            ExitLadder();
        }
    }

    private Ladder FindNearestLadderExcludingCurrent(Ladder currentLadder) {
        Ladder nearestLadder = null;
        float nearestHorizontalDistance = float.MaxValue;
        Vector2 directionToPlayer = playerTransform.position - transform.position;
        float maxVerticalDistance = 8f;

        // Filter ladders by vertical distance and usefulness, excluding current ladder
        var validLadders = ladders.Where(ladder =>
        {
            if (ladder == currentLadder) return false; // Exclude current ladder

            float verticalDistance = Mathf.Abs(ladder.transform.position.x - transform.position.x);
            bool isLadderBetween = (ladder.transform.position.x >= Mathf.Min(transform.position.x, playerTransform.position.x) &&
                                   ladder.transform.position.x <= Mathf.Max(transform.position.x, playerTransform.position.x));
            bool isLadderUseful = Mathf.Abs(directionToPlayer.x) > 1f;

            Debug.Log($"Checking ladder at {ladder.transform.position} - Vertical Distance: {verticalDistance}, " +
                      $"Between: {isLadderBetween}, Useful: {isLadderUseful}");

            return verticalDistance < maxVerticalDistance && isLadderBetween && isLadderUseful;
        });

        // Find nearest valid ladder horizontally
        foreach (Ladder ladder in validLadders) {
            float horizontalDistance = Mathf.Abs(ladder.transform.position.y - transform.position.y);

            if (horizontalDistance < detectionRange && horizontalDistance < nearestHorizontalDistance) {
                nearestHorizontalDistance = horizontalDistance;
                nearestLadder = ladder;
                Debug.Log($"New nearest ladder found at y: {ladder.transform.position.y}, " +
                         $"Horizontal distance: {horizontalDistance}");
            }
        }

        return nearestLadder;
    }

    private void MoveTowardsLadder(Ladder ladder) {
        Vector2 directionToLadder = ladder.transform.position - transform.position;
        float moveDirection = Mathf.Sign(directionToLadder.y);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveSpeed * moveDirection);

        // Check if we're aligned horizontally with the ladder
        float horizontalDistance = Mathf.Abs(transform.position.y - ladder.transform.position.y);
        Debug.Log($"Distance to ladder horizontal: {horizontalDistance}, Direction: {moveDirection}");

        if (horizontalDistance < 0.1f)  // Using a smaller threshold for precise alignment
        {
            Debug.Log("Close enough to ladder - entering");
            EnterLadder(ladder);
        }
    }

    private Ladder FindNearestLadder() {
        Ladder nearestLadder = null;
        float nearestHorizontalDistance = float.MaxValue;
        Vector2 directionToPlayer = playerTransform.position - transform.position;
        float maxVerticalDistance = 8f;

        // First, filter ladders by vertical distance and usefulness
        var validLadders = ladders.Where(ladder =>
        {
            float verticalDistance = Mathf.Abs(ladder.transform.position.x - transform.position.x);
            bool isLadderBetween = (ladder.transform.position.x >= Mathf.Min(transform.position.x, playerTransform.position.x) &&
                                   ladder.transform.position.x <= Mathf.Max(transform.position.x, playerTransform.position.x));
            bool isLadderUseful = Mathf.Abs(directionToPlayer.x) > 1f;

            Debug.Log($"Ladder at {ladder.transform.position} - Vertical Distance: {verticalDistance}, " +
                      $"Between: {isLadderBetween}, Useful: {isLadderUseful}");

            return verticalDistance < maxVerticalDistance && isLadderBetween && isLadderUseful;
        });

        // Then, among valid ladders, find the nearest one horizontally
        foreach (Ladder ladder in validLadders) {
            float horizontalDistance = Mathf.Abs(ladder.transform.position.y - transform.position.y);
            Debug.Log($"Checking ladder at y: {ladder.transform.position.y}, " +
                      $"Horizontal distance: {horizontalDistance}, " +
                      $"Ladder detection range: {detectionRange}");

            if (horizontalDistance < detectionRange && horizontalDistance < nearestHorizontalDistance) {
                nearestHorizontalDistance = horizontalDistance;
                nearestLadder = ladder;
                Debug.Log($"New nearest ladder found at y: {ladder.transform.position.y}, " +
                         $"Horizontal distance: {horizontalDistance}");
            }
        }

        if (nearestLadder != null) {
            Debug.Log($"Selected ladder at y: {nearestLadder.transform.position.y}, " +
                      $"Final horizontal distance: {nearestHorizontalDistance}");
        }

        return nearestLadder;
    }

    private void EnterLadder(Ladder ladder) {
        currentLadder = ladder;
        isOnLadder = true;
        rb.gravityScale = 0f;

        // Align with ladder (vertically for right gravity)
        Vector3 position = transform.position;
        position.y = ladder.transform.position.y;
        transform.position = position;

        Debug.Log($"Entering ladder at position: {position}, Ladder position: {ladder.transform.position}");
    }

    private void ExitLadder() {
        isOnLadder = false;
        currentLadder = null;
        rb.gravityScale = 1f;
    }

    private bool IsGrounded() {
        Vector2 rayDirection = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            rayDirection,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(transform.position, Vector2.right * groundCheckDistance, hit.collider != null ? Color.green : Color.red);
        Debug.Log($"Ground check hit: {hit.collider != null}, Layer hit: {(hit.collider != null ? hit.collider.gameObject.layer : -1)}");
        return hit.collider != null;
    }

    private void CheckPlayerDistance() {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool wasChasing = isChasing;
        isChasing = distanceToPlayer <= detectionRadius;

        if (wasChasing != isChasing) {
            Debug.Log($"Chase state changed: {isChasing}, Distance: {distanceToPlayer}, Detection Radius: {detectionRadius}");
        }
    }

    private void UpdateFootsteps() {
        if (footstepAudio == null) return;

        bool isMoving = rb.linearVelocity.magnitude > 0.1f;
        if (isMoving && !footstepAudio.isPlaying) {
            footstepAudio.Play();
        }
        else if (!isMoving && footstepAudio.isPlaying) {
            footstepAudio.Stop();
        }

        if (isMoving) {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float volumeMultiplier = 1f - (distanceToPlayer / maxHearingDistance);
            footstepAudio.volume = footstepVolume * Mathf.Clamp01(volumeMultiplier);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Ladder")) {
            Ladder ladder = other.GetComponent<Ladder>();
            Debug.Log($"Distance: {Vector2.Distance(transform.position, ladder.transform.position)}");
            if (ladder != null && Vector2.Distance(transform.position, ladder.transform.position) < ladderCheckRadius) {
                EnterLadder(ladder);
            }
        }
    }

    private void OnDrawGizmos() {
        // Detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Ground check
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * groundCheckDistance);

        // Ladder check radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, ladderCheckRadius);
    }
}
