using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChalkBoardMenuManager : MonoBehaviour, IGameEventListener<bool>

{
    public BoolGameEvent OnUpgradeUIOpen;

    public GameObject menucamera;

    //public GameObject mainMenu;
    //public GameObject mapMenu;

    public GraphicRaycaster interactablemenu;

    private void OnEnable()
    {
        OnUpgradeUIOpen.RegisterListener(this);
    }

    private void OnDisable()
    {
        OnUpgradeUIOpen.UnregisterListener(this);
    }

    public void OnEventRaised(bool value)
    {
        interactablemenu.enabled = value;
        menucamera.GetComponent<CinemachineCamera>().enabled = value;
        Debug.Log($"Upgrade UI open: {value}");
    }

}
