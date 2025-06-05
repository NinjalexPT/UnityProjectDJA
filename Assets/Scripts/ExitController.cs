using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitController : MonoBehaviour
{
    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get the player GameObject
        player = GameObject.FindGameObjectWithTag("Player");


    }

    private void OnTriggerEnter(Collider other)
    {
        //Check if the player has entered the exit trigger
        if (other.gameObject == player)
        {
            StartCoroutine(ExitScene());            
        }
    }

    private IEnumerator ExitScene()
    {
        // get the canvas object
        GameObject canvas = GameObject.Find("Canvas");
        // get BlockerImage Image component inside the canvas
        GameObject blockerImageObject = canvas.transform.Find("BlockerImage").gameObject;
        Image blockerImage = blockerImageObject.GetComponent<Image>();
        blockerImage.color = new Color(0, 0, 0, 0); // Set initial color to transparent

        while (blockerImage.color.a < 1)
        {
            yield return new WaitForSeconds(0.025f);
            blockerImage.color = new Color(0, 0, 0, blockerImage.color.a + 0.025f); // Fade in
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
    }

}
