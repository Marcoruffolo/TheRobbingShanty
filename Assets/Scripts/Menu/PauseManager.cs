using UnityEngine;
using UnityEngine.SceneManagement;


// Maneja el estado de pausa del juego.
// Se comunica con la UI a través de eventos, no con referencias directas.

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    public event System.Action<bool> OnPauseChanged;

    [Header("Nombre de la escena del menú principal")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnPauseToggle += TogglePause;
    }

    private void OnDestroy()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnPauseToggle -= TogglePause;
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else          Pause();
    }

    public void Pause()
    {
        IsPaused         = true;
        Time.timeScale   = 0f;
        PlayerCamera.LockCursor(false);
        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        IsPaused         = false;
        Time.timeScale   = 1f;
        PlayerCamera.LockCursor(true);
        OnPauseChanged?.Invoke(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
