using System;
using Marketplace.Framework;

namespace Marketplace.Domain
{
    public class PictureSize : Value<PictureSize>
    {
        public int Width { get; }
        public int Height { get; }

        public PictureSize(int width, int height)
        {
            if (Width <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    "Picture width must be a positive number");

            if (Height <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(height),
                    "Picture height must be a positive number");

            Width = width;
            Height = height;
        }

        internal PictureSize() { }
    }
}