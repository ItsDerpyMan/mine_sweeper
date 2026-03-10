using mine_sweeper.world;

namespace mine_sweeper.world
{
    public class Inventory
    {
        public const int SlotCount = 16;
        public InventorySlot[] Slots { get; } = new InventorySlot[SlotCount];

        public int MiningPower { get; set; } = 1;
        public int MiningRange { get; set; } = 1;
        public double LuckMultiplier { get; set; } = 1.0;

        public Inventory()
        {
            for (int i = 0; i < SlotCount; i++)
                Slots[i] = new InventorySlot();
        }

        public bool AddItem(ResourceType type, int amount = 1)
        {
            // Try to stack on existing slot of same type
            for (int i = 0; i < SlotCount; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].Type == type)
                {
                    Slots[i].Count += amount;
                    return true;
                }
            }

            // Try first empty slot
            for (int i = 0; i < SlotCount; i++)
            {
                if (Slots[i].IsEmpty)
                {
                    Slots[i].Type = type;
                    Slots[i].Count = amount;
                    return true;
                }
            }

            return false; // inventory full
        }

        public int GetCount(ResourceType type)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].Type == type)
                    return Slots[i].Count;
            }
            return 0;
        }

        public bool RemoveItem(ResourceType type, int amount)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].Type == type && Slots[i].Count >= amount)
                {
                    Slots[i].Count -= amount;
                    if (Slots[i].Count <= 0)
                        Slots[i].Clear();
                    return true;
                }
            }
            return false;
        }
    }

    public class InventorySlot
    {
        public ResourceType Type { get; set; }
        public int Count { get; set; }
        public bool IsEmpty => Count <= 0;

        public void Clear()
        {
            Count = 0;
        }
    }

    public class ShopOffer
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public ResourceType CostType { get; set; }
        public int CostAmount { get; set; }
        public System.Action<Inventory> Apply { get; set; } = _ => { };
    }
}
