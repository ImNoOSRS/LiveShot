﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LiveShot.UI.Objects
{
    public record Selection
    {
        private readonly Rectangle _rectangle;
        private double _left;
        private double _top;

        private Selection(double left, double top, double width, double height)
        {
            _rectangle = new Rectangle
            {
                Fill = Brushes.White,
                Opacity = 0.2,
                Width = width,
                Height = height
            };

            _left = left;
            _top = top;
        }

        public double Left
        {
            get => _left;
            set
            {
                if (value > 0) _left = value;
            }
        }

        public double Top
        {
            get => _top;
            set
            {
                if (value > 0) _top = value;
            }
        }

        public double Width
        {
            get => _rectangle.Width;
            set
            {
                if (value > 0) _rectangle.Width = value;
            }
        }

        public double Height
        {
            get => _rectangle.Height;
            set
            {
                if (value > 0) _rectangle.Height = value;
            }
        }

        public Rectangle Rectangle => _rectangle;

        public static Selection Empty => new Selection(0, 0, 0, 0);

        public bool Contains(Point point)
        {
            return point.X >= _left &&
                   point.X <= _left + _rectangle.Width &&
                   point.Y >= _top &&
                   point.Y <= _top + _rectangle.Height;
        }
    }
}