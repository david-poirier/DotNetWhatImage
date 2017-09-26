using System.IO;
using WhatImage;
using Xunit;

namespace WhatImageTests
{
    public class PngSnifferTest
    {
        private readonly PngSniffer _sniffer;

        public PngSnifferTest()
        {
            _sniffer = new PngSniffer();
        }

        [Fact]
        public void GoodSignatureTest()
        {
            var sig = new byte[8];
            sig[0] = 0x89;
            sig[1] = 0x50;
            sig[2] = 0x4e;
            sig[3] = 0x47;
            sig[4] = 0x0d;
            sig[5] = 0x0a;
            sig[6] = 0x1a;
            sig[7] = 0x0a;
            Assert.Equal(true, _sniffer.CanSniff(sig));
        }

        [Fact]
        public void BadSignatureTest()
        {
            var sig = new byte[8];
            sig[0] = 0x89;
            sig[1] = 0x51; // bad
            sig[2] = 0x4e;
            sig[3] = 0x47;
            sig[4] = 0x0d;
            sig[5] = 0x0a;
            sig[6] = 0x1a;
            sig[7] = 0x0a;
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
            Assert.Equal(212, metadata.Height);
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
            using (var file = File.OpenRead("../../../Images/MARBLE24.PNG"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }

        private byte[] GetFileBytesMissingHeader()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/MARBLE24 (missing header).PNG"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }

    }
}