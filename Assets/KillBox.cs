using UnityEngine;

public class KillBox : MonoBehaviour
{
    [SerializeField] VoidGameEvent endGameEvent;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            endGameEvent?.Raise();
        }
    }
}
