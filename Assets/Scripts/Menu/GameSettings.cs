using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Header("Sensibilidad")]
    [Range(0.1f, 10f)] public float mouseSensitivity = 2f;
    private const string KEY_SENSITIVITY   = "sensitivity";

    public event System.Action<float> OnSensitivityChanged;

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

    // Guardado sensibilidad, musica, sfx con PlayerPrefs

    public void Save()
    {
        PlayerPrefs.SetFloat(KEY_SENSITIVITY,  mouseSensitivity);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        mouseSensitivity = PlayerPrefs.GetFloat(KEY_SENSITIVITY,  mouseSensitivity);
    }
}
