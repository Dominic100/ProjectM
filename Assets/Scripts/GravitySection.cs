using System.Collections;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

public class GravitySection : MonoBehaviour
{
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private Transform monsterSpawnPoint;
    [SerializeField] private float monsterEnableDelay = 10f;
    [SerializeField] private float rotationDuration = 1.5f;
    [SerializeField] private float sectionLightIntensity = 1f;
    [SerializeField] private GravitySectionMonster sectionMonster; // Changed from MonsterAI

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gravitySectionMusic;
    [SerializeField] private float musicVolume = 1f;

    private Camera mainCamera;
    private Light2D sectionLight;

    private void Start() {
        //NavMeshObstacle obstacle = gameObject.GetComponent<NavMeshObstacle>();
        //obstacle.carving = true;
        //obstacle.size = GetComponent<Collider2D>().bounds.size;

        mainCamera = Camera.main;

        sectionLight = gameObject.GetComponent<Light2D>();
        sectionLight.lightType = Light2D.LightType.Freeform;
        sectionLight.intensity = sectionLightIntensity;
        sectionLight.enabled = true;

        if (monsterSpawnPoint == null) {
            Debug.LogError("Monster spawn point not assigned!");
        }

        if (sectionMonster == null) {
            Debug.LogError("Section Monster not assigned!");
        }
        else {
            sectionMonster.gameObject.SetActive(false); // Ensure monster starts disabled
        }

        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (gravitySectionMusic != null) {
            audioSource.clip = gravitySectionMusic;
            audioSource.loop = true; // Enable looping
            audioSource.volume = musicVolume;
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            PlayerAim playerAim = other.GetComponent<PlayerAim>();

            if (audioSource != null && gravitySectionMusic != null) {
                audioSource.Play();
            }

            if (playerRb!=null) {
                playerRb.gravityScale = gravityScale;
                Physics2D.gravity = Vector2.right * Physics2D.gravity.magnitude;
            }

            if (playerMovement != null) {
                playerMovement.EnterGravitySection();
            }

            if (playerAim != null) {
                playerAim.EnterGravitySection();
            }

            StartCoroutine(RotateTransform(other.transform, 90f));
            StartCoroutine(RotateCamera(90f));
            StartCoroutine(EnableMonsterDelayed());
        }
    }

    private IEnumerator RotateTransform(Transform targetTransform, float targetAngle) {
        float startAngle = targetTransform.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration) {
            elapsedTime += Time.deltaTime;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, elapsedTime / rotationDuration);
            targetTransform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        targetTransform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private IEnumerator RotateCamera(float targetAngle) {
        float startAngle = mainCamera.transform.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration) {
            elapsedTime += Time.deltaTime;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, elapsedTime / rotationDuration);
            mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    private IEnumerator EnableMonsterDelayed() {
        yield return new WaitForSeconds(monsterEnableDelay);

        if (sectionMonster != null && monsterSpawnPoint != null) {
            sectionMonster.transform.position = monsterSpawnPoint.position;
            sectionMonster.gameObject.SetActive(true);
            StartCoroutine(RotateTransform(sectionMonster.transform, 90f));
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            PlayerAim playerAim = other.GetComponent<PlayerAim>();

            if (audioSource != null) {
                audioSource.Stop();
            }

            if (playerMovement != null) {
                playerMovement.ExitGravitySection();
            }

            if (playerAim != null) {
                playerAim.ExitGravitySection();
            }
        }
    }

    private void OnDestroy() {
        // Ensure audio stops when section is destroyed
        if (audioSource != null) {
            audioSource.Stop();
        }
    }
}
