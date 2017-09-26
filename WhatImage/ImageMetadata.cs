namespace WhatImage
{
    public class ImageMetadata
    {
        public ImageMetadata(ImageFormat format)
        {
            Format = format;
            Width = 0;
            Height = 0;
        }

        public ImageFormat Format { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}