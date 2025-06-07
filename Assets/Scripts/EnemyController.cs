using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
   public enum EnemyState
   {
      Patrol,
      Investigating,
      Chasing,
      Dead
   }

   [Header("Navigation Settings")]
   [SerializeField] private NavMeshAgent agent;

   [Header("Speed Settings")]
   [SerializeField] private float patrolSpeed = 3f;
   [SerializeField] private float investigateSpeed = 4f;
   [SerializeField] private float chaseSpeed = 6f;
   public static float speedModifier = 1f;

   [Header("Distance Settings")]
   [SerializeField] private float patrolRadius = 40f;
   [SerializeField] private float investigationDistance = 30f;
   [SerializeField] private float chaseExitDistance = 35f;
   [SerializeField] private float killDistance = 2f;
   [SerializeField] private float returnToPatrolDistance = 50f;

   [Header("Behavior Settings")]
   [SerializeField] private float sightCheckInterval = 0.2f;
   [SerializeField] private float deathDuration = 15f;
   [SerializeField] private LayerMask obstacleLayers;

   [Header("State Timing Controls")]
   [SerializeField] private float minPatrolTime = 2f;           // Mínimo 2s em patrol
   [SerializeField] private float minInvestigateTime = 1.5f;    // Mínimo 1.5s em investigating
   [SerializeField] private float minChaseTime = 1f;           // Mínimo 1s em chase
   [SerializeField] private float minDeadTime = 0.5f;          // Mínimo 0.5s para confirmar morte

   [Header("X-Ray Vision")]
   public static bool seeEnemy = false;
   public Material xrayMaterial;
   [SerializeField] private Material[] originalMats;
   [SerializeField] private Renderer rend;

   [Header("Debug Info")]
   public EnemyState currentState;
   [SerializeField] private Vector3 currentTarget;
   [SerializeField] private float distanceToPlayer;
   [SerializeField] private Vector3 lastKnownPlayerPosition;
   [SerializeField] private bool hasReachedTarget;
   [SerializeField] private bool hasLineOfSight;
   [SerializeField] private string currentAnimationState;
   [SerializeField] private float timeInCurrentState;

   private Transform player;
   private float sightCheckTimer;
   private float deathTimer;
   private float stateStartTime;
   private bool targetSet = false;

   // FLAGS para cada estado
   private bool isInPatrol = false;
   private bool isInInvestigating = false;
   private bool isInChase = false;
   private bool isInDead = false;

   [Header("Audio & Visual")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private Animator animator;

   void Awake()
   {
      agent = GetComponent<NavMeshAgent>();
      audioSource = GetComponent<AudioSource>();
      animator = GetComponent<Animator>();

      if (rend != null)
      {
         originalMats = rend.materials;
      }
   }

   void Start()
   {
      player = GameObject.FindGameObjectWithTag("Player").transform;

      agent.autoBraking = true;
      agent.stoppingDistance = 1f;

      ChangeState(EnemyState.Patrol);

      Debug.Log($"[EnemyController] Started in {currentState} state");
   }

   void Update()
   {
      if (GameManager.Instance.gameOver)
      {
         animator?.SetTrigger("ToIdle");
         return;
      }

      distanceToPlayer = Vector3.Distance(transform.position, player.position);
      timeInCurrentState = Time.time - stateStartTime;

      UpdateLineOfSight();
      UpdateAnimationDebug();

      Debug.Log($"[EnemyController] State: {currentState}, TimeInState: {timeInCurrentState:F1}s, Distance: {distanceToPlayer:F1}, LineOfSight: {hasLineOfSight}, AnimState: {currentAnimationState}");

      if (distanceToPlayer < killDistance && currentState != EnemyState.Dead)
      {
         Debug.Log("[EnemyController] KILLING PLAYER!");
         KillPlayer();
         return;
      }

      HandleCurrentState();
      HandleXRayVision();

      if (audioSource != null)
         audioSource.volume = SoundManager.Instance.sfxVolume;

      if (currentState == EnemyState.Patrol)
      {
         animator.SetTrigger("ToWalk");
      }
      if (currentState == EnemyState.Investigating)
      {
         animator.SetTrigger("ToWalk");
      }
      if (currentState == EnemyState.Chasing)
      {
         animator.SetTrigger("ToChase");
      }
      if (currentState == EnemyState.Dead)
      {
         animator.SetTrigger("ToDie");
      }
   }

   void UpdateAnimationDebug()
   {
      if (animator != null)
      {
         var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
         if (stateInfo.IsName("Shadowlop_WALK"))
            currentAnimationState = "WALK";
         else if (stateInfo.IsName("Shadowlop_CHASE"))
            currentAnimationState = "CHASE";
         else if (stateInfo.IsName("Shadowlop_DEAD"))
            currentAnimationState = "DEAD";
         else if (stateInfo.IsName("Shadowlop_IDLE"))
            currentAnimationState = "IDLE";
         else
            currentAnimationState = "UNKNOWN";
      }
   }

   void UpdateLineOfSight()
   {
      sightCheckTimer += Time.deltaTime;

      if (sightCheckTimer >= sightCheckInterval)
      {
         sightCheckTimer = 0f;
         hasLineOfSight = CheckLineOfSight();
      }
   }

   bool CheckLineOfSight()
   {
      Vector3 directionToPlayer = (player.position - transform.position).normalized;
      float distanceToPlayerCheck = Vector3.Distance(transform.position, player.position);

      Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayerCheck, Color.red, sightCheckInterval);

      if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayerCheck, obstacleLayers))
      {
         return false;
      }

      return true;
   }

   void HandleXRayVision()
   {
      if (seeEnemy && (!hasLineOfSight || distanceToPlayer > 10))
      {
         SetXRay(true);
      }
      else
      {
         SetXRay(false);
      }
   }

   public void SetXRay(bool on)
   {
      if (rend == null) return;

      if (on)
      {
         var arr = new Material[originalMats.Length];
         for (int i = 0; i < arr.Length; i++)
            arr[i] = xrayMaterial;
         rend.materials = arr;
      }
      else
      {
         rend.materials = originalMats;
      }
   }

   void HandleCurrentState()
   {
      switch (currentState)
      {
         case EnemyState.Patrol:
            HandlePatrolState();
            break;
         case EnemyState.Investigating:
            HandleInvestigatingState();
            break;
         case EnemyState.Chasing:
            HandleChasingState();
            break;
         case EnemyState.Dead:
            HandleDeadState();
            break;
      }
   }

   void HandlePatrolState()
   {
      Debug.Log($"[PATROL] Target set: {targetSet}, Reached: {HasReachedTarget()}, TimeInState: {timeInCurrentState:F1}s, MinTime: {minPatrolTime}s");

      // SÓ pode mudar de estado depois do tempo mínimo
      if (timeInCurrentState >= minPatrolTime)
      {
         if (distanceToPlayer <= investigationDistance)
         {
            Debug.Log("[PATROL] Player detected! Switching to INVESTIGATING");
            lastKnownPlayerPosition = player.position;
            ChangeState(EnemyState.Investigating);
            return;
         }
      }
      else
      {
         Debug.Log($"[PATROL] Still in minimum time - {minPatrolTime - timeInCurrentState:F1}s remaining");
      }

      if (!targetSet || HasReachedTarget())
      {
         Debug.Log("[PATROL] Setting new patrol target");
         SetRandomPatrolTarget();
      }
   }

   void HandleInvestigatingState()
   {
      Debug.Log($"[INVESTIGATE] TimeInState: {timeInCurrentState:F1}s, MinTime: {minInvestigateTime}s, HasLineOfSight: {hasLineOfSight}");

      // SÓ pode mudar de estado depois do tempo mínimo
      if (timeInCurrentState >= minInvestigateTime)
      {
         if (hasLineOfSight)
         {
            Debug.Log("[INVESTIGATE] Clear line of sight! Switching to CHASING");
            ChangeState(EnemyState.Chasing);
            return;
         }

         if (distanceToPlayer >= returnToPatrolDistance)
         {
            Debug.Log("[INVESTIGATE] Player too far, returning to PATROL");
            ChangeState(EnemyState.Patrol);
            return;
         }
      }
      else
      {
         Debug.Log($"[INVESTIGATE] Still in minimum time - {minInvestigateTime - timeInCurrentState:F1}s remaining");
      }

      if (HasReachedTarget() && distanceToPlayer <= investigationDistance)
      {
         Debug.Log("[INVESTIGATE] Reached target but player still near, updating position");
         lastKnownPlayerPosition = player.position;
         SetTarget(lastKnownPlayerPosition);
      }
   }

   void HandleChasingState()
   {
      Debug.Log($"[CHASE] TimeInState: {timeInCurrentState:F1}s, MinTime: {minChaseTime}s, HasLineOfSight: {hasLineOfSight}");

      SetTarget(player.position);

      // SÓ pode sair do chase depois do tempo mínimo
      if (timeInCurrentState >= minChaseTime)
      {
         bool shouldExitChase = false;

         if (!hasLineOfSight && distanceToPlayer > chaseExitDistance)
         {
            shouldExitChase = true;
            Debug.Log("[CHASE] Lost line of sight AND player far - exiting chase");
         }
         else if (distanceToPlayer > chaseExitDistance * 1.5f)
         {
            shouldExitChase = true;
            Debug.Log("[CHASE] Player very far - exiting chase");
         }

         if (shouldExitChase)
         {
            Debug.Log("[CHASE] EXITING CHASE! Switching to INVESTIGATING");
            lastKnownPlayerPosition = player.position;
            ChangeState(EnemyState.Investigating);
            return;
         }
      }
      else
      {
         Debug.Log($"[CHASE] Still in minimum time - {minChaseTime - timeInCurrentState:F1}s remaining");
      }
   }

   void HandleDeadState()
   {
      Debug.Log($"[DEAD] TimeInState: {timeInCurrentState:F1}s, DeathDuration: {deathDuration}s, MinTime: {minDeadTime}s");

      // SÓ pode ressuscitar depois do tempo mínimo E tempo total de morte
      if (timeInCurrentState >= minDeadTime && timeInCurrentState >= deathDuration)
      {
         Debug.Log("[DEAD] Respawning!");
         agent.isStopped = false;

         if (distanceToPlayer <= investigationDistance)
         {
            Debug.Log("[DEAD] Player near on respawn, going to INVESTIGATING");
            lastKnownPlayerPosition = player.position;
            ChangeState(EnemyState.Investigating);
         }
         else
         {
            Debug.Log("[DEAD] Going to PATROL");
            ChangeState(EnemyState.Patrol);
         }
      }
      else
      {
         float remainingTime = Mathf.Max(minDeadTime - timeInCurrentState, deathDuration - timeInCurrentState);
         Debug.Log($"[DEAD] Still dead - {remainingTime:F1}s remaining");
      }
   }

   void SetRandomPatrolTarget()
   {
      Vector3 randomTarget = Vector3.zero;
      bool validTargetFound = false;

      float maxX = GameData.MazeColumns * GameData.CellSize;
      float maxZ = GameData.MazeRows * GameData.CellSize;

      for (int attempts = 0; attempts < 30; attempts++)
      {
         Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
         Vector3 candidatePosition = new Vector3(
             player.position.x + randomDirection.x,
             player.position.y,
             player.position.z + randomDirection.y
         );

         candidatePosition.x = Mathf.Clamp(candidatePosition.x, 2f, maxX - 2f);
         candidatePosition.z = Mathf.Clamp(candidatePosition.z, 2f, maxZ - 2f);

         if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit hit, GameData.CellSize, NavMesh.AllAreas))
         {
            float distanceFromEnemy = Vector3.Distance(transform.position, hit.position);
            if (distanceFromEnemy > 10f)
            {
               randomTarget = hit.position;
               validTargetFound = true;
               Debug.Log($"[SetRandomTarget] Valid target found: {randomTarget}");
               break;
            }
         }
      }

      if (!validTargetFound)
      {
         Debug.LogWarning("[SetRandomTarget] Could not find valid random target, using fallback");
         if (NavMesh.SamplePosition(player.position, out NavMeshHit fallbackHit, patrolRadius, NavMesh.AllAreas))
         {
            randomTarget = fallbackHit.position;
         }
         else
         {
            randomTarget = player.position;
         }
      }

      SetTarget(randomTarget);
   }

   void SetTarget(Vector3 target)
   {
      currentTarget = target;
      agent.SetDestination(currentTarget);
      targetSet = true;
      hasReachedTarget = false;

      Debug.Log($"[SetTarget] New target set: {currentTarget}");
   }

   bool HasReachedTarget()
   {
      if (!targetSet) return true;

      float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
      bool reached = distanceToTarget <= 2f;

      if (reached && !hasReachedTarget)
      {
         Debug.Log($"[ReachedTarget] Reached target! Distance: {distanceToTarget:F1}");
         hasReachedTarget = true;
      }

      return reached;
   }

   void ChangeState(EnemyState newState)
   {
      if (currentState == newState) return;

      Debug.Log($"[StateChange] {currentState} → {newState} (was in {currentState} for {timeInCurrentState:F1}s)");

      EnemyState previousState = currentState;
      currentState = newState;
      targetSet = false;
      stateStartTime = Time.time;  // Reset do timer de estado

      // Reset todas as flags
      isInPatrol = false;
      isInInvestigating = false;
      isInChase = false;
      isInDead = false;

      switch (currentState)
      {
         case EnemyState.Patrol:
            agent.speed = patrolSpeed * speedModifier;
            isInPatrol = true;
            Debug.Log("[ANIMATION] Calling ToWalk for PATROL");
            animator?.SetTrigger("ToWalk");
            break;

         case EnemyState.Investigating:
            agent.speed = investigateSpeed * speedModifier;
            isInInvestigating = true;

            // SÓ chama ToWalk se não veio de Chasing
            if (previousState != EnemyState.Chasing)
            {
               Debug.Log("[ANIMATION] Calling ToWalk for INVESTIGATING");
               animator?.SetTrigger("ToWalk");
            }
            else
            {
               Debug.Log("[ANIMATION] SKIPPING ToWalk - came from Chase");
            }
            SetTarget(lastKnownPlayerPosition);
            break;

         case EnemyState.Chasing:
            agent.speed = chaseSpeed * speedModifier;
            isInChase = true;
            Debug.Log("[ANIMATION] Calling ToChase for CHASING");
            animator?.SetTrigger("ToChase");

            // Força animação no próximo frame
            StartCoroutine(ForceAnimation("ToChase"));
            break;

         case EnemyState.Dead:
            agent.speed = 0f;
            agent.isStopped = true;
            isInDead = true;
            Debug.Log("[ANIMATION] Calling ToDie for DEAD");
            animator?.SetTrigger("ToDie");

            // Força animação no próximo frame
            StartCoroutine(ForceAnimation("ToDie"));
            break;
      }

      Debug.Log($"[StateChange] Speed set to: {agent.speed}, Flags: Patrol={isInPatrol}, Investigate={isInInvestigating}, Chase={isInChase}, Dead={isInDead}");
   }

   IEnumerator ForceAnimation(string triggerName)
   {
      yield return null; // Wait 1 frame
      yield return null; // Wait another frame for safety

      if (animator != null)
      {
         Debug.Log($"[ANIMATION] FORCING {triggerName} animation again!");
         animator.SetTrigger(triggerName);
      }
   }

   public void Die()
   {
      Debug.Log("[EnemyController] Enemy was shot! Dying...");
      ChangeState(EnemyState.Dead);
   }

   void KillPlayer()
   {
      Debug.Log("[EnemyController] Killing player!");
      GameManager.Instance.PlayerDied();
      SoundManager.Instance.PlayJumpscareSound(audioSource);
   }
}