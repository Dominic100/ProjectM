using UnityEngine;

public class FlashlightUpgrade : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            Debug.Log("Collision detected");
            PlayerAim playerAim = collision.GetComponent<PlayerAim>();

            if (playerAim != null && playerAim.GetCurrentUpgradeLevel() < 3) {
                Debug.Log("Flashlight upgrade picked up!");
                playerAim.UpgradeFlashlight();

                if(audioSource!=null) AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);

                Destroy(gameObject);
            }
        }
    }
}
