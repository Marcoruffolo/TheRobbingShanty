using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el menú principal.
/// Asigná los botones desde el Inspector.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnQuit;

    [Header("Paneles")]
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelOptions;

    [Header("Scripts")]
    [SerializeField] private OptionsUI optionsUI;

    [Header("Nombre de la escena de juego")]
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        PlayerCamera.LockCursor(false);

        btnPlay.onClick.AddListener(OnPlay);
        btnOptions.onClick.AddListener(OnOptions);
        btnQuit.onClick.AddListener(OnQuit);

        if (optionsUI != null)
        {
            optionsUI.OnBack = ShowMain;
        }

        ShowMain();
    }

    private void OnPlay()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnOptions()
    {
        panelMain.SetActive(false);
        panelOptions.SetActive(true);
    }

    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowMain()
    {
        panelMain.SetActive(true);
        panelOptions.SetActive(false);
    }
}
