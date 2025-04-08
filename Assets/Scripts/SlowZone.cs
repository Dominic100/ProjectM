using UnityEngine;

public class SlowZone : MonoBehaviour {
    [SerializeField] private float speedMultiplier = 0.8f;
    [SerializeField] private AudioSource zoneAudioSource;
    [SerializeField] private AudioClip zoneAmbience;

    private void Start() {
        // Setup audio source if not assigned
        if (zoneAudioSource == null) {
            zoneAudioSource = gameObject.AddComponent<AudioSource>();
        }
        zoneAudioSource.clip = zoneAmbience;
        zoneAudioSource.loop = true;
        zoneAudioSource.playOnAwake = false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null) {
                playerMovement.ApplySpeedModifier(speedMultiplier);
                Debug.Log("Player speed decreased");
            }

            // Start playing the zone audio
            if (zoneAudioSource != null && zoneAmbience != null) {
                zoneAudioSource.Play();
            }
        }
        else if (other.CompareTag("Monster")) // Assuming your monster has this tag
        {
            // Assuming your monster has a similar movement script
            MonsterAI monsterMovement = other.GetComponent<MonsterAI>();
            if (monsterMovement != null) {
                monsterMovement.ApplySpeedModifier(speedMultiplier);
                Debug.Log("Monster speed decreased");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null) {
                playerMovement.RemoveSpeedModifier();
                Debug.Log("Player speed back to normal");
            }

            // Stop playing the zone audio
            if (zoneAudioSource != null) {
                zoneAudioSource.Stop();
            }
        }
        else if (other.CompareTag("Monster")) {
            MonsterAI monsterMovement = other.GetComponent<MonsterAI>();
            if (monsterMovement != null) {
                monsterMovement.RemoveSpeedModifier();
                Debug.Log("Monster speed back to normal");
            }
        }
    }
}
