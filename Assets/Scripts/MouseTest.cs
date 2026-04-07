using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTest : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"Mouse.current: {Mouse.current}");
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    }
}