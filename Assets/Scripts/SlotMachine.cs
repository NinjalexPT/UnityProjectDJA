using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class SlotMachine : MachineController
{
   [Header("Main parts of the machine")]
   [SerializeField]
   private GameObject[] mainCylinders = new GameObject[3];
   [SerializeField] private float spinDuration = 3f;
   [SerializeField] private float spinSpeed = 720f;

   private Quaternion[] initialRotations;
   private bool isSuccess = false;

   void Start()
   {
      initialRotations = new Quaternion[mainCylinders.Length];
      for (int i = 0; i < mainCylinders.Length; i++)
      {
         initialRotations[i] = mainCylinders[i].transform.localRotation;
      }
   }

   public override void OnUseMachine()
   {
      if (GameManager.Instance.CoinCount() < RequiredCoins) return;
      if (!canDiscountCoins) return;
      GameManager.Instance.AddCoins(-RequiredCoins);
      canDiscountCoins = false;
      PullLever();
   }

   public void PullLever()
   {
      isSuccess = UnityEngine.Random.Range(0f, 1f) > 0.25f; // 75% chance of success
      print("Pulling lever: " + (isSuccess ? "Success" : "Failure"));
      StartCoroutine(SpinAnimation());
   }

   IEnumerator SpinAnimation()
   {
      float timer = 0f;

      while (timer < spinDuration)
      {
         foreach (GameObject cylinder in mainCylinders)
         {
            cylinder.transform.Rotate(spinSpeed * Time.deltaTime, 0, 0);
         }

         timer += Time.deltaTime;
         yield return null;
      }

      bool openFinalDoor = UnityEngine.Random.value > 1 - (float)(GameManager.Instance.chanceForKey / 100); // 5% chance of opening final door

      if (openFinalDoor && !FinishController.Instance.hasKey) { FinishController.Instance.OpenDoor(); yield return null; }


      int PowerUpTypeIndex = UnityEngine.Random.Range(0, Enum.GetValues(typeof(PowerUpType)).Length);
      for (int i = 0; i < mainCylinders.Length; i++)
      {
         if (isSuccess)
         {
            initialRotations[i] = Quaternion.Euler(initialRotations[i].x + 360 / 12 * PowerUpTypeIndex, initialRotations[i].y, initialRotations[i].z);
            mainCylinders[i].transform.localRotation = initialRotations[i];
         }
         else
         {
            initialRotations[i] = Quaternion.Euler((float)(initialRotations[i].x + 360 / 12 * Math.Floor(UnityEngine.Random.Range(0.0f, 12.0f))), initialRotations[i].y, initialRotations[i].z);
            mainCylinders[i].transform.localRotation = initialRotations[i];
         }
      }
      if (isSuccess) PowerUpManager.Instance.ActivateRandomPowerUp();
      else PowerUpManager.Instance.ActivateRandomDebuff();
      MachineController.canDiscountCoins = true;
   }
}