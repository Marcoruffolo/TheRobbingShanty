using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ResizeOnMouseWheel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] float startSize = 1;
    [SerializeField] float minSize = 0.25f;
    [SerializeField] float maxSize = 1;

    [SerializeField] private float zoomRate = 5;

    private bool onObj = false;

    private void Start() {
        SetZoom(0.25f);
    }

    private void Update() {
        float scrollWheel = -Mouse.current.scroll.ReadValue().y;

        if (onObj && scrollWheel != 0) {
            ChangeZoom(scrollWheel);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        onObj = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        onObj = false;
    }

    public void OnDisable() {
        onObj = false;
    }

    private void ChangeZoom(float scrollWheel) {
        float rate = 1 + zoomRate * Time.unscaledDeltaTime;
        if (scrollWheel > 0) {
            SetZoom(Mathf.Clamp(transform.localScale.y / rate, minSize, maxSize));
        } else {
            SetZoom(Mathf.Clamp(transform.localScale.y * rate, minSize, maxSize));
        }
    }

    private void SetZoom(float targetSize) {
        transform.localScale = new Vector3(targetSize, targetSize, 1);
    }
}