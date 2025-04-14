using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
  public enum EnemyState
  {
    Walking,
    Chasing
  }

  [SerializeField]
  private NavMeshAgent agent;
  public EnemyState currentState = EnemyState.Walking;
  public float walkingSpeed = 3f;
  public float chasingSpeedMultiplier = 2f;
  public float detectionThreshold = 30f;
  public float deathThreshold = 1f;
  private Transform playerTransform;
  private Vector3 walkTarget;
  public float walkRadius = 50f; // Adjust this based on your map size
  private int frameCounter = 0;
  public LayerMask viewBlockerLayer; // Assign layers that can block the view (e.g., Walls) in the Inspector

  [Header("Sound system")]
  [SerializeField] private AudioSource audioSource;
  [SerializeField] private float breathingSoundChance = 5f;

  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
    if (agent == null)
    {
      Debug.LogError("NavMeshAgent not found on this GameObject.");
      enabled = false;
      return;
    }

    playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    if (playerTransform == null)
    {
      Debug.LogError("Player not found. Make sure the Player GameObject has the tag 'Player'.");
      enabled = false;
      return;
    }

    SetState(EnemyState.Walking);
  }

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update()
  {

    if (GameManager.Instance.gameOver) return;

    FirstPersonController playerController = FindFirstObjectByType<FirstPersonController>();
    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
    // Temporary Debug Log
    // Debug.Log("Distance to Player: " + distanceToPlayer + ", Detection Threshold: " + detectionThreshold + ", Current State: " + currentState);

    if ((Random.Range(1, 10000) <= breathingSoundChance) && distanceToPlayer > detectionThreshold)
    {
      SoundManager.Instance.PlayBreathingSound(audioSource);
    }

    if (distanceToPlayer < deathThreshold)
    {
      SoundManager.Instance.PlayJumpscareSound(audioSource);
      GameManager.Instance.PlayerDied();
      if (agent != null && agent.isActiveAndEnabled)
      {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
      }
      return;
    }

    switch (currentState)
    {
      case EnemyState.Walking:
        playerController.unlimitedSprint = false;
        agent.speed = walkingSpeed;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
          SetNewRandomWalkTarget();
        }

        if (distanceToPlayer < detectionThreshold)
        {
          // Perform line of sight check every 5 frames
          frameCounter++;
          if (frameCounter % 5 == 0)
          {
            if (HasLineOfSightToPlayer())
            {
              SetState(EnemyState.Chasing);
            }
          }
        }
        break;

      case EnemyState.Chasing:

        playerController.unlimitedSprint = true;
        agent.speed = walkingSpeed * chasingSpeedMultiplier;
        agent.SetDestination(playerTransform.position);

        if (distanceToPlayer > detectionThreshold || !HasLineOfSightToPlayer())
        {
          SetState(EnemyState.Walking);
        }
        break;
    }
  }

  void SetState(EnemyState newState)
  {
    if (currentState == newState) return;

    currentState = newState;
    frameCounter = 0; // Reset frame counter when state changes

    if (currentState == EnemyState.Walking)
    {
      SetNewRandomWalkTarget();
    }
    else if (currentState == EnemyState.Chasing)
    {
      Debug.Log("Enemy is now Chasing!");
    }
  }

  void SetNewRandomWalkTarget()
  {
    Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
    randomDirection += transform.position;
    NavMeshHit hit;
    if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
    {
      walkTarget = hit.position;
      agent.SetDestination(walkTarget);
    }
    else
    {
      Debug.LogWarning("Could not find a valid random walk target on the NavMesh. Trying again...");
    }
  }

  bool HasLineOfSightToPlayer()
  {
    Vector3 directionToPlayer = playerTransform.position - transform.position;
    Ray ray = new Ray(transform.position, directionToPlayer.normalized);
    RaycastHit hit;

    // Perform the raycast, ignoring the enemy's own collider
    if (Physics.Raycast(ray, out hit, detectionThreshold, ~LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer))))
    {
      // Check if the raycast hit the player
      if (hit.collider.CompareTag("Player"))
      {
        return true;
      }
      // Check if the raycast hit a view blocker (e.g., a wall)
      if (viewBlockerLayer != 0 && (viewBlockerLayer & (1 << hit.collider.gameObject.layer)) != 0)
      {
        return false; // Blocked by a view blocker
      }
    }
    return false; // Player not hit or out of range
  }
}