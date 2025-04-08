using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static System.Collections.Specialized.BitVector32;

public class MonsterAI : MonoBehaviour {
    [Header("Target")]
    [SerializeField] private Transform playerTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistanceToPlayer = 1f;
    [SerializeField] private float nextPathUpdateTime = 0.5f;

    [Header("Patrol Settings")]
    [SerializeField] private PatrolSection[] patrolSections;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float searchRadius = 5f;
    [SerializeField] private float searchDuration = 10f;

    [Header("Search Settings")]
    [SerializeField] private float minSearchDistance = 3.6f; // Minimum distance from hiding spot
    [SerializeField] private float maxSearchDistance = 5f; // Maximum distance from hiding spot
    [SerializeField] private int searchPointAttempts = 10; // Attempts to find valid point

    [Header("Detection Settings")]
    [SerializeField] private float normalDetectionRadius = 20f;
    [SerializeField] private float closeDetectionRadius = 10f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Post Sprint Search Settings")]
    [SerializeField] private float postSprintSearchduration = 5f;
    [SerializeField] private float randomMoveRadius = 3f;
    [SerializeField] private float randomeMoveInterval = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float minMovementSpeed = 0.1f;
    [SerializeField] private float maxFootstepVolume = 1f;
    [SerializeField] private float minFootstepVolume = 0.01f;
    [SerializeField] private float maxHearingDistance = 30f;
    [SerializeField] private float minHearingDistance = 2f;
    [SerializeField] private bool useLogarithmicFalloff = true;

    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite patrolSprite;
    [SerializeField] private Sprite searchChaseSprite;
    [SerializeField] private Sprite forcedChaseSprite;



    private NavMeshAgent agent;
    private float pathUpdateTimer;
    private bool isStopped = false;
    private Rigidbody2D rb;
    private bool isPlayerHidden = false;
    private Vector3 lastKnownPosition;
    private int currentPatrolIndex = 0;
    private MonsterState currentState;
    private PlayerMovement playerMovement;
    private bool wasChasing = false;
    private Vector3 lastSprintPosition;
    private bool wasSprintingLastFrame;
    private bool isSearchingLastSprintPosition = false;
    private AudioSource audioSource;
    private float speedModifier = 1f;
    private float baseSpeed;
    private bool isMoving;
    private bool isSpeedModified = false;
    private PatrolSection currentSection;
    private Transform[] currentPatrolPoints;
    private bool isPatrollingForward = true;
    private bool isInChaseSection = false;
    private float originalSpeed;

    private enum MonsterState {
        Patrolling,
        ChasingPlayer,
        SearchingAroundHidingSpot,
        MovingToNextPatrol,
        SearchingLastSprintPosition,
        ForcedChase
    }

