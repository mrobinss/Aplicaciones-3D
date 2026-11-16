using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderVolumenMusica : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        float volumenActual = MenuMusicManager.instance != null
            ? MenuMusicManager.instance.volumenUsuario
            : PlayerPrefs.GetFloat("VolumenMusica", 1f);

        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, volumenActual);
        slider.onValueChanged.AddListener(AlMoverSlider);
    }

    private void AlMoverSlider(float valorSlider)
    {
        float volumenNormalizado = Mathf.InverseLerp(slider.minValue, slider.maxValue, valorSlider);

        if (MenuMusicManager.instance != null)
        {
            MenuMusicManager.instance.SetVolumen(volumenNormalizado);
        }
        else
        {
            PlayerPrefs.SetFloat("VolumenMusica", volumenNormalizado);
            PlayerPrefs.Save();
        }
    }

    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(AlMoverSlider);
        }
    }
}