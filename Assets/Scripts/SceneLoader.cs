using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SceneLoader : MonoBehaviour
{
    public GameObject OutTrans;
    public Animator animatorOutTrans;
    public string OutTransitionAnim;


    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Mainmenu");
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }




    public void IntoGame()
    {
        StartCoroutine(TransitioningINTO());
    }

    public void RestartGame()
    {
        StartCoroutine(TransitioningOUTOF());
    }


    public void AppExit()
    {
        Application.Quit();
    }



    private IEnumerator TransitioningINTO()
    {
        OutTrans = GameObject.Find("Canvas")?.transform
    .Find("OutTransition")?.gameObject;
        OutTransitionAnim = "OutTransitionAnim";

        if (OutTrans == null)
        {
            Debug.LogError("GameObject 'OutTransition' não foi encontrado!");
            yield break;
        }

        OutTrans.SetActive(true);

        animatorOutTrans = OutTrans.GetComponent<Animator>();
        if (animatorOutTrans == null)
        {
            Debug.LogError(" Animator não encontrado em 'OutTransition'.");
            yield break;
        }

        if (string.IsNullOrEmpty(OutTransitionAnim))
        {
            Debug.LogError(" O nome da animação está vazio.");
            yield break;
        }

        animatorOutTrans.Play(OutTransitionAnim);

        while (animatorOutTrans.GetCurrentAnimatorStateInfo(0).IsName(OutTransitionAnim) &&
               animatorOutTrans.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        NextScene();
    }

    private IEnumerator TransitioningOUTOF()
    {
        OutTrans = GameObject.Find("Canvas")?.transform
    .Find("OutTransition")?.gameObject;
        OutTransitionAnim = "OutTransitionAnim";

        if (OutTrans == null)
        {
            Debug.LogError("GameObject 'OutTransition' não foi encontrado!");
            yield break;
        }

        OutTrans.SetActive(true);

        animatorOutTrans = OutTrans.GetComponent<Animator>();
        if (animatorOutTrans == null)
        {
            Debug.LogError(" Animator não encontrado em 'OutTransition'.");
            yield break;
        }

        if (string.IsNullOrEmpty(OutTransitionAnim))
        {
            Debug.LogError(" O nome da animação está vazio.");
            yield break;
        }

        animatorOutTrans.Play(OutTransitionAnim);

        while (animatorOutTrans.GetCurrentAnimatorStateInfo(0).IsName(OutTransitionAnim) &&
               animatorOutTrans.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        ReloadScene();
    }

}
