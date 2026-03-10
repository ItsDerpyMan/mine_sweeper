using System;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
namespace mine_sweeper.world
{

	public class World
	{
		public Tile[,] Tile { get; set; }
        public int Width, Height;
		public World(int width, int height)
		{   
            this.Tile = new Tile[width, height];
            this.Width = width;
            this.Height = height;
            this.GenRandom(width * height);
		}
        private void GenRandom(int seed)
        {
            Random rng = new Random(seed);
            int width = Tile.GetLength(0);
            int height = Tile.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile[x, y] = new Tile
                    {
                        Type = TileType.Grass,
                        IsWalkable = true,
                        Resource = null
                    };
                }
            }

            var resourceTable = new (ResourceType type, double spawnChance)[]
            {
                (ResourceType.Stone,   0.08),
                (ResourceType.Iron,    0.04),
                (ResourceType.Gold,    0.02),
                (ResourceType.Diamond, 0.005)
            };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Tile[x, y].Type != TileType.Grass) continue;
                    if (Tile[x, y].Resource != null) continue;

                    foreach (var (type, chance) in resourceTable)
                    {
                        if (rng.NextDouble() < chance)
                        {
                            Tile[x, y].Resource = new ResourceNode
                            {
                                Type = type,
                                RemainingAmount = type switch
                                {
                                    ResourceType.Stone => rng.Next(5, 15),
                                    ResourceType.Iron => rng.Next(3, 10),
                                    ResourceType.Gold => rng.Next(2, 6),
                                    ResourceType.Diamond => rng.Next(1, 3),
                                    _ => 5
                                },
                                RespawnTimer = 0
                            };
                            Tile[x, y].IsWalkable = false;
                            break;
                        }
                    }
                }
            }
        }
        public Tile GetTile(int x, int y)
        {
            return Tile[x, y];
        }
    }


	public enum TileType { Grass, Path }
	public class Tile
	{
		public TileType Type { get; set; }
		public ResourceNode? Resource { get; set; }
		//public Villager? NPC { get; set; }
		public bool IsWalkable { get; set; }

	}

	public enum ResourceType { Stone, Iron, Gold, Diamond }
	public class ResourceNode
	{
		public ResourceType Type { get; set; }
		public int RemainingAmount { get; set; }
		public int RespawnTimer { get; set; }

	}
}