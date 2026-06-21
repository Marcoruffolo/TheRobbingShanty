using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    public event System.Action<bool> OnPauseChanged;
    
    [SerializeField] private SceneField mainMenuSceneName;

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
        if (Time.timeScale == 0f && !IsPaused)
            return;

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (IsPaused)
            return;

        IsPaused = true;
        Time.timeScale = 0f;
        PlayerCamera.LockCursor(false);
        OnPauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        Time.timeScale = 1f;

        if (CameraModeController.Instance != null)
            CameraModeController.Instance.RestoreGameplayState();
        else
            PlayerCamera.LockCursor(true);

        OnPauseChanged?.Invoke(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}