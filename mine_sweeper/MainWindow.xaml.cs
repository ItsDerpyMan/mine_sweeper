using System.Windows;
using System.Windows.Input;
using mine_sweeper.render;
using mine_sweeper.world;

namespace mine_sweeper
{
    public partial class MainWindow : Window
    {
        private World _world;
        private Render _render;
        private Camera _camera;
        private const int TileSize = 16;

        private int _playerX;
        private int _playerY;

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

            _render = new Render(area);
            _render.LoadSprites();

            _playerX = TileX / 2;
            _playerY = TileY / 2;

            _render.InitializeGrid(_world, _playerX, _playerY);

            _camera = new Camera(area, TileX, TileY);
            _camera.SetCords(_playerX, _playerY);

            this.KeyDown += OnKeyDown;
            this.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_world == null || _camera == null || _render == null) return;

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
            }
        }
    }
}
