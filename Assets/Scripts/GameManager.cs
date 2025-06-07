using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   public static GameManager Instance;

   [Header("Game Settings")]

   [SerializeField] private int coinCount;
   public bool fog;
   public bool gameOver;

   public bool skipIntro = false;
   public float coinReappearingTimer = 30f;

   [Header("Player Preferences")]
   public string SFX_VOLUME_KEY = "SFXVolume";
   public string MUSIC_VOLUME_KEY = "MusicVolume";
   public string VSYNC_KEY = "VSYNCName";

   public int chanceForKey = 5; // chance out of 100

   void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);
      }
      else
      {
         Destroy(gameObject);
      }
   }

   void Start()
   {
      if (UIManager.Instance != null)
         UIManager.Instance.UpdateCoinCount(coinCount);
   }

   public void AddCoins(int amount)
   {
      coinCount += amount;
      if (UIManager.Instance != null)
         UIManager.Instance.UpdateCoinCount(coinCount);
   }

   public int CoinCount()
   {
      return coinCount;
   }

   public void PlayerDied()
   {
      gameOver = true;
      var playerController = FindFirstObjectByType<FirstPersonController>();

      if (UIManager.Instance != null)
      {
         UIManager.Instance.ShowDeathScreen();
         UIManager.Instance.HideInteractText();
         UIManager.Instance.SetCoinObjectActive(false);
         UIManager.Instance.SetPlayerUIActive(false);
      }

      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;

      Destroy(playerController);
      SoundManager.Instance.StopMovementSound();
   }

   public void RestartGame()
   {
      SceneManager.LoadScene(SceneManager
        .GetActiveScene().buildIndex);
   }
}