using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderVolumenEfectos : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        float volumenActual = UIAudioManager.instance != null
            ? UIAudioManager.instance.volumenEfectos
            : PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, volumenActual);
        slider.onValueChanged.AddListener(AlMoverSlider);
    }

    private void AlMoverSlider(float valorSlider)
    {
        float volumenNormalizado = Mathf.InverseLerp(slider.minValue, slider.maxValue, valorSlider);

        if (UIAudioManager.instance != null)
        {
            UIAudioManager.instance.SetVolumenEfectos(volumenNormalizado);
        }
        else
        {
            PlayerPrefs.SetFloat("VolumenEfectos", volumenNormalizado);
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