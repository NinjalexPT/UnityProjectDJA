using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   public static GameManager Instance;

   [SerializeField] private int coinCount;
   [SerializeField] private bool fog;
   public bool gameOver;

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
      RenderSettings.fog = fog;
      // Inicializa UI com valor inicial de moedas
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
      var playerController = FindObjectOfType<FirstPersonController>();

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