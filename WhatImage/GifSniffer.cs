using System;
using System.IO;

namespace WhatImage
{
    public class GifSniffer : IImageSniffer
    {
        private readonly byte[] GIF = {0x47, 0x49, 0x46}; // "GIF"

        public bool CanSniff(byte[] imageData)
        {
            if (imageData.Length >= GIF.Length &&
                BinaryHelpers.CompareBytes(imageData, 0, GIF, 0, 3))
            {
                return true;
            }
            return false;
        }

        public ImageMetadata SniffMetadata(byte[] imageData)
        {
            if (!CanSniff(imageData))
                throw new ArgumentException("Can't sniff this image format");

            using (var ms = new MemoryStream(imageData))
            {
                var imageReader = new BinaryReader(ms);
                var imageMetadata = ParseHeader(imageReader);
                return imageMetadata;
            }
        }

        private ImageMetadata ParseHeader(BinaryReader imageReader)
        {
            var imageMetadata = new ImageMetadata(ImageFormat.Gif);

            // "GIF" + Version, discard
            var gif = imageReader.ReadBytes(3);
            if (gif.Length != 3)
                return imageMetadata;
            var version = imageReader.ReadBytes(3);
            if (version.Length != 3)
                return imageMetadata;

            var width = imageReader.ReadBytes(2);
            if (width.Length != 2)
                return imageMetadata;
            var height = imageReader.ReadBytes(2);
            if (height.Length != 2)
                return imageMetadata;

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(width);
                Array.Reverse(height);
            }
            imageMetadata.Width = BitConverter.ToUInt16(width, 0);
            imageMetadata.Height = BitConverter.ToUInt16(height, 0);

            return imageMetadata;
        }
    }
}