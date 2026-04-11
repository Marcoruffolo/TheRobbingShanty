using UnityEngine;
using UnityEngine.InputSystem;

public class Testsoap : MonoBehaviour
{
    public SOVariableInt testInt;
    [SerializeField] private VoidGameEvent dropGameEvent;
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            testInt.SetValue(testInt.Value - 1);
            dropGameEvent.Raise();
        }
    }
}
