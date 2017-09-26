using System;
using System.IO;
using System.Reflection.PortableExecutable;
using WhatImage;
using Xunit;


namespace WhatImageTests
{
    public class JpegSnifferTest
    {
        private readonly JpegSniffer _sniffer;

        public JpegSnifferTest()
        {
            _sniffer = new JpegSniffer();
        }

        [Fact]
        public void GoodSignatureTest()
        {
            var sig = new byte[3];
            sig[0] = 0xff;
            sig[1] = 0xd8;
            sig[2] = 0xff;
            Assert.Equal(true, _sniffer.CanSniff(sig));
        }

        [Fact]
        public void BadSignatureTest()
        {
            var sig = new byte[3];
            sig[0] = 0xff;
            sig[1] = 0xd7; //bad
            sig[2] = 0xff;
            Assert.Equal(false, _sniffer.CanSniff(sig));
        }

        [Fact]
        public void FileSignatureTest()
        {
            Assert.Equal(true, _sniffer.CanSniff(GetFileBytes()));
        }

        [Fact]
        public void FileMetadataTest()
        {
            var metadata = _sniffer.SniffMetadata(GetFileBytes());

            Assert.Equal(300, metadata.Width);
            Assert.Equal(283, metadata.Height);
        }

        [Fact]
        public void FileMissingHeaderMetadataTest()
        {
            var metadata = _sniffer.SniffMetadata(GetFileBytesMissingHeader());

            Assert.Equal(0, metadata.Width);
            Assert.Equal(0, metadata.Height);
        }

        public static byte[] GetFileBytes()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/rosette_gendler_big.jpg"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }

        private byte[] GetFileBytesMissingHeader()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/rosette_gendler_big (missing header).jpg"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }

    }
}