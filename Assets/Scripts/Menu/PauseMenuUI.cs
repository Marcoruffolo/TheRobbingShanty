using UnityEngine;
using UnityEngine.UI;


/// Controla el menú de pausa.
/// Se muestra/oculta automáticamente escuchando a PauseManager.
/// Asigná los botones y paneles desde el Inspector.
public class PauseMenuUI : MonoBehaviour
{
    [Header("Root del menú de pausa")]
    [SerializeField] private GameObject pauseRoot;

    [Header("Paneles")]
    [SerializeField] private GameObject panelPause;
    [SerializeField] private GameObject panelOptions;

    [Header("Botones — Panel Pausa")]
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnMainMenu;

    [Header("Botones — Panel Opciones")]
    [SerializeField] private Button btnOptionsBack;

    private void Start()
    {
        // Suscribirse al evento de pausa
        if (PauseManager.Instance != null)
            PauseManager.Instance.OnPauseChanged += OnPauseChanged;

        btnResume.onClick.AddListener(OnResume);
        btnOptions.onClick.AddListener(OnOptions);
        btnMainMenu.onClick.AddListener(OnMainMenu);
        btnOptionsBack.onClick.AddListener(OnOptionsBack);

        // Empezar oculto
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
        if (isPaused) ShowPausePanel();
    }


    private void OnResume()
    {
        PauseManager.Instance.Resume();
    }

    private void OnOptions()
    {
        panelPause.SetActive(false);
        panelOptions.SetActive(true);
    }

    private void OnMainMenu()
    {
        PauseManager.Instance.GoToMainMenu();
    }

    private void OnOptionsBack()
    {
        ShowPausePanel();
    }

    private void ShowPausePanel()
    {
        panelPause.SetActive(true);
        panelOptions.SetActive(false);
    }
}
