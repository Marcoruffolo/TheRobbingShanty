using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnMainMenu;
    [SerializeField] private Button btnExit;
    [SerializeField] private SceneField gameScene;
    [SerializeField] private SceneField mainMenuScene;
    [SerializeField] private SOVariableFloat durability;

    [SerializeField] private SOVariableInt installedNavigationCores;
    [SerializeField] private SOVariableBool artifactIslandCompleted;
    [SerializeField] private SOVariableBool artifactCoreClaimed;

    private bool _isGameOver;

    void Start()
    {
        btnRestart.onClick.AddListener(Restart);
        btnMainMenu.onClick.AddListener(GoToMainMenu);
        btnExit.onClick.AddListener(ExitGame);
        gameOverPanel.SetActive(false);
    }

    public void Show()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        PlayerCamera.LockCursor(false);
    }
    private void Restart()
    {
        ResetRun();

        SceneManager.LoadScene(gameScene);
    }

    private void GoToMainMenu()
    {
        ResetRun();
        SceneManager.LoadScene(mainMenuScene);
    }

    private void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ResetRun()
    {
        Time.timeScale = 1f;
        durability.ResetValue();

        installedNavigationCores?.ResetValue();
        artifactIslandCompleted?.ResetValue();
        artifactCoreClaimed?.ResetValue();
        if (PlayerInventoryHolder.Instance != null)
            PlayerInventoryHolder.Instance.ClearInventory();
    }
}
