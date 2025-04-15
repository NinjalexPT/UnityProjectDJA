using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
  public enum EnemyState
  {
    Walking,
    Investigating,
    Chasing
  }

  [Header("Navigation Settings")]
  [SerializeField] private NavMeshAgent agent;
  [SerializeField] private float walkingSpeed = 3f;
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

  void Start()
  {
    player = GameObject.FindGameObjectWithTag("Player").transform;
    currentState = EnemyState.Walking;
    agent.speed = walkingSpeed;
    agent.autoBraking = true; // Garantir que freia ao chegar perto
    agent.stoppingDistance = 1f; // Distância de parada
    SetNewWalkTarget();
  }

  void Update()
  {
    if (GameManager.Instance.gameOver) return;

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    HandleDeathCondition(distanceToPlayer);
    UpdateStateMachine(distanceToPlayer);
    HandleBreathingSounds(distanceToPlayer);
  }

  void HandleBreathingSounds(float distance)
  {
    if (currentState == EnemyState.Walking && distance > detectionRadius)
    {
      soundCheckTimer += Time.deltaTime;
      if (soundCheckTimer > 1f)
      {
        soundCheckTimer = 0;
        if (Random.Range(0, 100) < breathingSoundChance)
        {
          SoundManager.Instance.PlayBreathingSound(audioSource);
        }
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
    }
  }

  void HandleWalkingState(float distanceToPlayer)
  {
    // Atualizado para priorizar verificação de visão
    if (distanceToPlayer <= detectionRadius)
    {
      if (HasClearLineOfSight())
      {
        StartChasing();
        return; // Sai imediatamente após iniciar perseguição
      }
      else if (distanceToPlayer < detectionRadius * 0.5f) // Se estiver muito perto
      {
        StartInvestigating(player.position);
      }
    }

    // Mantém a lógica de caminhada somente se não estiver perseguindo
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

    // Verificação prioritária de linha de visada
    if (HasClearLineOfSight())
    {
      StartChasing();
      return;
    }

    // Atualização mais inteligente do destino
    if (distanceToPlayer < Vector3.Distance(transform.position, investigationTarget) * 0.8f)
    {
      investigationTarget = player.position;
      agent.SetDestination(investigationTarget);
    }

    // Condição de saída melhorada
    if (agent.remainingDistance <= agent.stoppingDistance || investigationTimer <= 0 || distanceToPlayer > detectionRadius)
    {
      ReturnToWalking();
    }
  }

  void HandleChaseState(float distanceToPlayer)
  {
    agent.SetDestination(player.position);

    if (distanceToPlayer > detectionRadius)
    {
      ReturnToWalking();
    }
  }

  bool HasClearLineOfSight()
  {
    sightCheckTimer += Time.deltaTime;
    if (sightCheckTimer < sightCheckInterval) return false;
    sightCheckTimer = 0;

    Vector3 direction = player.position - transform.position;
    float distance = Vector3.Distance(transform.position, player.position);

    // Ajuste crucial: usar ~obstacleLayers para ignorar camadas bloqueadoras
    if (!Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, distance, obstacleLayers))
    {
      return true;
    }

    // Debug visual
    Debug.DrawRay(transform.position, direction, Color.red, 0.5f);
    return hit.collider.CompareTag("Player");
  }

  void StartChasing()
  {
    currentState = EnemyState.Chasing;
    agent.speed = chasingSpeed;
    agent.SetDestination(player.position);
  }

  void StartInvestigating(Vector3 position)
  {
    currentState = EnemyState.Investigating;
    investigationTarget = position;
    agent.SetDestination(position);
    investigationTimer = maxInvestigationTime;
  }

  void ReturnToWalking()
  {
    currentState = EnemyState.Walking;
    agent.speed = walkingSpeed;
    SetNewWalkTarget();
  }

  void SetNewWalkTarget()
  {
    Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
    randomDirection += player.position;
    randomDirection.y = player.position.y;

    int attempts = 0;
    bool validPath = false;

    // Tenta até 5 vezes encontrar um caminho válido
    while (attempts < 5 && !validPath)
    {
      if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
      {
        currentWalkTarget = hit.position;
        agent.SetDestination(currentWalkTarget);

        // Espera o cálculo do caminho
        if (agent.pathPending) return;

        validPath = agent.pathStatus == NavMeshPathStatus.PathComplete;
      }
      attempts++;
    }

    if (!validPath)
    {
      Debug.LogWarning("Falha ao encontrar caminho válido");
    }
  }

  void HandleDeathCondition(float distance)
  {
    if (distance < deathDistance)
    {
      SoundManager.Instance.PlayJumpscareSound(audioSource);
      GameManager.Instance.PlayerDied();
      agent.isStopped = true;
    }
  }
}