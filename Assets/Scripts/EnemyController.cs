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
   [Space(10)]
   [SerializeField] private float walkingSpeed = 4.5f;
   [SerializeField] private float investigationSpeed = 3f;
   public static float speedModifier = 1f;
   [SerializeField] private float chasingSpeed = 6f;
   [SerializeField] private float walkRadius = 50f;
   [SerializeField] private float investigationRadius = 30f;
   [SerializeField] private float leaveChaseRadius = 20f;
   [SerializeField] private float deathDistance = 2f;
   [SerializeField] private float timeBeingDeath = 15f;
   [SerializeField] private float deathTimer;
   [Space(10)]
   [SerializeField] private Vector3 currentTarget;
   [SerializeField] private LayerMask obstacleLayers;
   [SerializeField] private float distanceToPlayer;


   [Header("Behavior Tuning")]
   [SerializeField] private float maxInvestigationTime = 10f;
   [SerializeField] private float sightCheckInterval = 0.2f;
   [SerializeField] private float pathUpdateDelay = 2f;

   [Header("Sound System")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private float breathingSoundChance = 5f;

   private Transform player;
   public EnemyState currentState;
   private float sightCheckTimer;
   private float soundCheckTimer;

   [Header("X-Ray Vision")]
   public static bool seeEnemy = false;
   public Material xrayMaterial;

   [SerializeField] private Material[] originalMats;
   [SerializeField] private Renderer rend;

   [SerializeField] private Animator animator;

   void Awake()
   {
      originalMats = rend.materials;
   }

   void Start()
   {
      player = GameObject.FindGameObjectWithTag("Player").transform;
      ChangeCurrentState(EnemyState.Walking);

      agent.autoBraking = true;
      agent.stoppingDistance = 1f;

      animator = GetComponent<Animator>();
   }

   void SetSpeed(float speed)
   {
      agent.speed = speed * speedModifier;
      print($"[EnemySpeed] Current speed = {agent.speed}");

   }

   void SetTarget(bool? walk = null)
   {
      // print($"[SetTarget] Called with walk = {walk}");

      // if (walk == false || walk == null)
      // {
      //    currentTarget = player.position;
      //    print($"[SetTarget] New currentTarget: {currentTarget}");
      //    return;
      // }

      // for (int attempts = 0; attempts < 25; attempts++)
      // {
      //    Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
      //    randomDirection += player.position;
      //    randomDirection.y = player.position.y;

      //    if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
      //    {
      //       if (Vector3.Distance(transform.position, hit.position) > 5f)
      //       {
      //          currentTarget = hit.position;
      //          print($"[SetTarget] Found valid walking target: {currentTarget}");
      //          return;
      //       }
      //    }
      // }

      // Vector3 fallback = player.position + (Random.insideUnitSphere * 20f);
      // fallback.y = player.position.y;
      // if (NavMesh.SamplePosition(fallback, out NavMeshHit fallbackHit, 20f, NavMesh.AllAreas))
      // {
      //    currentTarget = fallbackHit.position;
      // }
      // else
      // {
      //    currentTarget = player.position; // último recurso
      // }

      // print($"[SetTarget] Using fallback target: {currentTarget}");

      currentTarget = player.position;
   }

   void Update()
   {
      if (GameManager.Instance.gameOver) { animator.SetTrigger("ToIdle"); return; }

      distanceToPlayer = Vector3
          .Distance(transform.position, player.position);

      print($"[EnemyController] Distance to player = {distanceToPlayer}");
      print($"[EnemyController] Death distance = {deathDistance}");

      if (distanceToPlayer < deathDistance && currentState != EnemyState.Dead)
      {
         print($"[EnemyController] Killing player");
         KillPlayer();
      }

      agent.SetDestination(player.position);

      HandleBreathingSounds();
      HandleCurrentState();

      if (animator == null) animator = GetComponent<Animator>();
      audioSource.volume = SoundManager.Instance.sfxVolume;

      if (seeEnemy && (!HasClearLineOfSight() || distanceToPlayer > 10)) SetXRay(true);
      else SetXRay(false);
   }

   void KillPlayer()
   {
      GameManager.Instance.PlayerDied();
      SoundManager.Instance.PlayJumpscareSound(audioSource);
   }

   void HandleCurrentState()
   {
      switch (currentState)
      {
         case EnemyState.Walking:
            HandleWalkingState();
            break;
         case EnemyState.Investigating:
            HandleInvestigationState();
            break;
         case EnemyState.Chasing:
            HandleChaseState();
            break;
         case EnemyState.Dead:
            HandleDeathState();
            break;

      }
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

   void HandleBreathingSounds()
   {
      if (currentState == EnemyState.Walking &&
          distanceToPlayer > walkRadius)
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

   bool ReachedTarget()
   {
      float distance = Vector3.Distance(transform.position, currentTarget);
      print($"[ReachedTarget] Distance to currentTarget: {distance}, currentTarget: {currentTarget}");
      return distance < 1f;
   }

   void HandleWalkingState()
   {
      SetSpeed(walkingSpeed);

      print($"[HandleWalking] distanceToPlayer = {distanceToPlayer}, investigationRadius = {investigationRadius}");

      if (distanceToPlayer <= investigationRadius)
      {
         print("[EnemyController] Changing from Walking into Investigating");
         ChangeCurrentState(EnemyState.Investigating);
         return;
      }

      if (ReachedTarget())
      {
         print("[EnemyController] Reached Target in Walking, creating new target position");
         SetTarget(true);
      }
      else
      {
         print($"[HandleWalking] NOT reached target. Distance to target: {Vector3.Distance(transform.position, currentTarget)}");
      }
   }

   void HandleInvestigationState()
   {
      SetSpeed(investigationSpeed);

      print($"[HandleInvestigation] distanceToPlayer = {distanceToPlayer}, walkRadius = {walkRadius}");
      print($"[HandleInvestigation] HasClearLineOfSight = {HasClearLineOfSight()}");
      print($"[HandleInvestigation] ReachedTarget = {ReachedTarget()}");

      if (HasClearLineOfSight())
      {
         print("[EnemyController] Has line of sight, changing to Chasing");
         ChangeCurrentState(EnemyState.Chasing);
         return;
      }

      if (distanceToPlayer >= investigationRadius)
      {
         print("[EnemyController] Player too far, changing back to Walking");
         ChangeCurrentState(EnemyState.Walking);
         return;
      }

      if (ReachedTarget() && distanceToPlayer > 2f)
      {
         print("[EnemyController] Reached target in Investigation, setting new target");
         SetTarget(false);
      }
   }

   void HandleChaseState()
   {
      currentTarget = player.position;
      if (distanceToPlayer > leaveChaseRadius)
      {
         ChangeCurrentState(EnemyState.Walking);
         return;
      }
   }

   public void Die()
   {
      ChangeCurrentState(EnemyState.Dead);
      deathTimer = Time.time;
   }

   void HandleDeathState()
   {
      agent.isStopped = true;
      if (Time.time - deathTimer > timeBeingDeath)
      {
         agent.isStopped = false;
         currentState = EnemyState.Walking;
         animator.SetTrigger("ToWalk");
         return;
      }
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

      if (!Physics.Raycast(transform.position,
                           direction.normalized,
                           out RaycastHit hit,
                           distance,
                           obstacleLayers))
      {
         return true;
      }

      return hit.collider.CompareTag("Player");
   }

   void ChangeCurrentState(EnemyState state)
   {
      currentState = state;

      switch (currentState)
      {
         case EnemyState.Walking:
            SetTarget(true);
            animator.SetTrigger("ToWalk");
            SetSpeed(walkingSpeed);
            print("[EnemyState] Changed state to Walking");
            break;
         case EnemyState.Investigating:
            SetTarget(false);
            SetSpeed(investigationSpeed);
            print("[EnemyState] Changed state to Investigating");
            break;
         case EnemyState.Chasing:
            SetTarget(false);
            animator.SetTrigger("ToChase");
            print("[EnemyState] Changed state to Chasing");
            SetSpeed(chasingSpeed);
            break;
         case EnemyState.Dead:
            SetTarget(false);
            animator.SetTrigger("ToDie");
            print("[EnemyState] Changed state to Dead");
            SetSpeed(0);
            break;
      }

   }

}