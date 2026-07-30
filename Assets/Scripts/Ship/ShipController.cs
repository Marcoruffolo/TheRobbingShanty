using UnityEngine;

/// <summary>
/// Controla el movimiento del barco cuando el jugador está en el timón.
/// Se activa y desactiva desde SteeringWheel.
/// </summary>
public class ShipController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private SOVariableFloat speed;
    [SerializeField] private float turnSpeed    = 60f;
    [SerializeField] private float tiltAngle    = 5f;   // inclinación al girar
    [SerializeField] private float tiltSmooth   = 3f;

    private bool  _isControlled = false;
    private float _currentTilt  = 0f;

    // ── API pública ───────────────────────────────────────────────

    public void StartControlling() => _isControlled = true;
    public void StopControlling()
    {
        _isControlled = false;
        _currentTilt  = 0f;
        // Resetear inclinación
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    // ─────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!_isControlled) return;
        if (PlayerInputHandler.Instance == null) return;

        Vector2 input = PlayerInputHandler.Instance.MoveInput;

        // Avance / retroceso
        transform.Translate(Vector3.forward * input.y * speed.Value * Time.deltaTime);

        // Giro horizontal
        float turn = input.x * turnSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * turn);

        // Inclinación visual al girar
        float targetTilt = -input.x * tiltAngle;
        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, tiltSmooth * Time.deltaTime);
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, _currentTilt);
    }
}
