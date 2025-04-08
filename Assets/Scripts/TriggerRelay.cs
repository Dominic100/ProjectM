using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    private HidingSpot parentSpot;

    private void Start() {
        parentSpot = GetComponentInParent<HidingSpot>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (parentSpot != null) parentSpot.HandleTriggerEnter(collision);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (parentSpot != null) parentSpot.HandleTriggerExit(collision);
    }
}
