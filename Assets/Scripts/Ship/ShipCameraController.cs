using UnityEngine;

/// <summary>
/// Cámara en tercera persona que sigue al barco desde atrás.
/// Se activa solo cuando el jugador toma el timón.
/// 
/// Setup: este script va en un GameObject de cámara separado (no la cámara del jugador).
/// Asignalo como hijo del barco o como objeto independiente con referencia al barco.
/// </summary>
public class ShipCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform shipTransform;

    [Header("Posición")]
    [SerializeField] private float distance = 12f;   // distancia detrás del barco
    [SerializeField] private float height   = 5f;    // altura sobre el barco
    [SerializeField] private float smoothSpeed = 4f; // suavizado de movimiento

    private Camera _camera;
    private Camera _playerCamera;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.enabled = false; // empieza desactivada
    }

    private void LateUpdate()
    {
        if (!_camera.enabled || shipTransform == null) return;

        // Posición deseada: detrás y arriba del barco
        Vector3 desiredPos = shipTransform.position
                           - shipTransform.forward * distance
                           + Vector3.up * height;

        // Suavizado
        transform.position = Vector3.Lerp(
            transform.position, desiredPos, smoothSpeed * Time.deltaTime
        );

        // Siempre mirar al barco
        transform.LookAt(shipTransform.position + Vector3.up * 2f);
    }

    // ── API pública ───────────────────────────────────────────────
    public void SetPlayerCamera(Camera cam)
    {
        _playerCamera = cam;
    }

    public void Activate()
    {
        if (shipTransform != null)
        {
            transform.position = shipTransform.position
                               - shipTransform.forward * distance
                               + Vector3.up * height;
            transform.LookAt(shipTransform.position + Vector3.up * 2f);
        }

        if (_camera == null)
            _camera = GetComponent<Camera>();

        _camera.enabled = true;

        if (_playerCamera != null)
            _playerCamera.enabled = false;
    }

    public void Deactivate()
    {
        _camera.enabled = false;

        if (_playerCamera != null)
            _playerCamera.enabled = true;
    }
}
