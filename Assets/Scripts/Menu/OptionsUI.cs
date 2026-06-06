using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class OptionsUI : MonoBehaviour
{
    [Header("Sensibilidad")]
    [SerializeField] private Slider sliderSensitivity;
    [SerializeField] private TMP_Text valueSensitivity;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Slider sliderGeneral;
    [SerializeField] private TMP_Text valueGeneral;

    [SerializeField] private Slider sliderMusic;
    [SerializeField] private TMP_Text valueMusic;

    [SerializeField] private Slider sliderSFX;
    [SerializeField] private TMP_Text valueSFX;

    [Header("Botones")]
    [SerializeField] private Button btnCancel;

    // Callback para cuando se aprieta Volver (lo asigna el padre)
    public System.Action OnBack;


    private void OnEnable()
    {
        // Cargar valores actuales cada vez que se abre el panel
        //LoadCurrentValues();
    }

    private void Start()
    {
        sliderSensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        sliderGeneral.onValueChanged.AddListener(OnGeneralChanged);
        sliderMusic.onValueChanged.AddListener(OnMusicChanged);
        sliderSFX.onValueChanged.AddListener(OnSFXChanged);

        btnCancel.onClick.AddListener(OnBackPressed);
    }

    // Cargar valores desde GameSettings

    private void LoadCurrentValues()
    {
        if (GameSettings.Instance == null) return;

        sliderSensitivity.value = GameSettings.Instance.mouseSensitivity;
        sliderGeneral.value   = GameSettings.Instance.generalVolume;
        sliderMusic.value     = GameSettings.Instance.musicVolume;
        sliderSFX.value       = GameSettings.Instance.sfxVolume;

        UpdateLabels();
    }

    // Callbacks de sliders

    private void OnSensitivityChanged(float value)
    {
        if (valueSensitivity != null)
            valueSensitivity.text = value.ToString("F1");
    }

    private void OnGeneralChanged(float value)
    {
        if (valueGeneral != null)
            valueGeneral.text = Mathf.RoundToInt(value + 80) + "%";
            
        audioMixer.SetFloat("General", value);
    }

    private void OnMusicChanged(float value)
    {
        if (valueMusic != null)
            valueMusic.text = Mathf.RoundToInt(value + 80) + "%";
            
        audioMixer.SetFloat("Music", value);
    }

    private void OnSFXChanged(float value)
    {
        if (valueSFX != null)
            valueSFX.text = Mathf.RoundToInt(value + 80) + "%";
            
        audioMixer.SetFloat("SFX", value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void UpdateLabels()
    {
        OnSensitivityChanged(sliderSensitivity.value);
        OnMusicChanged(sliderMusic.value);
        OnSFXChanged(sliderSFX.value);
        OnGeneralChanged(sliderGeneral.value);
    }


    private void OnSave()
    {
        if (GameSettings.Instance == null) return;

        GameSettings.Instance.SetSensitivity(sliderSensitivity.value);
        GameSettings.Instance.SetMusicVolume(sliderMusic.value);
        GameSettings.Instance.SetSFXVolume(sliderSFX.value);

        OnBack?.Invoke();
    }

    private void OnBackPressed()
    {
        OnBack?.Invoke();
    }

}
