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
      // 1) só permitimos entrar em OnUseMachine (e descontar moedas)
      //    enquanto não estivermos em “choose color”
      if (!showingChooseColor)
         base.Update();

      // 2) se já estivermos em “choose color”, mostramos o prompt
      if (showingChooseColor)
      {
         GameManager.Instance.ShowInteractText(
           "Choose a color: R = Red, G = Green, B = Black");

         if (Input.GetKeyDown(KeyCode.R)) ChooseColor(0);
         if (Input.GetKeyDown(KeyCode.G)) ChooseColor(1);
         if (Input.GetKeyDown(KeyCode.B)) ChooseColor(2);
      }
   }

   // quando o jogador carrega em E
   public override void OnUseMachine()
   {
      // impede duplo desconto
      if (showingChooseColor || hasChargedCoins) return;

      showingChooseColor = true;

      MachineController.canDiscountCoins = false;

      GameManager.Instance.AddCoins(-RequiredCoins);

      // imediatamente substitui o texto de “Press E” por “Choose a color”
      GameManager.Instance.ShowInteractText(
        "Choose a color: R = Red, G = Green, B = Black");
   }

   private void ChooseColor(int color)
   {
      // ao escolher cor:
      //  a) escondemos o texto
      //  b) saímos do modo escolher cor
      //  c) começamos o spin
      GameManager.Instance.HideInteractText();
      showingChooseColor = false;
      StartSpin(color);
   }

   public void StartSpin(int color)
   {
      StartCoroutine(SpinAnimation(color));
   }

   private IEnumerator SpinAnimation(int color)
   {
      // parte de animação livre
      float timer = 0f;
      while (timer < spinDuration)
      {
         rouletteWheel.transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
         timer += Time.deltaTime;
         yield return null;
      }

      // decide finalZ com floats (nunca 360/37 inteiro)
      bool success = Random.value > 0.25f;
      float finalZ = 0f;

      if (success)
      {
         switch (color)
         {
            case 0: finalZ = 0f; break;
            case 1: finalZ = (360f / 37f) * 28f; break;
            case 2: finalZ = (360f / 37f) * 1f; break;
         }
      }
      else
      {
         int randomBin = Random.Range(0, 12);
         finalZ = (360f / 12f) * randomBin;
      }

      // aplica ROTATION ABSOLUTA sobre a rotação original
      rouletteWheel.transform.localRotation =
        Quaternion.Euler(
          originalRotation.eulerAngles.x,
          originalRotation.eulerAngles.y,
          finalZ
        );

      // 3) spin acabou, voltamos a mostrar “Press E to play…”
      //    e limpamos flags para permitir novo jogo
      hasChargedCoins = false;
      GameManager.Instance.ShowInteractText(
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