using System.Collections;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
  [SerializeField] AudioClip coinSound; //Assign the coin sound in the inspector
  [SerializeField] int coinValue = 1; //The value of the coin
  [SerializeField] float rotationSpeed = 100f; // Speed of rotation
  [SerializeField] float bobbingSpeed = 2f; // Speed of the up and down movement
  [SerializeField] float bobbingHeight = 0.2f; // Height of the up and down movement

  private AudioSource audioSource; //Assign the AudioSource in the inspector
  private MeshRenderer meshRenderer;
  private Collider coinCollider;
  private Vector3 initialPosition; // Store the initial position of the coin

  private void Start()
  {
    //Check if the AudioSource is assigned
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
      // Add an AudioSource component if one doesn't exist
      audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Load the coin sound only once in Start
    if (coinSound == null)
    {
      coinSound = Resources.Load<AudioClip>("Sounds/Coin");
      //Check if the AudioClip is assigned
      if (coinSound == null)
      {
        Debug.LogError("Coin sound not found in Resources/Sounds/Coin!");
      }
    }

    // Get the MeshRenderer and Collider components
    meshRenderer = GetComponent<MeshRenderer>();
    coinCollider = GetComponent<Collider>();

    // Ensure these components exist
    if (meshRenderer == null)
    {
      Debug.LogError("MeshRenderer not found on this Coin!");
    }
    if (coinCollider == null)
    {
      Debug.LogError("Collider not found on this Coin!");
    }

    // Store the initial position of the coin
    initialPosition = transform.position;
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {

      GameManager.Instance.AddCoins(this.coinValue);

      Debug.Log("Coin Collected!");
      //Play sound effect
      if (coinSound != null && audioSource != null)
      {
        audioSource.volume = 0.5f;
        audioSource.PlayOneShot(coinSound);

        // Disable MeshRenderer and Collider
        if (meshRenderer != null)
        {
          meshRenderer.enabled = false;
        }
        if (coinCollider != null)
        {
          coinCollider.enabled = false;
        }

        // Destroy the coin after the sound finishes playing
        StartCoroutine(DestroyAfterSound());
      }
      else
      {
        // Handle the case where the sound or audio source is not properly set
        Destroy(this.gameObject);
      }
    }
  }

  private IEnumerator DestroyAfterSound()
  {
    // Wait for the duration of the sound clip
    if (coinSound != null)
    {
      yield return new WaitForSeconds(coinSound.length);
    }
    else
    {
      // If there's no sound, destroy immediately to avoid issues
      yield break;
    }

    // Destroy the coin GameObject
    Destroy(this.gameObject);
  }

  private void Update()
  {
    //rotate the coin
    transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime);

    //make the coin go up and down
    float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
    transform.position = initialPosition + new Vector3(0f, yOffset, 0f);
  }
}