using UnityEngine;

public class MachineController : MonoBehaviour
{
   private bool playerInside = false;

   [SerializeField]
   private int requiredCoins = 25;

   [SerializeField]
   private float machineHeight;

   [SerializeField]
   private float initialRotationX;
   [SerializeField]
   private float initialRotationY;
   [SerializeField]
   private float initialRotationZ;

   public float MachineHeight => machineHeight;
   public Vector3 InitialRotation =>
       new Vector3(initialRotationX, initialRotationY, initialRotationZ);

   void OnTriggerEnter(Collider other)
   {
      if (other.tag == "Player")
      {
         if (GameManager.Instance.CoinCount() >= requiredCoins)
         {
            GameManager.Instance.ShowInteractText("Press E to play (" + requiredCoins + " coins)");
            playerInside = true;
         }
         else
            GameManager.Instance.ShowInteractText("You need at least (" + requiredCoins + " coins) coins to play.");
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

   void Awake()
   {
      machineHeight = transform.localPosition.y;
      initialRotationX = transform.localRotation.eulerAngles.x;
      initialRotationY = transform.localRotation.eulerAngles.y;
      initialRotationZ = transform.localRotation.eulerAngles.z;
   }

   void Update()
   {
      if (playerInside && Input.GetKeyDown(KeyCode.E))
      {
         OnUseMachine();
      }
   }
   public virtual void OnUseMachine()
   {
      GameManager.Instance.AddCoins(-requiredCoins);
   }
}