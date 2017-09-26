using System;
using System.IO;

namespace WhatImage
{
    public class WebpSniffer : IImageSniffer
    {
        private readonly byte[] RIFF = {0x52, 0x49, 0x46, 0x46}; // "RIFF"
        private readonly byte[] WEBP = {0x57, 0x45, 0x42, 0x50}; // "WEBP"

        public bool CanSniff(byte[] imageData)
        {
            if (imageData.Length >= 12 &&
                BinaryHelpers.CompareBytes(imageData, 0, RIFF, 0, 4) &&
                BinaryHelpers.CompareBytes(imageData, 8, WEBP, 0, 4))
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
            var imageMetadata = new ImageMetadata(ImageFormat.Webp);

            // File Identifier, discard
            imageReader.ReadBytes(12);

            while (true)
            {
                var chunk = ParseChunk(imageReader);
                if (chunk == null)
                    break;

                if (chunk.IsFrameHeader)
                {
                    imageMetadata.Width = chunk.Width;
                    imageMetadata.Height = chunk.Height;
                    break;
                }
            }

            return imageMetadata;
        }

        private Chunk ParseChunk(BinaryReader imageReader)
        {
            if (imageReader.BaseStream.Position == imageReader.BaseStream.Length)
                return null;

            var chunk = new Chunk();

            var fourCC = imageReader.ReadChars(4);
            if (fourCC.Length != 4)
                return null;
            chunk.FourCC = new String(fourCC);

            var size = imageReader.ReadBytes(4);
            if (size.Length != 4)
                return null;
            chunk.Size = BytesToUInt(size);

            var startPosition = imageReader.BaseStream.Position;
            var nextChunkPosition = startPosition + chunk.Size;
            if (chunk.Size % 2 != 0)
                nextChunkPosition++;

            if (chunk.FourCC == "VP8 ")
            {
                // lossy
                chunk.IsFrameHeader = true;
                var skip = imageReader.ReadBytes(6);
                if (skip.Length != 6)
                    return null;

                var width = imageReader.ReadBytes(2);
                if (width.Length != 2)
                    return null;
                // drop 2 bits (scale) off left-hand side
                var shortWidth = BytesToShort(width);
                shortWidth = (short) (shortWidth << 2);
                shortWidth = (short) (shortWidth >> 2);
                chunk.Width = shortWidth;

                var height = imageReader.ReadBytes(2);
                if (height.Length != 2)
                    return null;
                // drop 2 bits (scale) off left-hand side
                var shortHeight = BytesToShort(height);
                shortHeight = (short) (shortHeight << 2);
                shortHeight = (short) (shortHeight >> 2);
                chunk.Height = shortHeight;
            }
            else if (chunk.FourCC == "VP8L")
            {
                // lossless
                chunk.IsFrameHeader = true;
                var skip = imageReader.ReadBytes(1);
                if (skip.Length != 1)
                    return null;

                var sizeBytes = imageReader.ReadBytes(4);
                var stream = new BitStream(sizeBytes);
                var width = stream.ReadShort(14, Endianness.LittleEndian);
                var height = stream.ReadShort(14, Endianness.LittleEndian);
                chunk.Width = width + 1;
                chunk.Height = height + 1;
            }

            if (nextChunkPosition <= imageReader.BaseStream.Length)
                imageReader.BaseStream.Position = nextChunkPosition;
            else
                imageReader.BaseStream.Position = imageReader.BaseStream.Length;
            return chunk;
        }

        private uint BytesToUInt(byte[] data)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            var result = BitConverter.ToUInt32(data, 0);
            return result;
        }

        private short BytesToShort(byte[] data)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            var result = BitConverter.ToInt16(data, 0);
            return result;
        }

        private class Chunk
        {
            public string FourCC;
            public uint Size;
            public bool IsFrameHeader;
            public int Width;
            public int Height;
        }
    }
}