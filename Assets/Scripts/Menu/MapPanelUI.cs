using UnityEngine;

public class MapPanelUI : MonoBehaviour
{
    public static MapPanelUI Instance { get; private set; }

    private void Awake() => Instance = this;

    public void Close() => gameObject.SetActive(false);
}
