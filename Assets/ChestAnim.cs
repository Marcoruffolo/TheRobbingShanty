using UnityEngine;

public class ChestAnim : MonoBehaviour
{
    public Animator animator;


    public ChestInventory inventory;

    private void Awake()
    {
        inventory.OpenChestInventory += OpenChest;
    }

    private void OnDisable()
    {
        inventory.OpenChestInventory -= OpenChest;
    }

    public void OpenChest(bool open) 
    {
        animator.SetBool("Open", open);
    }
}
