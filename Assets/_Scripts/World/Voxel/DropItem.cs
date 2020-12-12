using UnityEngine;

public class DropItem : MonoBehaviour
{
    [SerializeField] private ItemStack items;
    [SerializeField] private float pickupProximity = 2;
    [SerializeField] private static GameObject player = default;
    private SphericalRigidbody rb;
    [SerializeField] private float decayTime = 60;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        Color color = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        GetComponent<Light>().color = color;
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
        items = new ItemStack(id, amount);
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