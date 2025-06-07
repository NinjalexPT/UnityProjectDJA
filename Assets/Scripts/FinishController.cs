using TMPro;
using UnityEngine;

public class FinishController : MonoBehaviour
{
   private BoxCollider boxCollider;
   private GameObject passTextObject;
   private TextMeshProUGUI passText;

   public bool hasKey = false;
   public bool canOpenDoor;

   void Awake()
   {
      GameManager.Instance.FetchCachedControllers();
   }

   void Start()
   {
      GameObject opening = GameObject.Find("Opening");
      boxCollider = opening.GetComponent<BoxCollider>();
      if (boxCollider == null)
      {
         Debug.LogError("BoxCollider component is missing on the FinishController GameObject.");
      }

      passTextObject = GameObject.Find("PassText");
      if (passText != null)
      {
         this.passText = passText.GetComponent<TextMeshProUGUI>();
         if (this.passText == null)
         {
            Debug.LogError("TextMeshProUGUI component is missing on the PassText GameObject.");
         }
         else
         {
            this.passText.text = $"To escape, you must find a key hidden in the machines. Once you do, come back.";
         }
      }
      boxCollider.enabled = true;
      passTextObject.SetActive(true);
   }

   void Update()
   {
      if (!canOpenDoor) return;

      if (Input.GetKey(KeyCode.E))
      {
         boxCollider.enabled = false;
         passTextObject.SetActive(false);
      }
   }

   void OnTriggerEnter(Collider other)
   {
      if (!(other.tag == "Player") || hasKey == false) return;

      GameManager.Instance.uIManager.ShowInteractText("Press E to use Mysterious Key");
      canOpenDoor = true;
   }

   void OnTriggerExit(Collider other)
   {
      if (!(other.tag == "Player") || hasKey == true) return;

      GameManager.Instance.uIManager.HideInteractText();
      canOpenDoor = false;
   }

   public void OpenDoor()
   {
      hasKey = true;
      SoundManager.Instance.PlayDoorSound();
      GameManager.Instance.uIManager.ShowKeyUI();
   }

}
