using System;
using System.IO;

namespace WhatImage
{
    public class PngSniffer : IImageSniffer
    {
        private static readonly byte[] StartOfFile = {0x89, 0x50};

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

        private ImageMetadata ParseHeader(BinaryReader imageReader)
        {
            var imageMetadata = new ImageMetadata(ImageFormat.Png);

            // File Identifier, discard
            imageReader.ReadBytes(8);

            while (true)
            {
                var chunk = ParseChunk(imageReader);
                if (chunk == null)
                    break;

                if (chunk.IsIHDR)
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

            var length = imageReader.ReadBytes(4);
            if (length.Length != 4)
                return null;
            chunk.Length = BytesToUInt(length);

            var type = imageReader.ReadChars(4);
            if (type.Length != 4)
                return null;
            chunk.Type = new String(type);

            var startPosition = imageReader.BaseStream.Position;
            var nextChunkPosition = startPosition + chunk.Length;

            if (chunk.Type == "IHDR")
            {
                chunk.IsIHDR = true;

                var width = imageReader.ReadBytes(4);
                if (width.Length != 4)
                    return null;
                chunk.Width = BytesToInt(width);
                var height = imageReader.ReadBytes(4);
                if (height.Length != 4)
                    return null;
                chunk.Height = BytesToInt(height);
            }

            if (nextChunkPosition <= imageReader.BaseStream.Length)
                imageReader.BaseStream.Position = nextChunkPosition;
            else
                imageReader.BaseStream.Position = imageReader.BaseStream.Length;
            return chunk;
        }

        private uint BytesToUInt(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            var result = BitConverter.ToUInt32(data, 0);
            return result;
        }

        private int BytesToInt(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            var result = BitConverter.ToInt32(data, 0);
            return result;
        }

        private class Chunk
        {
            public uint Length;
            public string Type;
            public bool IsIHDR;
            public int Width;
            public int Height;
        }
    }
}