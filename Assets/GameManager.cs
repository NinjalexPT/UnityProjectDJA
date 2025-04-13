using TMPro;
using UnityEngine;
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
  private bool fog;

  public void AddCoins(int amount)
  {
    coinCount += amount;
    coinCountText.text = "" + coinCount;
  }

  public void ShowInteractText()
  {
    interactText.rectTransform.anchoredPosition = new Vector3(100f, -20f, 0f);
  }

  public void HideInteractText()
  {
    interactText.rectTransform.anchoredPosition = new Vector3(100f, 60f, 0f);
  }

  public void PlayerDied()
  {
    FirstPersonController playerController = FindFirstObjectByType<FirstPersonController>();
    deathScreen.SetActive(true);

    HideInteractText();
    coinObject.SetActive(false);

    playerController.enableSprint = false;
    playerController.enableCrouch = false;
    playerController.enableJump = false;
    playerController.enableZoom = false;
    playerController.playerCanMove = false;
    playerController.cameraCanMove = false;
  }

  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
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
