using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
   public static UIManager Instance;

   [SerializeField] private TextMeshProUGUI coinCountText;
   [SerializeField] private GameObject coinObject;
   [SerializeField] private TextMeshProUGUI interactText;
   [SerializeField] private GameObject deathScreen;
   [SerializeField] private GameObject playerUI;
   [SerializeField] private GameObject pauseScreen;

   void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
      }
      else
      {
         Destroy(gameObject);
         return;
      }
   }

   void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape))
      {
         if (pauseScreen.activeSelf)
         {
            HidePauseScreen();
         }
         else
         {
            ShowPauseScreen();
         }
      }
   }

   public void ShowPauseScreen()
   {
      FindFirstObjectByType<FirstPersonController>().cameraCanMove = false;
      pauseScreen.SetActive(true);
      Time.timeScale = 0f;
   }
   public void HidePauseScreen()
   {
      FindFirstObjectByType<FirstPersonController>().cameraCanMove = true;
      Time.timeScale = 1f;
      pauseScreen.SetActive(false);
   }

   public void UpdateCoinCount(int count)
   {
      if (coinCountText != null)
         coinCountText.text = count.ToString();
      if (coinObject != null)
         coinObject.SetActive(count > 0);
   }

   public void ShowInteractText(string text)
   {
      if (interactText == null) return;
      interactText.text = text;
      interactText.rectTransform.anchoredPosition =
        new Vector3(100f, -20f, 0f);
   }

   public void HideInteractText()
   {
      if (interactText == null) return;
      interactText.rectTransform.anchoredPosition =
        new Vector3(100f, 60f, 0f);
   }

   public void ShowDeathScreen()
   {
      if (deathScreen != null) deathScreen.SetActive(true);
   }

   public void HideDeathScreen()
   {
      if (deathScreen != null) deathScreen.SetActive(false);
   }

   public void SetPlayerUIActive(bool active)
   {
      if (playerUI != null) playerUI.SetActive(active);
   }

   public void SetCoinObjectActive(bool active)
   {
      if (coinObject != null) coinObject.SetActive(active);
   }
}