   using UnityEngine;
   using UnityEngine.UI;
   using UnityEngine.SceneManagement;

   public class SettingsManager : MonoBehaviour
   {
       public Slider sliderMusica;
       public Slider sliderEfectos;
       public Slider sliderDialogo;

       void Start()
       {
           sliderMusica.value = PlayerPrefs.GetFloat("VolMusica", 1f);
           sliderEfectos.value = PlayerPrefs.GetFloat("VolEfectos", 1f);
           sliderDialogo.value = PlayerPrefs.GetFloat("VolDialogo", 1f);
       }

       public void OnAceptar()
       {
           PlayerPrefs.SetFloat("VolMusica", sliderMusica.value);
           PlayerPrefs.SetFloat("VolEfectos", sliderEfectos.value);
           PlayerPrefs.SetFloat("VolDialogo", sliderDialogo.value);
           PlayerPrefs.Save();
       }

       public void OnSalir()
       {
           SceneManager.LoadScene(SceneHistory.LastScene);
       }
   }