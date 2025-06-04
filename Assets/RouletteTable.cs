using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RouletteTable : MachineController
{
   [SerializeField] private GameObject rouletteWheel;
   [SerializeField] private float spinDuration = 3f;
   [SerializeField] private float spinSpeed = 720f;

   private Quaternion originalRotation;
   private bool showingChooseColor;
   private bool hasChargedCoins;

   void Start()
   {
      // guarda a rotação “de repouso” da roda
      originalRotation = rouletteWheel.transform.localRotation;
   }

   public override void Update()
   {

      if (!showingChooseColor)
         base.Update();

      if (showingChooseColor)
      {
         UIManager.Instance.ShowInteractText(
           "Choose a color: R = Red, G = Green, B = Black");

         if (Input.GetKeyDown(KeyCode.R)) ChooseColor(0);
         if (Input.GetKeyDown(KeyCode.G)) ChooseColor(1);
         if (Input.GetKeyDown(KeyCode.B)) ChooseColor(2);
      }
   }


   public override void OnUseMachine()
   {
      if (GameManager.Instance.gameOver) return;
      if (GameManager.Instance.CoinCount() < RequiredCoins) return;
      if (showingChooseColor || hasChargedCoins) return;

      showingChooseColor = true;

      MachineController.canDiscountCoins = false;

      GameManager.Instance.AddCoins(-RequiredCoins);

      UIManager.Instance.ShowInteractText(
        "Choose a color: R = Red, G = Green, B = Black");
   }

   private void ChooseColor(int color)
   {
      UIManager.Instance.HideInteractText();
      showingChooseColor = false;
      StartSpin(color);
   }

   public void StartSpin(int color)
   {
      StartCoroutine(SpinAnimation(color));
   }

   private IEnumerator SpinAnimation(int color)
   {
      float timer = 0f;
      while (timer < spinDuration)
      {
         rouletteWheel.transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
         timer += Time.deltaTime;
         yield return null;
      }

      bool success = Random.value > (color == 0 ? 18 / 37f : (color == 1 ? 18 / 37f : 1 / 37f));
      float finalZ = 0f;

      print("Roulette spin result: " + (success ? "Success" : "Failure"));

      if (success)
      {
         switch (color)
         {
            case 0: finalZ = 0f; break;
            case 1: finalZ = (360f / 37f) * 28f; break;
            case 2: finalZ = (360f / 37f) * 1f; break;
         }

         PowerUpManager.Instance.ActivateRandomPowerUp();

      }
      else
      {
         int randomBin = Random.Range(0, 12);
         finalZ = (360f / 12f) * randomBin;

         PowerUpManager.Instance.ActivateRandomDebuff();
      }

      rouletteWheel.transform.localRotation =
        Quaternion.Euler(
          originalRotation.eulerAngles.x,
          originalRotation.eulerAngles.y,
          finalZ
        );

      hasChargedCoins = false;
      UIManager.Instance.ShowInteractText(
        $"Press E to play ({RequiredCoins} coins)");

      MachineController.canDiscountCoins = true;
   }

   // se o jogador sair do “gatilho”, escondemos texto e resetamos
   public override void OnTriggerExit(Collider other)
   {
      base.OnTriggerExit(other);
      showingChooseColor = false;
      hasChargedCoins = false;
   }
}