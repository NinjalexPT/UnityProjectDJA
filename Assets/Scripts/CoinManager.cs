using System.Collections;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
   [SerializeField] private AudioClip coinSound;
   [SerializeField] private int coinValue = 1;
   [SerializeField] private float rotationSpeed = 100f;
   [SerializeField] private float bobbingSpeed = 2f;
   [SerializeField] private float bobbingHeight = 0.2f;

   private AudioSource audioSource;
   private MeshRenderer meshRenderer;
   private Collider coinCollider;
   private Vector3 initialPosition;

   public static int valueModifier = 1;

   private bool isCollected = false;

   private void Start()
   {
      audioSource = GetComponent<AudioSource>();
      if (audioSource == null)
      {
         audioSource = gameObject.AddComponent<AudioSource>();
      }

      if (coinSound == null)
      {
         coinSound = Resources.Load<AudioClip>("Sounds/Coin");
         if (coinSound == null)
         {
            Debug.LogError("Coin sound not found in Resources/Sounds/Coin!");
         }
      }

      meshRenderer = GetComponent<MeshRenderer>();
      coinCollider = GetComponent<Collider>();

      if (meshRenderer == null)
      {
         Debug.LogError("MeshRenderer not found on this Coin!");
      }
      if (coinCollider == null)
      {
         Debug.LogError("Collider not found on this Coin!");
      }

      initialPosition = transform.position;
   }

   private void OnTriggerEnter(Collider other)
   {
      if (isCollected || !other.CompareTag("Player"))
      {
         return;
      }

      isCollected = true;

      GameManager.Instance.AddCoins(this.coinValue * valueModifier);
      Debug.Log("Coin Collected! It will reappear soon.");

      if (coinSound != null && audioSource != null)
      {
         audioSource.volume = SoundManager.Instance.sfxVolume;
         audioSource.PlayOneShot(coinSound);
      }

      if (meshRenderer != null)
      {
         meshRenderer.enabled = false;
      }
      if (coinCollider != null)
      {
         coinCollider.enabled = false;
      }

      StartCoroutine(RespawnCoin());
   }

   private IEnumerator RespawnCoin()
   {
      yield return new WaitForSeconds(GameManager.Instance.coinReappearingTimer);

      Debug.Log($"Coin at {transform.position} is respawning!");

      if (meshRenderer != null)
      {
         meshRenderer.enabled = true;
      }
      if (coinCollider != null)
      {
         coinCollider.enabled = true;
      }

      isCollected = false;
   }

   private void Update()
   {
      if (!isCollected)
      {
         transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime);
         float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
         transform.position = initialPosition + new Vector3(0f, yOffset, 0f);
      }
   }
}