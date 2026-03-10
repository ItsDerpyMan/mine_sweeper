using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mine_sweeper.render
{
    public class Camera
    {
        private const int TileSize = 16;
        public double PosX { get; private set; }
        public double PosY { get; private set; }

        private double _viewportWidth;
        private double _viewportHeight;

        private int _worldWidth;
        private int _worldHeight;

        private TransformGroup _transformGroup;
        private ScaleTransform _scale;
        private TranslateTransform _translate;

        public Camera(Canvas canvas, int width, int height)
        {
            _viewportWidth = canvas.Width;
            _viewportHeight = canvas.Height;
            _worldWidth = width;
            _worldHeight = height;

            _scale = new ScaleTransform(3, 3);
            _translate = new TranslateTransform();

            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_scale);
            _transformGroup.Children.Add(_translate);

            canvas.RenderTransform = _transformGroup;

            RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

        }
        public double Scale => 3;

        public void SetCords(int x, int y)
        {
            PosX = x; PosY = y;
            Update();
        }

        public Point WorldToScreen(double tileX, double tileY)
        {
            double screenX = tileX * TileSize * Scale + _translate.X;
            double screenY = tileY * TileSize * Scale + _translate.Y;
            return new Point(screenX, screenY);
        }

        private void Update()
        {
            double pixelX = (PosX + 0.5) * TileSize;
            double pixelY = (PosY + 0.5) * TileSize;

            double offsetX = (_viewportWidth / 2.0) - (pixelX * 3);
            double offsetY = (_viewportHeight / 2.0) - (pixelY * 3);

            double maxOffsetX = 0;
            double maxOffsetY = 0;
            double minOffsetX = _viewportWidth - (_worldWidth * TileSize * 3);
            double minOffsetY = _viewportHeight - (_worldHeight * TileSize * 3);

            offsetX = Math.Clamp(offsetX, minOffsetX, maxOffsetX);
            offsetY = Math.Clamp(offsetY, minOffsetY, maxOffsetY);

            _translate.X = offsetX;
            _translate.Y = offsetY;
        }
    }
}
