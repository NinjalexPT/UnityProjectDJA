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

   [Header("Power Up UI")]
   [SerializeField] private GameObject powerUpUI;
   [SerializeField] private GameObject speedBoostGO;
   [SerializeField] private GameObject doubleCoinsGO;
   [SerializeField] private GameObject seeEnemyGO;
   [SerializeField] private GameObject gunGO;
   [SerializeField] private GameObject enemySpeedBoostGO;
   [SerializeField] private GameObject speedSlowGO;

   public static bool isPauseScreenActive;

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
      isPauseScreenActive = pauseScreen.activeSelf;

      if (PowerUpManager.Instance != null)
      {
         speedBoostGO.SetActive(PowerUpManager.Instance.IsPowerUpActive(PowerUpType.SpeedBoost));
         doubleCoinsGO.SetActive(PowerUpManager.Instance.IsPowerUpActive(PowerUpType.DoubleCoins));
         seeEnemyGO.SetActive(PowerUpManager.Instance.IsPowerUpActive(PowerUpType.SeeEnemy));
         gunGO.SetActive(PowerUpManager.Instance.IsPowerUpActive(PowerUpType.Gun));
         enemySpeedBoostGO.SetActive(PowerUpManager.Instance.IsPowerUpActive(PowerUpType.EnemySpeedBoost));
         speedSlowGO.SetActive(PowerUpManager.Instance.IsPowerUpActive(PowerUpType.SpeedSlow));
      }
   }

   public void ShowPauseScreen()
   {
      FindFirstObjectByType<FirstPersonController>().cameraCanMove = false;
      pauseScreen.SetActive(true);
      Time.timeScale = 0f;

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;

      SoundManager.Instance.PauseAllSound();
   }
   public void HidePauseScreen()
   {
      FindFirstObjectByType<FirstPersonController>().cameraCanMove = true;
      Time.timeScale = 1f;
      pauseScreen.SetActive(false);

      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
      SoundManager.Instance.ResumeAllSounds();
   }

   public void SetPowerUpText(PowerUpType type, string text)
   {
      GameObject go = null;
      switch (type)
      {
         case PowerUpType.SpeedBoost:
            go = speedBoostGO; break;
         case PowerUpType.DoubleCoins:
            go = doubleCoinsGO; break;
         case PowerUpType.SeeEnemy:
            go = seeEnemyGO; break;
         case PowerUpType.Gun:
            go = gunGO; break;
         case PowerUpType.EnemySpeedBoost:
            go = enemySpeedBoostGO; break;
         case PowerUpType.SpeedSlow:
            go = speedSlowGO; break;
      }

      if (go == null) return;
      var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
      if (tmp != null) tmp.text = text;
      go.SetActive(!string.IsNullOrEmpty(text));
   }

   public void IntoMenu()
   {
      HidePauseScreen();

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;

      FindFirstObjectByType<SceneLoader>().IntoMenu();
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