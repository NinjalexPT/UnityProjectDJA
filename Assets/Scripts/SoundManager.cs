using UnityEngine;
using System.Linq;
using System;

public class SoundManager : MonoBehaviour
{
  public static SoundManager Instance;

  [Header("Audio Sources")]
  [SerializeField] private AudioSource musicSource;
  [SerializeField] private AudioSource sfxSource;
  [SerializeField] private AudioSource movementSource;
  [SerializeField] private AudioSource heartbeatSource;
  [SerializeField] private AudioSource chaseSource;

  [Header("Movement Sounds")]
  public AudioClip walkingSound;
  public AudioClip runningSound;

  [Header("Chasing Sounds")]
  public AudioClip chasingSound;
  public AudioClip escapedSound;
  [Range(0, 5)] public float escapeCooldown = 3f;

  [Header("Heartbeat Settings")]
  public AudioClip heartbeatSound;
  public float maxHeartbeatDistance = 15f;
  [Range(0.5f, 1.5f)] public float minHeartbeatPitch = 0.8f;
  [Range(1.6f, 3f)] public float maxHeartbeatPitch = 2.5f;

  [Header("Monster")]
  [SerializeField] private AudioClip jumpscareSound;
  [SerializeField] private AudioClip breathingSound;

  private Transform player;
  private AudioClip currentMovementSound;
  private bool wasChasing;
  private float cooldownTimer;
  private bool isChaseSoundFinishing;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }

  private void Start()
  {
    InitializeHeartbeat();
    FindPlayer();
    heartbeatSource.volume = 0.3f;
    movementSource.volume = 0.7f;
    chaseSource.volume = 0.3f;
    sfxSource.volume = 0.5f;
  }

  private void Update()
  {
    if (GameManager.Instance.gameOver)
    {
      StopAllSounds();
      return;
    }

    UpdateHeartbeat();
    UpdateChaseState();
    UpdateSoundCompletion();
    UpdateCooldown();
  }

  void StopAllSounds()
  {
    movementSource.Stop();
    heartbeatSource.Stop();
    chaseSource.Stop();
    sfxSource.Stop();
  }

  void FindPlayer()
  {
    player = GameObject.FindWithTag("Player")?.transform;
    if (player == null)
    {
      Debug.LogWarning("Player not found! Make sure your player has the 'Player' tag.");
    }
  }

  void UpdateChaseState()
  {
    bool isChasing = CheckForChasingEnemies();

    if (isChasing != wasChasing)
    {
      if (isChasing)
      {
        StartChase();
      }
      else
      {
        EndChase();
      }
      wasChasing = isChasing;
    }
  }

  bool CheckForChasingEnemies()
  {
    return GameObject.FindGameObjectsWithTag("Enemy")
        .Select(enemy => enemy.GetComponent<EnemyController>())
        .Any(controller => controller != null &&
             controller.currentState == EnemyController.EnemyState.Chasing);
  }

  void StartChase()
  {
    Debug.Log("Starting chase sequence");
    isChaseSoundFinishing = false;
    cooldownTimer = 0f;
    chaseSource.Stop();
    chaseSource.clip = chasingSound;
    chaseSource.loop = true;
    chaseSource.Play();
  }

  void EndChase()
  {
    Debug.Log("Ending chase - letting sound finish");
    chaseSource.loop = false; // Stop looping but let sound finish
    isChaseSoundFinishing = true;
  }

  void UpdateSoundCompletion()
  {
    if (isChaseSoundFinishing && !chaseSource.isPlaying)
    {
      Debug.Log("Chase sound finished, starting cooldown");
      isChaseSoundFinishing = false;
      cooldownTimer = escapeCooldown;
    }
  }

  void UpdateCooldown()
  {
    if (cooldownTimer > 0)
    {
      cooldownTimer -= Time.deltaTime;

      if (CheckForChasingEnemies())
      {
        Debug.Log("Chase resumed during cooldown");
        cooldownTimer = 0f;
        StartChase();
        return;
      }

      if (cooldownTimer <= 0)
      {
        Debug.Log("Playing escape sound");
        sfxSource.PlayOneShot(escapedSound);
        cooldownTimer = 0f;
      }
    }
  }

  void InitializeHeartbeat()
  {
    heartbeatSource.clip = heartbeatSound;
    heartbeatSource.loop = true;
    heartbeatSource.Play();
  }

  void UpdateHeartbeat()
  {
    if (player == null) return;

    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    float closestDistance = enemies.Any() ?
        enemies.Min(e => Vector3.Distance(player.position, e.transform.position)) :
        maxHeartbeatDistance;

    float normalizedDistance = Mathf.Clamp01(1 - (closestDistance / maxHeartbeatDistance));
    heartbeatSource.pitch = Mathf.Lerp(minHeartbeatPitch, maxHeartbeatPitch, normalizedDistance);
    heartbeatSource.volume = closestDistance > 30 ? 0 : Mathf.Lerp(0.3f, 1f, normalizedDistance) / 3;
  }

  public void PlayMovementSound(AudioClip clip)
  {
    if (movementSource.isPlaying && currentMovementSound == clip) return;

    currentMovementSound = clip;
    movementSource.clip = clip;
    movementSource.loop = true;
    movementSource.Play();
  }

  public void StopMovementSound()
  {
    movementSource.Pause();
    currentMovementSound = null;
  }

  public void PlayWalking() => PlayMovementSound(walkingSound);
  public void PlayRunning() => PlayMovementSound(runningSound);

  public void PlayBreathingSound(AudioSource monsterAudioSource)
  {
    monsterAudioSource.PlayOneShot(breathingSound);
  }

  internal void PlayJumpscareSound(AudioSource monsterAudioSource)
  {
    monsterAudioSource.PlayOneShot(jumpscareSound);
  }
}