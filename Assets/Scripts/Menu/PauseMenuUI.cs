using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Root del menú de pausa")]
    [SerializeField] private GameObject pauseRoot;

    [Header("Botones")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;

    [Header("Paneles")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;

    [Header("Scripts")]
    [SerializeField] private OptionsUI optionsUI;

    private void Start()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnPauseChanged += OnPauseChanged;

        btnPlay.onClick.AddListener(OnPlay);
        btnOptions.onClick.AddListener(OnOptions);
        btnQuit.onClick.AddListener(OnQuit);

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
    }

    private void ShowMain()
    {
        panelMain.SetActive(true);
        panelOptions.SetActive(false);
    }
}
