using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Header("Sensibilidad")]
    [Range(0.1f, 10f)] public float mouseSensitivity = 2f;

    [Header("Audio")]
    [Range(-80f, 20f)] public float generalVolume = 1f;
    [Range(-80f, 20f)] public float musicVolume = 1f;
    [Range(-80f, 20f)] public float sfxVolume = 1f;
    private const string KEY_SENSITIVITY   = "sensitivity";
    private const string KEY_MUSIC_VOLUME  = "musicVolume";
    private const string KEY_SFX_VOLUME    = "sfxVolume";
    private const string KEY_GENERAL_VOLUME = "generalVolume";

    public event System.Action<float> OnSensitivityChanged;
    public event System.Action<float> OnGeneralVolumeChanged;
    public event System.Action<float> OnMusicVolumeChanged;
    public event System.Action<float> OnSFXVolumeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void SetSensitivity(float value)
    {
        mouseSensitivity = Mathf.Clamp(value, 0.1f, 10f);
        OnSensitivityChanged?.Invoke(mouseSensitivity);
        Save();
    }

    public void SetGeneralVolume(float value)
    {
        generalVolume = Mathf.Clamp(value, -80f, 20f);
        OnGeneralVolumeChanged?.Invoke(generalVolume);
        Save();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp(value, -80f, 20f);
        OnMusicVolumeChanged?.Invoke(musicVolume);
        Save();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp(value, -80f, 20f);
        OnSFXVolumeChanged?.Invoke(sfxVolume);
        Save();
    }

    // Guardado sensibilidad, musica, sfx con PlayerPrefs

    public void Save()
    {
        PlayerPrefs.SetFloat(KEY_SENSITIVITY,  mouseSensitivity);
        PlayerPrefs.SetFloat(KEY_GENERAL_VOLUME, generalVolume);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, musicVolume);
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME,   sfxVolume);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        mouseSensitivity = PlayerPrefs.GetFloat(KEY_SENSITIVITY,  mouseSensitivity);
        generalVolume    = PlayerPrefs.GetFloat(KEY_GENERAL_VOLUME, generalVolume);
        musicVolume      = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, musicVolume);
        sfxVolume        = PlayerPrefs.GetFloat(KEY_SFX_VOLUME,   sfxVolume);
    }
}