    private void Start() {
        // Get or find references
        if (playerTransform == null) {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = footstepSound;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        baseSpeed = moveSpeed;

        // Setup NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Configure agent
        UpdateAgentSpeed();
        agent.stoppingDistance = minDistanceToPlayer;

        rb = GetComponent<Rigidbody2D>();
        if(rb==null) {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateSprite();

        if (patrolSections == null || patrolSections.Length == 0) {
            Debug.LogError("No patrol sections assigned!");
            return;
        }

        if (playerTransform != null) {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();

            // Find initial section if player is in one
            foreach (PatrolSection section in patrolSections) {
                if (section.sectionBounds.bounds.Contains(playerTransform.position)) {
                    SwitchToSection(section);
                    Debug.Log($"Starting at section: {section.sectionName}");
                    break;
                }
            }
        }

        // If no section was found containing the player, use the first section
        if (currentSection == null && patrolSections.Length > 0) {
            Debug.Log("No section containing player found, using first section");
            SwitchToSection(patrolSections[0], true);
        }

        currentState = MonsterState.Patrolling;
        UpdateSprite();
        StartCoroutine(PatrolBehavior());
    }

    private void UpdateSprite() {
        if (spriteRenderer == null) return;

        switch (currentState) {
            case MonsterState.Patrolling:
            case MonsterState.MovingToNextPatrol:
                spriteRenderer.sprite = patrolSprite;
                break;

            case MonsterState.ChasingPlayer:
            case MonsterState.SearchingAroundHidingSpot:
            case MonsterState.SearchingLastSprintPosition:
                spriteRenderer.sprite = searchChaseSprite;
                break;

            case MonsterState.ForcedChase:
                spriteRenderer.sprite = forcedChaseSprite;
                break;
        }
    }


    public void OnPlayerEnteredSection(PatrolSection newSection) {
        Debug.Log($"Monster switching to section: {newSection.sectionName}");
        SwitchToSection(newSection);
    }

    private void UpdateAgentSpeed() {
        if (agent!=null) agent.speed = baseSpeed * speedModifier;
    }

    public void ApplySpeedModifier(float modifier) {
        if (!isSpeedModified) {
            speedModifier = modifier;
            isSpeedModified = true;
            UpdateAgentSpeed();
        }
    }

    public void RemoveSpeedModifier() {
        speedModifier = 1f;
        isSpeedModified = false;
        UpdateAgentSpeed();
    }

    private void SwitchToSection(PatrolSection newSection, bool isInitialSection = false) {
        if (currentSection == newSection) return;

        // Only check cooldown if it's not the initial section assignment
        if (!isInitialSection && !newSection.CanSwitchTo()) return;

        Debug.Log("Section assigned");
        currentSection = newSection;
        newSection.MarkSwitched();
        currentPatrolPoints = newSection.GetPatrolPoints();
        Debug.Log($"Monster switching to section: {newSection.sectionName} with {currentPatrolPoints.Length} patrol points");
        currentPatrolIndex = 0;

        if (currentState == MonsterState.Patrolling) {
            StopAllCoroutines();
            StartCoroutine(PatrolBehavior());
        }
    }

    public void EnterChaseSection(Vector3 spawnPosition, float chaseSpeed = 0f) {
        isInChaseSection = true;

        transform.position = spawnPosition;
        agent.Warp(spawnPosition);

        //originalSpeed = moveSpeed;

        //if (chaseSpeed > 0) {
        //    moveSpeed = chaseSpeed;
        //    UpdateAgentSpeed();
        //}

        StopAllCoroutines();

        currentState = MonsterState.ForcedChase;
        UpdateSprite();
        StartCoroutine(ForcedChaseBehavior());
    }

    public void ExitChaseSection() {
        isInChaseSection = false;

        StartCoroutine(TemporaryDisable());
    }

    private IEnumerator ForcedChaseBehavior() {
        yield return new WaitForSeconds(1f);

        while (isInChaseSection && playerTransform!=null) {
            agent.SetDestination(playerTransform.position);
            yield return new WaitForSeconds(nextPathUpdateTime);
        }
    }

    private IEnumerator TemporaryDisable() {
        gameObject.SetActive(false);
        yield break;
    }

    private void Update() {
        if (playerTransform == null || isStopped) {
            StopFootsteps();
            return;
        }

        if (!isInChaseSection && currentState != MonsterState.SearchingAroundHidingSpot) CheckPlayerState();

        // Update rotation to face movement direction
        if (agent.velocity.magnitude > 0.1f) {
            float angle = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        HandleFootsteps();

        if (playerTransform != null) {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < 2.5f && !isPlayerHidden) {
                GameManager.Instance.GameOver();
            }
        }
    }

    private void HandleFootsteps() {
        if (playerTransform == null) return;

        bool isCurrentlyMoving = agent.velocity.magnitude > minMovementSpeed;

        if (isCurrentlyMoving) {
            if (!audioSource.isPlaying) {
                audioSource.Play();
                isMoving = true;
            }

            UpdateFootstepVolume();
        }
        else if (!isCurrentlyMoving && audioSource.isPlaying) {
            StopFootsteps();
        }
    }

    private void UpdateFootstepVolume() {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float targetVolume;

        if (distanceToPlayer > maxHearingDistance) {
            targetVolume = 0f;
        }
        else {
            float clampedDistance = Mathf.Clamp(distanceToPlayer, minHearingDistance, maxHearingDistance);
            float volumeMultiplier;

            if (useLogarithmicFalloff) {
                float normalizedDistance = (clampedDistance - minHearingDistance) / (maxHearingDistance - minHearingDistance);
                volumeMultiplier = Mathf.Lerp(1f, 0f, Mathf.Log10(normalizedDistance * 9f + 1f));
            }
            else {
                volumeMultiplier = 1f - ((clampedDistance - minHearingDistance) / (maxHearingDistance - minHearingDistance));
            }

            targetVolume = Mathf.Lerp(minFootstepVolume, maxFootstepVolume, volumeMultiplier);
        }

        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 10f);
    }

    private void StopFootsteps() {
        if(audioSource.isPlaying) {
            audioSource.Stop();
            isMoving = false;
        }
    }

    private void CheckPlayerState() {
        if (isPlayerHidden) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool shouldChase = false;

        bool isPlayerMoving = playerMovement.IsMoving;
        bool isPlayerSprinting = playerMovement.IsSprinting;

        if (wasSprintingLastFrame && !isPlayerSprinting) {
            lastSprintPosition = playerTransform.position;
            StartCoroutine(SearchLastSprintPosition());
            wasSprintingLastFrame = false;
            return;
        }

        wasSprintingLastFrame = isPlayerSprinting;

        if (!isPlayerMoving) shouldChase = distanceToPlayer <= closeDetectionRadius;
        else if (isPlayerSprinting) shouldChase = true;
        else shouldChase = distanceToPlayer <= normalDetectionRadius;

        if(shouldChase && currentState != MonsterState.ChasingPlayer) {
            wasChasing = true;
            currentState = MonsterState.ChasingPlayer;
            UpdateSprite();
            StopAllCoroutines();
            UpdatePath();
        }
        else if (!shouldChase && currentState == MonsterState.ChasingPlayer) {
            wasChasing = false;
            if (!wasSprintingLastFrame) {
                currentState = MonsterState.Patrolling;
                UpdateSprite();
                StartCoroutine(PatrolBehavior());
            }
        }

        if(currentState == MonsterState.ChasingPlayer) {
            pathUpdateTimer -= Time.deltaTime;
            if(pathUpdateTimer <= 0) {
                UpdatePath();
                pathUpdateTimer = nextPathUpdateTime;
            }
        }
    }

    private IEnumerator SearchLastSprintPosition() {
        currentState = MonsterState.SearchingLastSprintPosition;
        UpdateSprite();
        isSearchingLastSprintPosition = true;
        agent.SetDestination(lastSprintPosition);

        while (Vector3.Distance(transform.position, lastSprintPosition) > agent.stoppingDistance + 0.1f) {
            if (ShouldInterruptSearch()) { 
                isSearchingLastSprintPosition = false;
                yield break;
            }
            yield return null;
        }

        float searchStartTime = Time.time;
        float nextMoveTime = 0f;

        while (Time.time - searchStartTime < postSprintSearchduration) {
            if (ShouldInterruptSearch()) { 
                isSearchingLastSprintPosition = false;
                yield break;
            }

            if(Time.time >= nextMoveTime) {
                Vector3 randomPosition = GetRandomPositionAroundPoint(lastSprintPosition, randomMoveRadius);
                agent.SetDestination(randomPosition);
                nextMoveTime = Time.time + randomeMoveInterval;
            }

            yield return null;
        }

        if (currentState == MonsterState.SearchingLastSprintPosition) {
            currentState = MonsterState.Patrolling;
            UpdateSprite();
            StartCoroutine(PatrolBehavior());
        }

        isSearchingLastSprintPosition = false;
    }

    private bool ShouldInterruptSearch() {
        if (isPlayerHidden) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (playerMovement.IsSprinting) return true;
        if (playerMovement.IsMoving && distanceToPlayer <= normalDetectionRadius) return true;
        if (distanceToPlayer <= closeDetectionRadius) return true;

        return false;
    }

    private Vector3 GetRandomPositionAroundPoint(Vector3 center, float radius) {
        for(int i=0; i<30; i++) {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPoint = center + new Vector3(randomCircle.x, randomCircle.y, 0);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) return hit.position;
        }

        return center;
    }

