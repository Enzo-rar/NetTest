using UnityEngine;
using UnityEngine.UI;

public class LocalSettingsManager : MonoBehaviour
{
    public GameObject optionsPanel;
    public Slider musicSlider;
    public Slider fxSlider;
    public Slider sensXSlider;
    public Slider sensYSlider;

    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        fxSlider.value = PlayerPrefs.GetFloat("FXVolume", 1f);
        sensXSlider.value = PlayerPrefs.GetFloat("SensX", 20f);
        sensYSlider.value = PlayerPrefs.GetFloat("SensY", 20f);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        fxSlider.onValueChanged.AddListener(SetFXVolume);
        sensXSlider.onValueChanged.AddListener(SetSensX);
        sensYSlider.onValueChanged.AddListener(SetSensY);
    }

    void Update()
    {
        // Abrir y cerrar con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }
    }

    public void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);

        // Si abrimos las opciones, liberamos el ratón. Si las cerramos, lo ocultamos.
        if (optionsPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetMusicVolume(float value) { PlayerPrefs.SetFloat("MusicVolume", value); }
    public void SetFXVolume(float value) { PlayerPrefs.SetFloat("FXVolume", value); }
    public void SetSensX(float value) { PlayerPrefs.SetFloat("SensX", value); }
    public void SetSensY(float value) { PlayerPrefs.SetFloat("SensY", value); }
}