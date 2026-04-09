using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// Controla el panel de opciones (sensibilidad, música, efectos).
/// Funciona tanto en el menú principal como en el menú de pausa.
/// Asigná los sliders y textos desde el Inspector.

public class OptionsUI : MonoBehaviour
{
    [Header("Sensibilidad")]
    [SerializeField] private Slider sliderSensitivity;
    [SerializeField] private TMP_Text valueSensitivity;

    [Header("Audio")]
    [SerializeField] private Slider sliderMusic;
    [SerializeField] private TMP_Text valueMusic;

    [SerializeField] private Slider sliderSFX;
    [SerializeField] private TMP_Text valueSFX;

    [Header("Botones")]
    [SerializeField] private Button btnSave;
    [SerializeField] private Button btnCancel;

    // Callback para cuando se aprieta Volver (lo asigna el padre)
    public System.Action OnBack;

    private void OnEnable()
    {
        // Cargar valores actuales cada vez que se abre el panel
        LoadCurrentValues();
    }

    private void Start()
    {
        sliderSensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        sliderMusic.onValueChanged.AddListener(OnMusicChanged);
        sliderSFX.onValueChanged.AddListener(OnSFXChanged);

        btnSave.onClick.AddListener(OnSave);
        btnCancel.onClick.AddListener(OnBackPressed);
    }

    // Cargar valores desde GameSettings

    private void LoadCurrentValues()
    {
        if (GameSettings.Instance == null) return;

        sliderSensitivity.value = GameSettings.Instance.mouseSensitivity;
        sliderMusic.value       = GameSettings.Instance.musicVolume;
        sliderSFX.value         = GameSettings.Instance.sfxVolume;

        UpdateLabels();
    }

    // Callbacks de sliders

    private void OnSensitivityChanged(float value)
    {
        if (valueSensitivity != null)
            valueSensitivity.text = value.ToString("F1");
    }

    private void OnMusicChanged(float value)
    {
        if (valueMusic != null)
            valueMusic.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnSFXChanged(float value)
    {
        if (valueSFX != null)
            valueSFX.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void UpdateLabels()
    {
        OnSensitivityChanged(sliderSensitivity.value);
        OnMusicChanged(sliderMusic.value);
        OnSFXChanged(sliderSFX.value);
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
