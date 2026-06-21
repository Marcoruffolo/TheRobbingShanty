using UnityEngine;

public class ShipDeath : MonoBehaviour
{
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private VoidGameEvent onPlayerDeath;
    [SerializeField] private float sinkDuration = 2f;
    [SerializeField] private float sinkDistance = 3f;
    [SerializeField] private float sinkTilt = 15f;

    private ShipController _shipController;
    private bool _isSinking;
    private float _sinkTimer;
    private Vector3 _startPosition;
    private Quaternion _startRotation;


    private void Awake()
    {
        _shipController = GetComponent<ShipController>();
    }
    void Update()
    {
        if (!_isSinking)
        {
            if (durability.Value > 0f) return;

            _isSinking = true;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _shipController.StopControlling();
        }

        _sinkTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(_sinkTimer / Mathf.Max(0.01f, sinkDuration));

        transform.position = Vector3.Lerp(_startPosition, _startPosition + Vector3.down * sinkDistance, progress);
        transform.rotation = _startRotation * Quaternion.Euler(0f, 0f, sinkTilt * progress);

        if (progress >= 1f)
        {
            enabled = false;
            onPlayerDeath.Raise();
        }
    }
}