    private void UpdatePath() {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) {
            agent.SetDestination(playerTransform.position);
        }
    }

    // Visualize path in scene view
    private void OnDrawGizmos() {
        if (patrolSections == null) return;

        foreach (PatrolSection section in patrolSections) {
            if (section.sectionBounds == null || section.patrolPointsParent == null) continue;

            Gizmos.color = (section == currentSection) ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(section.sectionBounds.bounds.center, section.sectionBounds.bounds.size);

            Transform[] points = section.GetPatrolPoints();
            if (points != null) {
                for (int i = 0; i < points.Length; i++) {
                    if (points[i] == null) continue;
                    Gizmos.DrawSphere(points[i].position, 0.3f);
                    if (i < points.Length - 1 && points[i + 1] != null) {
                        Gizmos.DrawLine(points[i].position, points[i + 1].position);
                    }
                }
            }
        }
    }

    public void tempStop(float time) {
        if(!isStopped) {
            StartCoroutine(StopRoutine(time));
        }
    }

    private IEnumerator StopRoutine(float time) {
        isStopped = true;

        Vector3 previousVelocity = agent.velocity;
        RigidbodyType2D originalBodyType = rb.bodyType;
        bool originalKinematic = rb.isKinematic;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        StopFootsteps();

        yield return new WaitForSeconds(time);

        agent.isStopped = false;
        rb.bodyType = originalBodyType;
        rb.isKinematic = originalKinematic;
        isStopped = false;
    }

    public void OnPlayerHide(Vector3 hidingSpotPosition) {
        isPlayerHidden = true;
        lastKnownPosition = hidingSpotPosition;

        if (wasChasing && !isSearchingLastSprintPosition) {
            StopAllCoroutines();
            StartCoroutine(SearchAroundHidingSpot());
        }
        else {
            if (!isSearchingLastSprintPosition) {
                StopAllCoroutines();
                currentState = MonsterState.Patrolling;
                UpdateSprite();
                StartCoroutine(PatrolBehavior());
            }
        }
    }

    public void OnPlayerUnhide() {
        isPlayerHidden = false;
        currentState = MonsterState.ChasingPlayer;
        UpdateSprite();
        StopAllCoroutines();
    }

    private IEnumerator SearchAroundHidingSpot() {
        Debug.Log("Starting search around hiding spot");
        currentState = MonsterState.SearchingAroundHidingSpot;
        UpdateSprite();
        float searchStartTime = Time.time;

        while (Time.time - searchStartTime < searchDuration) {
            // Try to find a valid position
            Vector3 targetPosition = FindValidSearchPoint();

            if (targetPosition != Vector3.zero) {
                Debug.Log($"Moving to search point: {targetPosition}");
                agent.SetDestination(targetPosition);

                // Wait until reaching the point or path is invalid
                float waitStartTime = Time.time;
                while (Time.time - waitStartTime < 3f && // Timeout after 3 seconds
                       !agent.pathStatus.Equals(NavMeshPathStatus.PathComplete)) {
                    if (!isPlayerHidden) {
                        Debug.Log("Player unhidden during search");
                        yield break;
                    }
                    yield return null;
                }

                // Wait at the point
                yield return new WaitForSeconds(1f);
            }
            else {
                Debug.Log("Couldn't find valid search point");
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log("Search duration ended, starting patrol");
        currentState = MonsterState.Patrolling;
        UpdateSprite();
        StartCoroutine(PatrolBehavior());
    }

    private Vector3 FindValidSearchPoint() {
        for (int i = 0; i < searchPointAttempts; i++) {
            // Generate random direction and distance
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(minSearchDistance, maxSearchDistance);

            // Convert to position
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
            Vector3 targetPosition = lastKnownPosition + direction * distance;

            // Check if position is valid on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas)) {
                return hit.position;
            }
        }
        return Vector3.zero;
    }

    private IEnumerator PatrolBehavior() {
        Debug.Log("Starting patrol behavior");
        while (true) {
            if (currentPatrolPoints == null || currentPatrolPoints.Length == 0) {
                if (currentPatrolPoints == null) Debug.Log("CurrentPatrolPoints null boi");

                Debug.LogWarning("No patrol points assigned for current section");
                yield return new WaitForSeconds(patrolWaitTime);
                continue;
            }

            Debug.Log($"Moving to patrol point {currentPatrolIndex}");
            agent.SetDestination(currentPatrolPoints[currentPatrolIndex].position);

            // Wait until reaching the point or path is invalid
            float waitStartTime = Time.time;
            while (Time.time - waitStartTime < 5f && // Timeout after 5 seconds
                   !agent.pathStatus.Equals(NavMeshPathStatus.PathComplete)) {
                if (!isPlayerHidden) {
                    Debug.Log("Player unhidden during patrol");
                    currentState = MonsterState.ChasingPlayer;
                    UpdateSprite();
                    yield break;
                }
                yield return null;
            }

            yield return new WaitForSeconds(patrolWaitTime);

            // Update patrol index based on direction
            if (isPatrollingForward) {
                currentPatrolIndex++;
                // If we reached the end, switch direction
                if (currentPatrolIndex >= currentPatrolPoints.Length - 1) {
                    isPatrollingForward = false;
                    currentPatrolIndex = currentPatrolPoints.Length - 1;
                }
            }
            else {
                currentPatrolIndex--;
                // If we reached the start, switch direction
                if (currentPatrolIndex <= 0) {
                    isPatrollingForward = true;
                    currentPatrolIndex = 0;
                }
            }
        }
    }
}
