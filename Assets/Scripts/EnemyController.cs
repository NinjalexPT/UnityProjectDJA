using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
   public enum EnemyState
   {
      Walking,
      Investigating,
      Chasing,
      Dead
   }

   [Header("Navigation Settings")]
   [SerializeField] private NavMeshAgent agent;
   [SerializeField] private float walkingSpeed = 3f;
   public static float speedModifier = 1f;
   [SerializeField] private float chasingSpeed = 6f;
   [SerializeField] private float detectionRadius = 30f;
   [SerializeField] private float deathDistance = 1f;
   [SerializeField] private LayerMask obstacleLayers;

   [Header("Behavior Tuning")]
   [SerializeField] private float maxInvestigationTime = 10f;
   [SerializeField] private float sightCheckInterval = 0.2f;
   [SerializeField] private float walkRadius = 20f;
   [SerializeField] private float pathUpdateDelay = 2f; // Novo parâmetro

   [Header("Sound System")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private float breathingSoundChance = 5f;

   private Transform player;
   public EnemyState currentState;
   private Vector3 investigationTarget;
   private float investigationTimer;
   private float sightCheckTimer;
   private float soundCheckTimer;
   private Vector3 currentWalkTarget;
   private float lastPathUpdateTime; // Controle de tempo

   [Header("X-Ray Vision")]
   public static bool seeEnemy = false;
   public Material xrayMaterial;

   [SerializeField] private Material[] originalMats;
   [SerializeField] private Renderer rend;

   private Animator animator;

   void Awake()
   {
      // Guarda materiais originais para o XRay
      originalMats = rend.materials;
   }

   void Start()
   {
      player = GameObject.FindGameObjectWithTag("Player").transform;
      currentState = EnemyState.Walking;
      agent.speed = walkingSpeed * speedModifier;
      agent.autoBraking = true;
      agent.stoppingDistance = 1f;
      animator = GetComponent<Animator>();
      SetNewWalkTarget();
   }

   void Update()
   {
      if (GameManager.Instance.gameOver) return;

      float distanceToPlayer = Vector3
          .Distance(transform.position, player.position);

      HandleDeathCondition(distanceToPlayer);
      UpdateStateMachine(distanceToPlayer);
      HandleBreathingSounds(distanceToPlayer);

      audioSource.volume = SoundManager.Instance.sfxVolume;

      // X-Ray toggle
      if (seeEnemy) SetXRay(true);
      else SetXRay(false);
   }

   public void SetXRay(bool on)
   {
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

   void HandleBreathingSounds(float distance)
   {
      if (currentState == EnemyState.Walking &&
          distance > detectionRadius)
      {
         soundCheckTimer += Time.deltaTime;
         if (soundCheckTimer > 1f)
         {
            soundCheckTimer = 0;
            if (Random.Range(0, 100) < breathingSoundChance)
               SoundManager.Instance
                   .PlayBreathingSound(audioSource);
         }
      }
   }

   void UpdateStateMachine(float distanceToPlayer)
   {
      switch (currentState)
      {
         case EnemyState.Walking:
            HandleWalkingState(distanceToPlayer);
            break;
         case EnemyState.Investigating:
            HandleInvestigationState(distanceToPlayer);
            break;
         case EnemyState.Chasing:
            HandleChaseState(distanceToPlayer);
            break;
         case EnemyState.Dead:
            DieTemporarily(15f);
            break;
      }
   }

   void HandleWalkingState(float distanceToPlayer)
   {
      if (distanceToPlayer <= detectionRadius)
      {
         if (HasClearLineOfSight())
         {
            StartChasing();
            return;
         }
         else if (distanceToPlayer < detectionRadius * 0.5f)
         {
            StartInvestigating(player.position);
         }
      }

      if (agent.pathStatus == NavMeshPathStatus.PathComplete &&
          agent.remainingDistance <= agent.stoppingDistance &&
          Time.time - lastPathUpdateTime > pathUpdateDelay)
      {
         SetNewWalkTarget();
         lastPathUpdateTime = Time.time;
      }
   }

   void HandleInvestigationState(float distanceToPlayer)
   {
      investigationTimer -= Time.deltaTime;

      if (HasClearLineOfSight())
      {
         StartChasing();
         return;
      }

      if (distanceToPlayer <
          Vector3.Distance(transform.position,
                            investigationTarget) * 0.8f)
      {
         investigationTarget = player.position;
         agent.SetDestination(investigationTarget);
      }

      if (agent.remainingDistance <= agent.stoppingDistance ||
          investigationTimer <= 0 ||
          distanceToPlayer > detectionRadius)
      {
         ReturnToWalking();
      }
   }

   void HandleChaseState(float distanceToPlayer)
   {
      agent.SetDestination(player.position);

      if (distanceToPlayer > detectionRadius)
         ReturnToWalking();
   }

   bool HasClearLineOfSight()
   {
      sightCheckTimer += Time.deltaTime;
      if (sightCheckTimer < sightCheckInterval) return false;
      sightCheckTimer = 0;

      Vector3 direction =
          player.position - transform.position;
      float distance =
          Vector3.Distance(transform.position,
                           player.position);

      // testamos colisão com layers de obstáculo
      if (!Physics.Raycast(transform.position,
                           direction.normalized,
                           out RaycastHit hit,
                           distance,
                           obstacleLayers))
      {
         return true;
      }

      Debug.DrawRay(transform.position,
                    direction,
                    Color.red,
                    0.5f);
      return hit.collider.CompareTag("Player");
   }

   void StartChasing()
   {
      currentState = EnemyState.Chasing;
      agent.speed = chasingSpeed;
      agent.SetDestination(player.position);
      animator.SetTrigger("ToChase");
   }

   void StartInvestigating(Vector3 position)
   {
      currentState = EnemyState.Investigating;
      investigationTarget = position;
      agent.SetDestination(position);
      investigationTimer = maxInvestigationTime;
      animator.SetTrigger("ToIdle");
   }

   void ReturnToWalking()
   {
      currentState = EnemyState.Walking;
      agent.speed = walkingSpeed;
      SetNewWalkTarget();
      animator.SetTrigger("ToWalk");
   }

   void SetNewWalkTarget()
   {
      Vector3 randomDirection =
          Random.insideUnitSphere * walkRadius;
      randomDirection += player.position;
      randomDirection.y = player.position.y;

      int attempts = 0;
      bool validPath = false;

      while (attempts < 5 && !validPath)
      {
         if (NavMesh.SamplePosition(randomDirection,
                                    out NavMeshHit hit,
                                    walkRadius,
                                    NavMesh.AllAreas))
         {
            currentWalkTarget = hit.position;
            agent.SetDestination(currentWalkTarget);
            if (agent.pathPending) return;
            validPath =
                agent.pathStatus ==
                NavMeshPathStatus.PathComplete;
         }
         attempts++;
      }

      if (!validPath)
         Debug.LogWarning("Falha ao encontrar caminho válido");
   }

   void HandleDeathCondition(float distance)
   {
      // só prenche o estado de Dead e dispara animação uma vez
      if (currentState != EnemyState.Dead &&
          distance < deathDistance)
      {
         currentState = EnemyState.Dead;
         agent.isStopped = true;
         // dispara a animação de morte — crie no Animator
         // um Trigger chamado "ToDead"
         animator.SetTrigger("ToDie");
         SoundManager.Instance
             .PlayJumpscareSound(audioSource);
         GameManager.Instance.PlayerDied();
      }
   }

   public void DieTemporarily(float sec)
   {
      StartCoroutine(DieCoroutine(sec));
   }

   private IEnumerator DieCoroutine(float sec)
   {
      agent.isStopped = true;
      yield return new WaitForSeconds(sec);
      currentState = EnemyState.Walking;
      agent.isStopped = false;
      SetNewWalkTarget();
   }
}