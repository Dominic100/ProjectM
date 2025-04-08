using UnityEngine;

public class Ladder : MonoBehaviour {
    [Header("Ladder Settings")]
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] public bool allowHorizontalMovement = true;

    private BoxCollider2D ladderCollider;

    // Properties to get ladder dimensions from transform
    private float LadderHeight => transform.localScale.x;
    private float LadderWidth => transform.localScale.y;

    private void Start() {
        ladderCollider = GetComponent<BoxCollider2D>();

        if (gameObject.tag != "Ladder") {
            gameObject.tag = "Ladder";
        }
    }

    public bool IsWithinLadderBounds(Vector2 position) {
        if (ladderCollider != null) {
            return ladderCollider.bounds.Contains(position);
        }

        // Fallback if collider is not available (adjusted for right gravity)
        Vector2 ladderPos = transform.position;
        return position.x >= ladderPos.x - LadderHeight / 2 &&
               position.x <= ladderPos.x + LadderHeight / 2 &&
               position.y >= ladderPos.y - LadderWidth / 2 &&
               position.y <= ladderPos.y + LadderWidth / 2;
    }

    public float GetClimbSpeed() {
        return climbSpeed;
    }

    public Vector2 GetRightPosition() {
        return new Vector2(transform.position.x + LadderHeight / 2, transform.position.y);
    }

    public Vector2 GetLeftPosition() {
        return new Vector2(transform.position.x - LadderHeight / 2, transform.position.y);
    }

    public float GetLadderHeight() {
        return LadderHeight;
    }

    public float GetLadderWidth() {
        return LadderWidth;
    }

    private void OnDrawGizmos() {
        // Draw ladder bounds in editor for debugging
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(LadderHeight, LadderWidth, 0.1f);
        Gizmos.DrawWireCube(center, size);

        // Draw right and left points
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetRightPosition(), 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetLeftPosition(), 0.1f);
    }

    public bool IsAtRightOfLadder(Vector2 position, float threshold = 0.1f) {
        return Vector2.Distance(position, GetRightPosition()) < threshold;
    }

    public bool IsAtLeftOfLadder(Vector2 position, float threshold = 0.1f) {
        return Vector2.Distance(position, GetLeftPosition()) < threshold;
    }

    public bool IsNearLadder(Vector2 position, float proximityThreshold = 1f) {
        return Vector2.Distance(position, transform.position) < proximityThreshold;
    }
}
