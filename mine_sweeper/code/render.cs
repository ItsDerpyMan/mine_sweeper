using System;
using System.Collections.Generic;
using mine_sweeper.world;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace mine_sweeper.render
{
    public class Render
    {
        private const int TileSize = 16;
        private Canvas _canvas;
        private Dictionary<string, byte[]> _spritePixels = new();
        private WriteableBitmap _backBuffer;
        private Image _display;
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
                var bmp = LoadImage($"{basePath}{files[i]}");
                // Convert to BGRA pixel array for fast blitting
                var converted = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
                byte[] pixels = new byte[TileSize * TileSize * 4];
                converted.CopyPixels(pixels, TileSize * 4, 0);
                _spritePixels[names[i]] = pixels;
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

            int pixelWidth = _worldWidth * TileSize;
            int pixelHeight = _worldHeight * TileSize;

            _backBuffer = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgra32, null);

            _display = new Image
            {
                Width = pixelWidth,
                Height = pixelHeight,
                SnapsToDevicePixels = true,
                Source = _backBuffer
            };
            RenderOptions.SetBitmapScalingMode(_display, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(_display, 0);
            Canvas.SetTop(_display, 0);
            _canvas.Children.Add(_display);

            DrawGameArea(world);
        }

        public void DrawGameArea(World world)
        {
            _backBuffer.Lock();
            try
            {
                int stride = _backBuffer.BackBufferStride;
                IntPtr buffer = _backBuffer.BackBuffer;

                for (int x = 0; x < _worldWidth; x++)
                {
                    for (int y = 0; y < _worldHeight; y++)
                    {
                        Tile tile = world.GetTile(x, y);

                        // Draw terrain
                        BlitSprite(buffer, stride, x, y, _spritePixels[tile.Type.ToString()]);

                        // Draw resource on top (alpha-blended)
                        if (tile.Resource != null)
                        {
                            BlitSpriteAlpha(buffer, stride, x, y, _spritePixels[tile.Resource.Type.ToString()]);
                        }
                    }
                }

                // Draw player on top
                BlitSpriteAlpha(buffer, stride, _playerX, _playerY, _spritePixels["Player"]);

                _backBuffer.AddDirtyRect(new Int32Rect(0, 0, _backBuffer.PixelWidth, _backBuffer.PixelHeight));
            }
            finally
            {
                _backBuffer.Unlock();
            }
        }

        public void DrawPlayer(int x, int y, World world)
        {
            int oldX = _playerX;
            int oldY = _playerY;
            _playerX = x;
            _playerY = y;

            _backBuffer.Lock();
            try
            {
                int stride = _backBuffer.BackBufferStride;
                IntPtr buffer = _backBuffer.BackBuffer;

                // Redraw old player position (terrain + possible resource)
                RedrawTile(buffer, stride, oldX, oldY, world);

                // Redraw new player position (terrain + possible resource + player)
                RedrawTile(buffer, stride, x, y, world);
                BlitSpriteAlpha(buffer, stride, x, y, _spritePixels["Player"]);

                // Mark only the changed regions as dirty
                _backBuffer.AddDirtyRect(new Int32Rect(oldX * TileSize, oldY * TileSize, TileSize, TileSize));
                _backBuffer.AddDirtyRect(new Int32Rect(x * TileSize, y * TileSize, TileSize, TileSize));
            }
            finally
            {
                _backBuffer.Unlock();
            }
        }

        private void RedrawTile(IntPtr buffer, int stride, int tileX, int tileY, World world)
        {
            Tile tile = world.GetTile(tileX, tileY);
            BlitSprite(buffer, stride, tileX, tileY, _spritePixels[tile.Type.ToString()]);
            if (tile.Resource != null)
            {
                BlitSpriteAlpha(buffer, stride, tileX, tileY, _spritePixels[tile.Resource.Type.ToString()]);
            }
        }

        // Opaque blit — overwrites destination pixels entirely
        private unsafe void BlitSprite(IntPtr buffer, int stride, int tileX, int tileY, byte[] spritePixels)
        {
            int startX = tileX * TileSize;
            int startY = tileY * TileSize;

            fixed (byte* srcPtr = spritePixels)
            {
                for (int row = 0; row < TileSize; row++)
                {
                    byte* dst = (byte*)buffer + (startY + row) * stride + startX * 4;
                    byte* src = srcPtr + row * TileSize * 4;
                    Buffer.MemoryCopy(src, dst, TileSize * 4, TileSize * 4);
                }
            }
        }

        // Alpha-blended blit — respects source alpha channel
        private unsafe void BlitSpriteAlpha(IntPtr buffer, int stride, int tileX, int tileY, byte[] spritePixels)
        {
            int startX = tileX * TileSize;
            int startY = tileY * TileSize;

            fixed (byte* srcPtr = spritePixels)
            {
                for (int row = 0; row < TileSize; row++)
                {
                    byte* dst = (byte*)buffer + (startY + row) * stride + startX * 4;
                    byte* src = srcPtr + row * TileSize * 4;

                    for (int col = 0; col < TileSize; col++)
                    {
                        int si = col * 4;
                        int di = col * 4;
                        byte sa = src[si + 3];

                        if (sa == 255)
                        {
                            // Fully opaque — direct copy
                            dst[di] = src[si];
                            dst[di + 1] = src[si + 1];
                            dst[di + 2] = src[si + 2];
                            dst[di + 3] = 255;
                        }
                        else if (sa > 0)
                        {
                            // Semi-transparent — blend
                            byte da = dst[di + 3];
                            int invSa = 255 - sa;
                            dst[di] = (byte)((src[si] * sa + dst[di] * invSa) / 255);
                            dst[di + 1] = (byte)((src[si + 1] * sa + dst[di + 1] * invSa) / 255);
                            dst[di + 2] = (byte)((src[si + 2] * sa + dst[di + 2] * invSa) / 255);
                            dst[di + 3] = (byte)(sa + (da * invSa) / 255);
                        }
                        // sa == 0: fully transparent, skip
                    }
                }
            }
        }
    }
}
