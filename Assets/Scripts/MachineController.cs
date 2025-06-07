using UnityEngine;

public class MachineController : MonoBehaviour
{
   private bool playerInside = false;

   [SerializeField]
   private int requiredCoins = 25;
   public static bool canDiscountCoins = true;
   [SerializeField]
   private float machineHeight;

   [SerializeField]
   private float initialRotationX;
   [SerializeField]
   private float initialRotationY;
   [SerializeField]
   private float initialRotationZ;

   public float MachineHeight => machineHeight;
   public int RequiredCoins => requiredCoins;
   public bool PlayerInside => playerInside;
   public Vector3 InitialRotation =>
       new Vector3(initialRotationX, initialRotationY, initialRotationZ);

   public virtual void OnTriggerEnter(Collider other)
   {
      if (other.tag == "Player")
      {
         if (GameManager.Instance.CoinCount() >= requiredCoins)
         {
            GameManager.Instance.uIManager.ShowInteractText("Press E to play (" + requiredCoins + " coins)");
            playerInside = true;
         }
         else
            GameManager.Instance.uIManager.ShowInteractText("You need at least (" + requiredCoins + " coins) coins to play.");
      }
   }

   public virtual void OnTriggerExit(Collider other)
   {
      if (other.tag == "Player")
      {
         playerInside = false;
         GameManager.Instance.uIManager.HideInteractText();
      }
   }

   void Awake()
   {
      machineHeight = transform.localPosition.y;
      initialRotationX = transform.localRotation.eulerAngles.x;
      initialRotationY = transform.localRotation.eulerAngles.y;
      initialRotationZ = transform.localRotation.eulerAngles.z;
   }

   public virtual void Update()
   {
      if (playerInside && Input.GetKeyDown(KeyCode.E))
      {
         OnUseMachine();
      }
   }
   public virtual void OnUseMachine()
   {
      if (MachineController.canDiscountCoins == false)
         return;
      GameManager.Instance.AddCoins(-requiredCoins);
   }
}