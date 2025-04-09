using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] AudioClip coinSound; //Assign the coin sound in the inspector
    [SerializeField] int coinValue = 1; //The value of the coin

    private AudioSource audioSource; //Assign the AudioSource in the inspector


    private void Start()
    {
        //Check if the AudioSource is assigned
        audioSource = GetComponent<AudioSource>();

        //Check if the AudioClip is assigned
        if (coinSound == null) 
        {
            Debug.Log("Coin sound not assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            Debug.Log("Coin Collected!");
            //Play sound effect
            if (coinSound != null) 
            {
                audioSource.PlayOneShot(coinSound);
            }
            Destroy(gameObject, coinSound.length);
        }
    }

    private void Update()
    {
        //rotate the coin
        transform.Rotate(Vector3.forward, 100 * Time.deltaTime);
    }

}
