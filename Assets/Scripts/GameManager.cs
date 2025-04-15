using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance;

  [SerializeField]
  private int coinCount;

  [SerializeField]
  private TextMeshProUGUI coinCountText;
  [SerializeField]
  private GameObject coinObject;

  [SerializeField]
  private TextMeshProUGUI interactText;

  [SerializeField]
  private GameObject deathScreen;
  [SerializeField]
  private GameObject playerUI;

  [SerializeField]
  private bool fog;

  public bool gameOver;

  public void AddCoins(int amount)
  {
    coinCount += amount;
    coinCountText.text = "" + coinCount;
  }

  public int CoinCount()
  {
    return this.coinCount;
  }

  public void ShowInteractText(String text)
  {
    interactText.text = text;
    interactText.rectTransform.anchoredPosition = new Vector3(100f, -20f, 0f);
  }

  public void HideInteractText()
  {
    interactText.rectTransform.anchoredPosition = new Vector3(100f, 60f, 0f);
  }

  public void PlayerDied()
  {

    gameOver = true;

    FirstPersonController playerController = FindFirstObjectByType<FirstPersonController>();
    deathScreen.SetActive(true);

    HideInteractText();
    coinObject.SetActive(false);

    playerUI.SetActive(false);
    Destroy(playerController);

    SoundManager.Instance.StopMovementSound();

  }

  public void RestartGame()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }
  void Start()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(this);
    }

    RenderSettings.fog = fog;
  }

}
