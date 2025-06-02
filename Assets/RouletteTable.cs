using System;
using System.Collections;
using UnityEngine;

public class RouletteTable : MachineController
{
   [Header("Main parts of the machine")]
   [SerializeField]
   private GameObject rouletteWheel;

   [SerializeField] private float spinDuration = 3f;
   [SerializeField] private float spinSpeed = 720f;

   private Quaternion initialRotation;

   void Start()
   {
      initialRotation = rouletteWheel.transform.localRotation;
   }

   public override void Update()
   {
      if (playerInside && Input.GetKeyDown(KeyCode.E))
      {
         OnUseMachine();
      }
   }

   public override void OnUseMachine()
   {
      SpinWheel();
   }

   public void SpinWheel()
   {
      print("Spinning the roulette wheel");
      StartCoroutine(SpinAnimation());
   }

   IEnumerator SpinAnimation()
   {
      float timer = 0f;

      while (timer < spinDuration)
      {

         rouletteWheel.transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

         timer += Time.deltaTime;
         yield return null;
      }

      int PowerUpTypeIndex = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PowerUpType)).Length);

      bool isSuccess = UnityEngine.Random.Range(0f, 1f) > 0.25f;

      print("Spin result: " + (isSuccess ? "Success" : "Failure"));

      if (isSuccess)
      {
         // Set the wheel to the success position
         initialRotation = Quaternion.Euler(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y, initialRotation.eulerAngles.z + 360 / 36 * PowerUpTypeIndex);
         rouletteWheel.transform.localRotation = initialRotation;
      }
      else
      {
         // Set the wheel to a random failure position
         initialRotation = Quaternion.Euler(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y, (float)(initialRotation.eulerAngles.z - (360 / 12) + 360 / 12 * Math.Floor(UnityEngine.Random.Range(0.0f, 12.0f) * 2)));
         rouletteWheel.transform.localRotation = initialRotation;
      }
   }

}
