using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;

    public InventorySlot AssignedInventorySlot;

    [Header("Drop Settings")]
    [SerializeField] private Transform playerDropPoint;

    private void Awake()
    {
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    private void Update()
    {
        if (AssignedInventorySlot.ItemData != null)
        {
            transform.position = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())
            {
                DropItem();
            }
        }
    }

    private void DropItem()
    {
        if (AssignedInventorySlot.ItemData == null) return;

        var itemData = AssignedInventorySlot.ItemData;
        int amount = AssignedInventorySlot.StackSize;

        if (itemData.itemPrefab != null && playerDropPoint != null)
        {
            Vector3 basePos = playerDropPoint.position + playerDropPoint.forward * 0.5f;
            for (int i = 0; i < amount; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.2f;
                offset.y = 0f;
                Instantiate(itemData.itemPrefab, basePos + offset, playerDropPoint.rotation);
            }
        }
        else if (itemData.itemPrefab != null)
        {
            Debug.LogWarning("MouseItemData: playerDropPoint not assigned. Spawning at world origin.");
            for (int i = 0; i < amount; i++)
                Instantiate(itemData.itemPrefab, Vector3.zero, Quaternion.identity);
        }

        ClearSlot();
    }

    public void ClearSlot()
    {
        AssignedInventorySlot.ClearSlot();
        itemCount.text = "";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        AssignedInventorySlot.AssignItem(invSlot);
        itemSprite.sprite = invSlot.ItemData.Icon;
        itemCount.text = invSlot.StackSize.ToString();
        itemSprite.color = Color.white;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}