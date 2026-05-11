using UnityEngine;

public class InteractionLabel : MonoBehaviour
{
    [SerializeField] private string prompt = "[E] Interactuar";

    private bool _inUse;

    public string Prompt => prompt;
    public bool IsVisible => !_inUse;

    public void SetInUse(bool inUse) => _inUse = inUse;
}
