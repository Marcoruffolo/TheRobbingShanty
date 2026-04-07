using UnityEngine;

/// Controla la cámara en primera persona.
/// La sensibilidad se lee desde GameSettings, se actualiza automáticamente
/// cuando el jugador la cambia en opciones sin necesidad de reiniciar.
/// 
/// Setup en la escena:
/// - Este script va en el GameObject de la CÁMARA (hijo del jugador)
/// - El jugador rota en Y (horizontal), la cámara rota en X (vertical)

public class PlayerCamera : MonoBehaviour
{
    [Header("Límites de rotación vertical")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle =  80f;

    [Header("Suavizado (0 = sin suavizado)")]
    [SerializeField] private float smoothTime = 0.05f;

    private PlayerInputHandler _input;
    private Transform          _playerBody; // el padre (jugador) rota horizontal
    private float              _xRotation;  // rotación vertical acumulada
    private float              _sensitivity;

    private Vector2 _currentLook;
    private Vector2 _lookVelocity;

    private void Start()
    {
        _input      = PlayerInputHandler.Instance;
        _playerBody = transform.parent;
        Debug.Log("[PlayerCamera] Start ejecutado en: " + gameObject.name);
        if (_input == null)
            Debug.LogError("[PlayerCamera] No se encontró PlayerInputHandler.");

        if (_playerBody == null)
            Debug.LogError("[PlayerCamera] La cámara debe ser hija del GameObject del jugador.");

        if (GameSettings.Instance != null)
        {
            _sensitivity = GameSettings.Instance.mouseSensitivity;
            GameSettings.Instance.OnSensitivityChanged += OnSensitivityChanged;
        }

        LockCursor(true);
    }

    private void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSensitivityChanged -= OnSensitivityChanged;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerBody.Rotate(Vector3.up * mouseX);
    }

    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    private void OnSensitivityChanged(float newValue)
    {
        _sensitivity = newValue;
    }
}
