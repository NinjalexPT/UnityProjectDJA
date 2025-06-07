using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
   public static SoundManager Instance;

   [Header("Audio Sources")]
   public AudioSource musicSource;
   public AudioSource sfxSource;
   public AudioSource movementSource;
   public AudioSource heartbeatSource;
   public AudioSource chaseSource;
   public AudioSource doorSource;

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
   [Range(0.5f, 1.5f)] public float minHeartbeatPitch = 0.5f;
   [Range(1.6f, 3f)] public float maxHeartbeatPitch = 2.0f;

   [Header("Monster")]
   [SerializeField] private AudioClip jumpscareSound;
   [SerializeField] private AudioClip breathingSound;

   [Header("Machines/Power Ups")]
   public AudioClip winSound;
   public AudioClip loseSound;
   public AudioClip gunSound;

   [Header("Story")]
   [SerializeField] private AudioClip whereAmIClip;
   [SerializeField] private AudioClip doorSound;

   [Header("Music")]
   [SerializeField] private AudioClip musicClip;

   private Transform player;
   private AudioClip currentMovementSound;
   private bool wasChasing;
   private float cooldownTimer;
   public bool isChaseSoundFinishing;

   public float musicVolume = 100f;
   public float sfxVolume = 100f;

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
      sfxSource.volume = 0.5f;
      heartbeatSource.volume = 0.3f * sfxSource.volume;
      movementSource.volume = 0.7f * sfxSource.volume;
      chaseSource.volume = 0.3f * sfxSource.volume;
   }

   private void Update()
   {

      if (musicSource == null || sfxSource == null ||
            movementSource == null || heartbeatSource == null || chaseSource == null || doorSource == null)
      {
         GetSources();
         PlayWhereAmI();
         FindPlayer();
         return;
      }

      if (GameManager.Instance.gameOver)
      {
         StopAllSounds();
         return;
      }

      movementSource.volume = sfxVolume;
      heartbeatSource.volume = sfxVolume;
      chaseSource.volume = sfxVolume;
      sfxSource.volume = sfxVolume;
      musicSource.volume = musicVolume;

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

   public void PauseAllSound()
   {
      movementSource.Pause();
      heartbeatSource.Pause();
      chaseSource.Pause();
      sfxSource.Pause();
      musicSource.Pause();
   }

   public void ResumeAllSounds()
   {
      movementSource.UnPause();
      heartbeatSource.UnPause();
      chaseSource.UnPause();
      sfxSource.UnPause();
      musicSource.UnPause();
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
      if (isChaseSoundFinishing && !chaseSource.isPlaying && !GameManager.Instance.restarting)
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

   public void InitializeHeartbeat()
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

   public void PlayWhereAmI()
   {
      sfxSource.PlayOneShot(whereAmIClip);
      print("where m i should have played");
   }

   public void PlayBreathingSound(AudioSource monsterAudioSource)
   {
      monsterAudioSource.PlayOneShot(breathingSound);
   }

   internal void PlayJumpscareSound(AudioSource monsterAudioSource)
   {
      monsterAudioSource.PlayOneShot(jumpscareSound);
   }

   public void PlayDoorSound()
   {
      doorSource.PlayOneShot(doorSound);
   }

   void GetSources()
   {
      if (SceneManager.GetActiveScene().name == "Maingame")
      {
         musicSource = GameObject.Find("Music")?.GetComponent<AudioSource>();
         if (musicSource != null)
         {
            musicSource.volume = musicVolume;
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
         }
         else
         {
            Debug.LogWarning("Music Source not found");
         }

         sfxSource = GameObject.Find("SFX")?.GetComponent<AudioSource>();
         if (sfxSource != null)
         {
            sfxSource.volume = sfxVolume;
         }
         else
         {
            Debug.LogWarning("SFX Source not found");
         }

         movementSource = GameObject.Find("Movement")?.GetComponent<AudioSource>();
         if (movementSource != null)
         {
            movementSource.volume = sfxVolume;
         }
         else
         {
            Debug.LogWarning("Movement Source not found");
         }

         heartbeatSource = GameObject.Find("Heartbeat")?.GetComponent<AudioSource>();
         if (heartbeatSource != null)
         {
            heartbeatSource.volume = sfxVolume;
            heartbeatSource.loop = true;
            InitializeHeartbeat();
         }
         else
         {
            Debug.LogWarning("Heartbeat Source not found");
         }

         chaseSource = GameObject.Find("Chasing")?.GetComponent<AudioSource>();
         if (chaseSource != null)
         {
            chaseSource.volume = sfxVolume;
         }
         else
         {
            Debug.LogWarning("Chase Source not found");
         }

         doorSource = FindFirstObjectByType<FinishController>()?.GetComponent<AudioSource>();
         if (doorSource != null)
         {
            doorSource.volume = sfxVolume;
         }
         else
         {
            Debug.LogWarning("Chase Source not found");
         }
      }
   }
}