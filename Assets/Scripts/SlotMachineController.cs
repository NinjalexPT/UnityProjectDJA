using UnityEngine;

public class SlotMachineController : MonoBehaviour
{
  private bool playerInside = false;

  void OnTriggerEnter(Collider other)
  {
    if (other.tag == "Player")
    {
      playerInside = true;
      GameManager.Instance.ShowInteractText();
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

  void Update()
  {
    if (playerInside && Input.GetKeyDown(KeyCode.E))
    {
      Debug.Log("interagiu");

      // idrk o que querem fazer mas quanod o jogar esta perto e clica no E esta função é chamada.
      // do wahtever you like
    }
  }
}