using UnityEngine;

public class SlotMachineController : MonoBehaviour
{
  private bool playerInside = false;

  [SerializeField]
  private PrototypeMachine prototype;

  void OnTriggerEnter(Collider other)
  {
    if (other.tag == "Player")
    {
      if (GameManager.Instance.CoinCount() >= 25)
      {
        GameManager.Instance.ShowInteractText("Press E to play (25 coins)");
        playerInside = true;
      }
      else
        GameManager.Instance.ShowInteractText("You need at least 25 coins to play.");
    }
  }

  void OnTriggerExit(Collider other)
  {
    if (other.tag == "Player")
    {
      playerInside = false;
      GameManager.Instance.HideInteractText();
    }
  }

  void Start()
  {
    prototype = this.GetComponent<PrototypeMachine>();
  }

  void Update()
  {
    if (playerInside && Input.GetKeyDown(KeyCode.E))
    {
      Debug.Log("interagiu");
      GameManager.Instance.AddCoins(-25);
      prototype.PullLever();
    }
  }
}