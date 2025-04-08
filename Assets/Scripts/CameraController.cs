using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -15);

    // LateUpdate() and Update() have similar performance overhead. LateUpdate works after Update function has been executed
    private void LateUpdate() { 
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset;

        // Lerp function travels from current position to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime); 

        transform.position = smoothedPosition;
    }
}
