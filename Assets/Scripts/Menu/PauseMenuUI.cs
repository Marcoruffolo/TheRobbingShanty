using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseRoot;

    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;
    [SerializeField] private Button btnQuitGame;

    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private GameObject panelRebind;

    [SerializeField] private OptionsUI optionsUI;


    [SerializeField] private SOVariableInt installedNavigationCores;
    [SerializeField] private SOVariableBool artifactIslandCompleted;
    [SerializeField] private SOVariableBool artifactCoreClaimed;


    private void Start()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnPauseChanged += OnPauseChanged;

        btnPlay.onClick.AddListener(OnPlay);
        btnOptions.onClick.AddListener(OnOptions);
        btnQuit.onClick.AddListener(OnQuit);
        btnQuitGame.onClick.AddListener(OnQuitGame);
        if (optionsUI != null)
            optionsUI.OnBack = ShowMain;

        pauseRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnPauseChanged -= OnPauseChanged;
    }


    private void OnPauseChanged(bool isPaused)
    {
        pauseRoot.SetActive(isPaused);
        if (isPaused) ShowMain();
    }


    private void OnPlay()
    {
        PauseManager.Instance.Resume();
    }

    private void OnOptions()
    {
        panelMain.SetActive(false);
        panelOptions.SetActive(true);
    }

    private void OnQuit()
    {
        PauseManager.Instance.GoToMainMenu();
        installedNavigationCores?.ResetValue();
        artifactIslandCompleted?.ResetValue();
        artifactCoreClaimed?.ResetValue();
    }

    private void ShowMain()
    {
        panelMain.SetActive(true);
        panelOptions.SetActive(false);
        panelRebind.SetActive(false);
    }

    private void OnQuitGame()
    {
        Application.Quit();
        installedNavigationCores?.ResetValue();
        artifactIslandCompleted?.ResetValue();
        artifactCoreClaimed?.ResetValue();
    }
}
