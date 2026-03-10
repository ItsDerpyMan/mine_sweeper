using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mine_sweeper.world;

namespace mine_sweeper.render
{
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows;

    public class Render
    {
        private const int TileSize = 16;
        private Canvas _canvas;
        private Dictionary<string, BitmapImage> _spriteCache = new();
        private Image[,] _terrainLayer;
        private Image[,] _resourceLayer;
        private Image _playerImage;
        private int _worldWidth;
        private int _worldHeight;

        public Render(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void LoadSprites()
        {
            string basePath = "pack://application:,,,/Assets/";
            _spriteCache["Grass"] = LoadImage($"{basePath}grass.png");
            _spriteCache["Path"] = LoadImage($"{basePath}path.png");
            _spriteCache["Stone"] = LoadImage($"{basePath}stone.png");
            _spriteCache["Iron"] = LoadImage($"{basePath}iron.png");
            _spriteCache["Gold"] = LoadImage($"{basePath}gold.png");
            _spriteCache["Diamond"] = LoadImage($"{basePath}diamond.png");
            _spriteCache["Player"] = LoadImage($"{basePath}player.png");
        }

        private BitmapImage LoadImage(string uri)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(uri, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            return img;
        }

        // Call once after world is created
        public void InitializeGrid(World world, int playerX, int playerY)
        {
            _canvas.Children.Clear();
            _worldWidth = world.Width;
            _worldHeight = world.Height;

            _terrainLayer = new Image[_worldWidth, _worldHeight];
            _resourceLayer = new Image[_worldWidth, _worldHeight];

            for (int x = 0; x < _worldWidth; x++)
            {
                for (int y = 0; y < _worldHeight; y++)
                {
                    var terrain = CreateImage();
                    Canvas.SetLeft(terrain, x * TileSize);
                    Canvas.SetTop(terrain, y * TileSize);
                    _canvas.Children.Add(terrain);
                    _terrainLayer[x, y] = terrain;

                    var resource = CreateImage();
                    Canvas.SetLeft(resource, x * TileSize);
                    Canvas.SetTop(resource, y * TileSize);
                    resource.Visibility = Visibility.Collapsed;
                    _canvas.Children.Add(resource);
                    _resourceLayer[x, y] = resource;
                }
            }

            _playerImage = CreateImage();
            _playerImage.Source = _spriteCache["Player"];
            Canvas.SetLeft(_playerImage, playerX * TileSize);
            Canvas.SetTop(_playerImage, playerY * TileSize);
            _canvas.Children.Add(_playerImage);

            DrawGameArea(world);
        }

        private Image CreateImage()
        {
            var img = new Image
            {
                Width = TileSize,
                Height = TileSize,
                SnapsToDevicePixels = true
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
            return img;
        }

        public void DrawGameArea(World world)
        {
            for (int x = 0; x < _worldWidth; x++)
            {
                for (int y = 0; y < _worldHeight; y++)
                {
                    Tile tile = world.GetTile(x, y);

                    _terrainLayer[x, y].Source = _spriteCache[tile.Type.ToString()];

                    if (tile.Resource != null)
                    {
                        _resourceLayer[x, y].Source = _spriteCache[tile.Resource.Type.ToString()];
                        _resourceLayer[x, y].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        _resourceLayer[x, y].Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void DrawPlayer(int x, int y)
        {
            Canvas.SetLeft(_playerImage, x * TileSize);
            Canvas.SetTop(_playerImage, y * TileSize);
        }
    }
}
