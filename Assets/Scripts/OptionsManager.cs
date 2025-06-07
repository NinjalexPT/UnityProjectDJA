using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{

   public static OptionsManager Instance;

   private bool vsync;

   public TextMeshProUGUI vsyncText;

   void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
      }
      else
      {
         Destroy(gameObject);
      }

      GameObject.Find("MusicSlider").GetComponent<Slider>().value = SoundManager.Instance.musicVolume;
      GameObject.Find("SFXSlider").GetComponent<Slider>().value = SoundManager.Instance.sfxVolume;
   }

   public void OnChangeVolume()
   {
      SoundManager.Instance.musicVolume = GameObject.Find("MusicSlider").GetComponent<Slider>().value;
      SoundManager.Instance.sfxVolume = GameObject.Find("SFXSlider").GetComponent<Slider>().value;
   }

   public void OnVSyncChange()
   {
      Debug.Log("clicked");
      vsync = !vsync;
      QualitySettings.vSyncCount = vsync ? 1 : 0;

      vsyncText.text = vsync ? "On" : "Off";
      if (vsync)
      {
         Debug.Log("VSync enabled");
      }
      else
      {
         Debug.Log("VSync disabled");
      }
   }

}
