using UnityEngine;

public class DropItem : MonoBehaviour
{
    [SerializeField] private ItemStack items;
    [SerializeField] private float pickupProximity = 2;
    [SerializeField] private static GameObject player = default;
    private SphericalRigidbody rb;
    [SerializeField] private float decayTime = 60;

    [SerializeField] private Color[] colors = default;
    [SerializeField] private Material[] materials = default;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        rb = GetComponent<SphericalRigidbody>();
        Destroy(this.gameObject, decayTime);
    }

    private void FixedUpdate()
    {
        rb.CalculateVelocity(Vector3.zero, 5);
        transform.Rotate(10 * Vector3.up * Time.fixedDeltaTime);
        if (Vector3.Distance(player.transform.position, this.transform.position) <= pickupProximity)
        {
            PickupItem();
        }
    }

    public void SetItemStack(ushort id, int amount)
    {
        if (id == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            items = new ItemStack(id, amount);
            GetComponent<Light>().color = colors[items.id];
            GetComponent<MeshRenderer>().material = materials[items.id];
        }
    }

    private void PickupItem()
    {
        bool success = player.GetComponent<Player>().inventory.GetComponent<Inventory>().TryAdd(ref items);
        if (success)
        {
            Destroy(this.gameObject);
        }
    }
}