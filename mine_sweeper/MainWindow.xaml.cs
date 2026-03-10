using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using mine_sweeper.render;
using mine_sweeper.world;

namespace mine_sweeper
{
    public partial class MainWindow : Window
    {
        private World _world;
        private Render _render;
        private Camera _camera;
        private Inventory _inventory;
        private const int TileSize = 16;

        private int _playerX;
        private int _playerY;
        private bool _inventoryOpen;

        private ShopOffer[] _shopOffers;
        private Border[] _slotBorders;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            int TileX, TileY;
            TileX = (int)area.ActualWidth / TileSize;
            TileY = (int)area.ActualHeight / TileSize;
            _world = new World(TileX, TileY);

            _render = new Render(area, uiOverlay);
            _render.LoadSprites();

            _playerX = TileX / 2;
            _playerY = TileY / 2;

            _render.InitializeGrid(_world, _playerX, _playerY);

            _camera = new Camera(area, TileX, TileY);
            _camera.SetCords(_playerX, _playerY);

            _render.OnMineRequested += MineResource;
            _render.UpdateMineButtons(_world, _camera);

            _inventory = new Inventory();
            InitInventoryUI();
            InitShop();

            this.KeyDown += OnKeyDown;
            this.Focus();
        }

        private void InitInventoryUI()
        {
            _slotBorders = new Border[Inventory.SlotCount];
            for (int i = 0; i < Inventory.SlotCount; i++)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(180, 60, 60, 60)),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(200, 120, 120, 120)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2),
                    Child = new TextBlock
                    {
                        Text = "",
                        Foreground = Brushes.White,
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    }
                };
                _slotBorders[i] = border;
                inventoryGrid.Children.Add(border);
            }
        }

        private void InitShop()
        {
            _shopOffers = new ShopOffer[]
            {
                new ShopOffer
                {
                    Name = "Mining Power",
                    Description = "+1 ore per mine",
                    CostType = ResourceType.Stone,
                    CostAmount = 20,
                    Apply = inv => inv.MiningPower++
                },
                new ShopOffer
                {
                    Name = "Mining Range",
                    Description = "+1 tile reach",
                    CostType = ResourceType.Iron,
                    CostAmount = 15,
                    Apply = inv => inv.MiningRange++
                },
                new ShopOffer
                {
                    Name = "Luck",
                    Description = "x1.5 bonus drops",
                    CostType = ResourceType.Gold,
                    CostAmount = 8,
                    Apply = inv => inv.LuckMultiplier += 0.5
                }
            };

            foreach (var offer in _shopOffers)
            {
                var btn = new Button
                {
                    Focusable = false,
                    Margin = new Thickness(0, 0, 0, 6),
                    Padding = new Thickness(6, 4, 6, 4),
                    Background = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Content = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 10,
                        Foreground = Brushes.White,
                        Text = $"{offer.Name}\n{offer.Description}\nCost: {offer.CostAmount} {offer.CostType}"
                    }
                };

                var capturedOffer = offer;
                btn.Click += (s, e) => BuyUpgrade(capturedOffer);
                shopPanel.Children.Add(btn);
            }
        }

        private void BuyUpgrade(ShopOffer offer)
        {
            if (_inventory.RemoveItem(offer.CostType, offer.CostAmount))
            {
                offer.Apply(_inventory);
                RefreshInventoryUI();
                RefreshMineButtons();
            }
        }

        private void RefreshInventoryUI()
        {
            for (int i = 0; i < Inventory.SlotCount; i++)
            {
                var slot = _inventory.Slots[i];
                var tb = (TextBlock)_slotBorders[i].Child;
                if (slot.IsEmpty)
                {
                    tb.Text = "";
                    _slotBorders[i].Background = new SolidColorBrush(Color.FromArgb(180, 60, 60, 60));
                }
                else
                {
                    tb.Text = $"{slot.Type}\n{slot.Count}";
                    _slotBorders[i].Background = slot.Type switch
                    {
                        ResourceType.Stone => new SolidColorBrush(Color.FromArgb(180, 100, 100, 100)),
                        ResourceType.Iron => new SolidColorBrush(Color.FromArgb(180, 140, 120, 90)),
                        ResourceType.Gold => new SolidColorBrush(Color.FromArgb(180, 160, 140, 40)),
                        ResourceType.Diamond => new SolidColorBrush(Color.FromArgb(180, 60, 160, 180)),
                        _ => new SolidColorBrush(Color.FromArgb(180, 60, 60, 60))
                    };
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_world == null || _camera == null || _render == null) return;

            if (e.Key == Key.Tab)
            {
                _inventoryOpen = !_inventoryOpen;
                inventoryPanel.Visibility = _inventoryOpen ? Visibility.Visible : Visibility.Collapsed;
                if (_inventoryOpen) RefreshInventoryUI();
                e.Handled = true;
                return;
            }

            if (_inventoryOpen) return;

            if (e.Key == Key.E)
            {
                MineNearestResource();
                return;
            }

            int newX = _playerX;
            int newY = _playerY;
            bool moved = false;

            switch (e.Key)
            {
                case Key.W: case Key.Up: newY--; moved = true; break;
                case Key.S: case Key.Down: newY++; moved = true; break;
                case Key.A: case Key.Left: newX--; moved = true; break;
                case Key.D: case Key.Right: newX++; moved = true; break;
                default: return;
            }

            if (moved &&
                newX >= 0 && newX < _world.Width &&
                newY >= 0 && newY < _world.Height &&
                _world.GetTile(newX, newY).IsWalkable)
            {
                _playerX = newX;
                _playerY = newY;

                _render.DrawPlayer(_playerX, _playerY, _world);
                _camera.SetCords(_playerX, _playerY);
                RefreshMineButtons();
            }
        }

        private void RefreshMineButtons()
        {
            _render.UpdateMineButtons(_world, _camera, _inventory.MiningRange);
        }

        private void MineNearestResource()
        {
            int range = _inventory.MiningRange;
            for (int dist = 1; dist <= range; dist++)
            {
                int[][] offsets = { new[] {0,-dist}, new[] {0,dist}, new[] {-dist,0}, new[] {dist,0} };
                foreach (var off in offsets)
                {
                    int tx = _playerX + off[0];
                    int ty = _playerY + off[1];
                    if (tx >= 0 && tx < _world.Width && ty >= 0 && ty < _world.Height)
                    {
                        Tile tile = _world.GetTile(tx, ty);
                        if (tile.Resource != null)
                        {
                            MineResource(tx, ty);
                            return;
                        }
                    }
                }
            }
        }

        private void MineResource(int tileX, int tileY)
        {
            Tile tile = _world.GetTile(tileX, tileY);
            if (tile.Resource == null) return;

            int yield = _inventory.MiningPower;
            int actual = System.Math.Min(yield, tile.Resource.RemainingAmount);

            // Apply luck multiplier for bonus
            int bonus = 0;
            if (_inventory.LuckMultiplier > 1.0)
            {
                var rng = new System.Random();
                if (rng.NextDouble() < (_inventory.LuckMultiplier - 1.0))
                    bonus = 1;
            }

            _inventory.AddItem(tile.Resource.Type, actual + bonus);
            tile.Resource.RemainingAmount -= actual;

            if (tile.Resource.RemainingAmount <= 0)
            {
                tile.Resource = null;
                tile.IsWalkable = true;
                _render.RedrawTerrain(_world);
            }

            RefreshMineButtons();
            if (_inventoryOpen) RefreshInventoryUI();
        }
    }
}
