using System;
using System.Collections.Generic;
using mine_sweeper.world;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace mine_sweeper.render
{
    public class VisualHost : FrameworkElement
    {
        private readonly List<Visual> _visuals = new();
        private readonly Size _size;

        public VisualHost(double width, double height)
        {
            _size = new Size(width, height);
        }

        public void AddVisual(Visual visual)
        {
            _visuals.Add(visual);
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];
        protected override Size MeasureOverride(Size availableSize) => _size;
    }

    public class Render
    {
        private const int TileSize = 16;
        private Canvas _canvas;
        private Dictionary<string, BitmapSource> _sprites = new();

        private VisualHost _host;
        private DrawingVisual _terrainVisual;
        private DrawingVisual _entityVisual;

        private int _worldWidth;
        private int _worldHeight;
        private int _playerX;
        private int _playerY;

        public Render(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void LoadSprites()
        {
            string basePath = "pack://application:,,,/Assets/";
            string[] names = { "Grass", "Path", "Stone", "Iron", "Gold", "Diamond", "Player" };
            string[] files = { "grass.png", "path.png", "stone.png", "iron.png", "gold.png", "diamond.png", "player.png" };

            for (int i = 0; i < names.Length; i++)
            {
                _sprites[names[i]] = LoadImage($"{basePath}{files[i]}");
            }
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

        public void InitializeGrid(World world, int playerX, int playerY)
        {
            _canvas.Children.Clear();
            _worldWidth = world.Width;
            _worldHeight = world.Height;
            _playerX = playerX;
            _playerY = playerY;

            double pixelWidth = _worldWidth * TileSize;
            double pixelHeight = _worldHeight * TileSize;

            _host = new VisualHost(pixelWidth, pixelHeight);
            RenderOptions.SetBitmapScalingMode(_host, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(_host, EdgeMode.Aliased);

            _terrainVisual = new DrawingVisual();
            _entityVisual = new DrawingVisual();
            _host.AddVisual(_terrainVisual);
            _host.AddVisual(_entityVisual);

            Canvas.SetLeft(_host, 0);
            Canvas.SetTop(_host, 0);
            _canvas.Children.Add(_host);

            DrawGameArea(world);
        }

        public void DrawGameArea(World world)
        {
            using (DrawingContext dc = _terrainVisual.RenderOpen())
            {
                for (int x = 0; x < _worldWidth; x++)
                {
                    for (int y = 0; y < _worldHeight; y++)
                    {
                        Tile tile = world.GetTile(x, y);
                        Rect rect = new Rect(x * TileSize, y * TileSize, TileSize, TileSize);

                        dc.DrawImage(_sprites[tile.Type.ToString()], rect);

                        if (tile.Resource != null)
                        {
                            dc.DrawImage(_sprites[tile.Resource.Type.ToString()], rect);
                        }
                    }
                }
            }

            using (DrawingContext dc = _entityVisual.RenderOpen())
            {
                dc.DrawImage(_sprites["Player"], new Rect(_playerX * TileSize, _playerY * TileSize, TileSize, TileSize));
            }
        }

        public void DrawPlayer(int x, int y, World world)
        {
            _playerX = x;
            _playerY = y;

            using (DrawingContext dc = _entityVisual.RenderOpen())
            {
                dc.DrawImage(_sprites["Player"], new Rect(x * TileSize, y * TileSize, TileSize, TileSize));
            }
        }
    }
}
