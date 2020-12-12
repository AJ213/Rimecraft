using UnityEngine;

[System.Serializable]
public class Toolbar : MonoBehaviour
{
    public InventoryRow inventory;
    public RectTransform highlight;
    public Player player;
    public int slotIndex = 0;

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            player.playerSounds.Play("ToolbarSwap");
            if (scroll > 0)
            {
                slotIndex--;
            }
            else
            {
                slotIndex++;
            }

            if (slotIndex > inventory.slots.Length - 1)
            {
                slotIndex = 0;
            }

            if (slotIndex < 0)
            {
                slotIndex = inventory.slots.Length - 1;
            }

            highlight.position = inventory.slots[slotIndex].slotIcon.transform.position;
        }
    }
}