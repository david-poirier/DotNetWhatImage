using System;
using System.IO;


namespace WhatImage
{
    public class JpegSniffer : IImageSniffer
    {
        private static readonly byte[] StartOfFile = {0xff, 0xd8};

        public bool CanSniff(byte[] imageData)
        {
            if (imageData.Length >= StartOfFile.Length &&
                BinaryHelpers.CompareBytes(imageData, 0, StartOfFile, 0, 2))
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

        public ImageMetadata ParseHeader(BinaryReader imageReader)
        {
            var imageMetadata = new ImageMetadata(ImageFormat.Jpeg);

            // Start of Image, discard
            imageReader.ReadBytes(2);

            // Scan segments for Start of Frame
            while (true)
            {
                var segment = ParseSegment(imageReader);
                if (segment == null)
                    break;

                if (segment.IsStartOfFrame)
                {
                    imageMetadata.Width = segment.Width;
                    imageMetadata.Height = segment.Height;
                    break;
                }
            }

            return imageMetadata;
        }

        private Segment ParseSegment(BinaryReader imageReader)
        {
            if (imageReader.BaseStream.Position == imageReader.BaseStream.Length)
                return null;

            var segment = new Segment();
            var marker = imageReader.ReadBytes(2);
            if (marker.Length != 2 || marker[0] != 0xff)
                return null;
            segment.Marker = marker;

            var startPosition = imageReader.BaseStream.Position;
            var length = imageReader.ReadBytes(2);
            if (length.Length != 2)
                return null;
            segment.Length = BytesToShort(length);
            var nextSegmentPosition = startPosition + segment.Length;

            // check for Start of Frame marker, c0 to c2
            if (segment.Marker[1] >= 0xc0 &&
                segment.Marker[1] <= 0xc2)
            {
                segment.IsStartOfFrame = true;
                imageReader.ReadByte(); // Data Precision, discard
                var height = imageReader.ReadBytes(2);
                if (height.Length != 2)
                    return null;
                segment.Height = BytesToShort(height);
                var width = imageReader.ReadBytes(2);
                if (width.Length != 2)
                    return null;
                segment.Width = BytesToShort(width);
            }

            if (nextSegmentPosition <= imageReader.BaseStream.Length)
                imageReader.BaseStream.Position = nextSegmentPosition;
            else
                imageReader.BaseStream.Position = imageReader.BaseStream.Length;
            return segment;
        }

        private short BytesToShort(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            var result = BitConverter.ToInt16(data, 0);
            return result;
        }

        private class Segment
        {
            public byte[] Marker;
            public short Length;
            public bool IsStartOfFrame;
            public short Width;
            public short Height;
        }
    }
}