using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndDemoUI : MonoBehaviour
{
    [SerializeField] private Button btnPlay;
    [SerializeField] private Button btnExit;
    [SerializeField] private SceneField gameScene;
    [SerializeField] private NavigationRunState navigationRunState;

    private void Start()
    {
        PlayerCamera.LockCursor(false);
        btnPlay.onClick.AddListener(OnPlay);
        btnExit.onClick.AddListener(OnExit);
    }

    private void OnPlay()
    {
        navigationRunState?.ResetRun();
        SceneManager.LoadScene(gameScene);
    }

    private void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
