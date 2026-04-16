using UnityEngine;
using Unity.Cinemachine;

public class TelescopeCamera : MonoBehaviour
{
    [SerializeField] private CinemachinePanTilt panTilt;
    [SerializeField] private SOVariableFloat sensitivity;
    [SerializeField] private float panMin = -80f, panMax = 80f;
    [SerializeField] private float tiltMin = -30f, tiltMax = 30f;

    public bool IsActive { get; set; }

    private void Update()
    {
        if (!IsActive) return;
        Vector2 input = PlayerInputHandler.Instance.LookInput * sensitivity.Value;

        panTilt.PanAxis.Value = Mathf.Clamp(panTilt.PanAxis.Value + input.x, panMin, panMax);
        panTilt.TiltAxis.Value = Mathf.Clamp(panTilt.TiltAxis.Value - input.y, tiltMin, tiltMax);
    }
}