using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private MonsterAI monsterAI;
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float gravitySectionSpeed = 10f;

    [Header("Sprint Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDepletionRate = 30f;
    [SerializeField] private float staminaRegenRate = 20f;
    [SerializeField] private float staminaRegenDelay = 1f;

    private float currentSpeed;
    private float currentStamina;
    private float lastSprintTime;
    private bool canSprint = true;
    private Rigidbody2D rb;
    private bool hasInfiniteStamina = false;
    private float infiniteStaminaTimer = 0f;
    private float speedModifier = 1f;
    private bool isSpeedModified = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private float normalPitch = 1f;
    [SerializeField] private float sprintPitch = 1.5f;
    [SerializeField] private AudioSource audioSource;

    [Header("Gravity Section Settings")]
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private Vector2 gravityScaleSize = new Vector2(2f, 2f); // Adjust these values as needed

    private bool isInGravitySection = false;
    private bool isOnLadder = false;
    private Ladder currentLadder;
    private Vector2 originalGravity;
    private Quaternion originalRotation;
    private Vector2 originalScale;

    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.sharedMaterial = new PhysicsMaterial2D {
            friction = 0f,
            bounciness = 0f
        };
        currentSpeed = normalSpeed;
        currentStamina = maxStamina;

        originalScale = transform.localScale;
        originalGravity = Physics2D.gravity;
        originalRotation = transform.rotation;

        //audioSource = gameObject.GetComponent<AudioSource>();
        if(audioSource != null) {
            audioSource.clip = movementSound;
            audioSource.loop = true;
            audioSource.pitch = normalPitch;
        }
    }

    private void Update() {
        HandleStamina();
        HandleMovementSound();
    }

    private void FixedUpdate() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (isInGravitySection) {
            if (isOnLadder) {
                Vector2 movement = new Vector2(-verticalInput, horizontalInput);
                if (movement.magnitude > 1f) {
                    movement.Normalize();
                }
                rb.linearVelocity = movement * climbSpeed;
            }
            else {
                Vector2 movement = new Vector2(horizontalInput, 0);
                if(movement.magnitude > 1f) {
                    movement.Normalize();
                }

                float modifiedSpeed = currentSpeed * speedModifier;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, movement.x * modifiedSpeed);
            }
        }
        else {
            Vector2 movement = new Vector2(horizontalInput, verticalInput);

            if (movement.magnitude > 1f) {
                movement.Normalize();
            }

            IsMoving = movement.magnitude > 0.1f;  // Update the public property
            IsSprinting = IsMoving && Input.GetKey(KeyCode.LeftShift) && canSprint && currentStamina > 0f;  // Update sprint state

            currentSpeed = IsSprinting ? sprintSpeed : normalSpeed;  // Use IsSprinting property

            float modifiedSpeed = currentSpeed * speedModifier;

            rb.linearVelocity = movement * modifiedSpeed;
        }        
    }

    public void EnterGravitySection() {
        isInGravitySection = true;
        rb.gravityScale = 1f;
        Physics2D.gravity = Vector2.right * Physics2D.gravity.magnitude;

        transform.localScale = new Vector3(
            originalScale.x * gravityScaleSize.x,
            originalScale.y * gravityScaleSize.y,
            transform.localScale.z
        );

        currentSpeed = gravitySectionSpeed;
    }

    public void ExitGravitySection() {
        isInGravitySection = false;
        rb.gravityScale = 0f;
        Physics2D.gravity = originalGravity;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        currentSpeed = normalSpeed;
    }

    public void ApplySpeedModifier(float modifier) {
        if (!isSpeedModified) {
            speedModifier = modifier;
            isSpeedModified = true;
        }
    }

    public void RemoveSpeedModifier() {
        speedModifier = 1f;
        isSpeedModified = false;
    }

    private void HandleMovementSound() {
        if (isInGravitySection) {
            if (audioSource.isPlaying) {
                audioSource.Stop();
            }
            return;
        }

        audioSource.pitch = IsSprinting ? sprintPitch : normalPitch;

        if (IsMoving && !audioSource.isPlaying) {
            audioSource.Play();
        }
        else if (!IsMoving && audioSource.isPlaying) {
            audioSource.Stop();
        }
    }

    private void HandleStamina() {
        if(hasInfiniteStamina) {
            if(infiniteStaminaTimer > 0) {
                infiniteStaminaTimer -= Time.deltaTime;
            }
            else {
                hasInfiniteStamina = false;
            }

            currentStamina = maxStamina;
            return;
        }

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);

        // Depleting stamina while sprinting
        if(wantsToSprint && canSprint && currentStamina > 0f) {
            currentStamina = Mathf.Max(0, currentStamina - staminaDepletionRate * Time.deltaTime);
            lastSprintTime = Time.time;

            // Disable sprinting if stamina is depleting
            if(currentStamina <= 0f) {
                canSprint = false;
            }
        }
        // Regenerating stamina
        else if(Time.time > lastSprintTime + staminaRegenDelay) {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);

            if(currentStamina > maxStamina * 0.2f) {
                canSprint = true;
            }
        }
    }

    public void InfiniteStamina(float val) {
        hasInfiniteStamina = true;
        infiniteStaminaTimer = val;
    }

    public float GetStaminaPercentage() {
        return currentStamina / maxStamina;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Ladder") && isInGravitySection) {
            currentLadder = other.GetComponent<Ladder>();
            if (currentLadder != null) {
                isOnLadder = true;
                rb.gravityScale = 0f;
            }
        }

        PatrolSection section = other.GetComponent<PatrolSectionBounds>()?.Section;

        Debug.Log("Player entered section: " + section?.sectionName);

        if(section!=null) {
            monsterAI.OnPlayerEnteredSection(section);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Ladder") && isInGravitySection) {
            ExitLadder();
        }
    }

    private void ExitLadder() {
        isOnLadder = false;
        currentLadder = null;

        if (isInGravitySection) {
            rb.gravityScale = 1f;
        }
    }
}
