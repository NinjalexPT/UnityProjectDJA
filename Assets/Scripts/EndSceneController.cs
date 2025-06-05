using System.Collections;
using UnityEngine;

public class EndSceneController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ExitScene());
    }

    private IEnumerator ExitScene()
    {
        yield return new WaitForSeconds(5f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("Mainmenu");
    }
}
