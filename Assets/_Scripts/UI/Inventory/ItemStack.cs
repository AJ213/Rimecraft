[System.Serializable]
public class ItemStack
{
    public ushort id;
    public static readonly int maxSize = 100;
    public int amount;

    public ItemStack(ushort id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}