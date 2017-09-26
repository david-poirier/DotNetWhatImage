using System.IO;
using WhatImage;
using Xunit;

namespace WhatImageTests
{
    public class GifSnifferTest
    {
        private readonly GifSniffer _sniffer;

        public GifSnifferTest()
        {
            _sniffer = new GifSniffer();
        }

        [Fact]
        public void GoodSignatureTest()
        {
            byte[] sig = {0x47, 0x49, 0x46, 0x38, 0x39, 0x61};
            Assert.Equal(true, _sniffer.CanSniff(sig));
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

            Assert.Equal(640, metadata.Width);
            Assert.Equal(200, metadata.Height);
        }

        public static byte[] GetFileBytes()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/WFPC05.GIF"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }
    }
}