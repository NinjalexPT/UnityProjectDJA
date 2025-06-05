using TMPro;
using UnityEngine;

public class FinishController : MonoBehaviour
{

    private BoxCollider boxCollider;
    private GameObject passTextObject;
    private TextMeshProUGUI passText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject opening = GameObject.Find("Opening");
        boxCollider = opening.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider component is missing on the FinishController GameObject.");
        }

        passTextObject = GameObject.Find("PassText");
        if (passText != null)
        {
            this.passText = passText.GetComponent<TextMeshProUGUI>();
            if (this.passText == null)
            {
                Debug.LogError("TextMeshProUGUI component is missing on the PassText GameObject.");
            }
            else
            {
                this.passText.text = $"You need at least {GameManager.Instance.CoinsToWin()} coins to pass";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider is not initialized. Please check the Start method.");
            return;
        }
        //check player coins counter
        if (GameManager.Instance != null && GameManager.Instance.CoinCount() >= GameManager.Instance.CoinsToWin())
        {
            // If the player has enough coins, disable the collider
            boxCollider.enabled = false;
            passTextObject.SetActive(false);
        }
        else
        {
            // If not enough coins, enable the collider
            boxCollider.enabled = true;
            passTextObject.SetActive(true);
        }
    }

}
