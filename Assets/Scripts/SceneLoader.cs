using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public bool IsLoading => _loadRoutine != null;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private GameObject loadingVisual;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float minimumBlackScreenDuration = 0.15f;

    private Coroutine _loadRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetOverlayAlpha(0f);
        SetOverlayInteractivity(false);
        SetLoadingVisual(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool TryLoadScene(SceneField sceneField)
    {
        if (sceneField == null)
        {
            Debug.LogError("[SceneLoader] SceneField reference is missing.");
            return false;
        }

        return TryLoadScene(sceneField.SceneName);
    }

    public bool TryLoadScene(string sceneName)
    {
        if (_loadRoutine != null)
            return false;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SceneLoader] Scene name is empty.");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not available in Build Settings.");
            return false;
        }

        _loadRoutine = StartCoroutine(LoadSceneRoutine(sceneName));
        return true;
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        SetOverlayInteractivity(true);
        SetLoadingVisual(true);

        yield return FadeTo(1f, fadeOutDuration);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (loadOperation == null)
        {
            Debug.LogError($"[SceneLoader] Failed to create async load operation for '{sceneName}'.");
            yield return FadeTo(0f, fadeInDuration);
            SetLoadingVisual(false);
            SetOverlayInteractivity(false);
            _loadRoutine = null;
            yield break;
        }

        float blackScreenTime = 0f;

        while (!loadOperation.isDone || blackScreenTime < minimumBlackScreenDuration)
        {
            blackScreenTime += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return null;
        yield return FadeTo(0f, fadeInDuration);

        SetLoadingVisual(false);
        SetOverlayInteractivity(false);
        _loadRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha = fadeCanvasGroup.alpha;

        if (duration <= 0f)
        {
            SetOverlayAlpha(targetAlpha);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float easedAlpha = Mathf.SmoothStep(startAlpha, targetAlpha, normalizedTime);

            SetOverlayAlpha(easedAlpha);
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = alpha;
    }

    private void SetOverlayInteractivity(bool isInteractive)
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.blocksRaycasts = isInteractive;
        fadeCanvasGroup.interactable = isInteractive;
    }

    private void SetLoadingVisual(bool isVisible)
    {
        if (loadingVisual == null)
            return;

        loadingVisual.SetActive(isVisible);
    }
}
