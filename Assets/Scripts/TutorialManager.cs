using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
   [Header("UI")]
   [SerializeField] private GameObject interactText;
   [SerializeField] private GameObject blackScreen;
   [SerializeField] private GameObject playerUi;

   [Space]

   [Header("Interaction")]
   [SerializeField] private Collider colliderBed;
   [SerializeField] private bool canInteract = false;

   [Space]

   [Header("Sound")]
   [SerializeField] private AudioSource audioSource;
   [SerializeField] private AudioClip arguing;

   void Start()
   {
      StartCoroutine(CutScene());
   }

   void Update()
   {
      if (!canInteract) return;

      if (Input.GetKey(KeyCode.E))
      {
         FindFirstObjectByType<SceneLoader>().IntoMaze();

      }
   }

   IEnumerator CutScene()
   {
      audioSource.volume = SoundManager.Instance.sfxVolume;
      audioSource.PlayOneShot(arguing);
      yield return new WaitForSeconds(arguing.length);
      blackScreen.SetActive(false);
      playerUi.SetActive(true);
      canInteract = true;
   }

}
