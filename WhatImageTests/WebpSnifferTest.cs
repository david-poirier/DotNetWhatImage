using System.IO;
using WhatImage;
using Xunit;

namespace WhatImageTests
{
    public class WebpSnifferTest
    {
        private readonly WebpSniffer _sniffer;

        public WebpSnifferTest()
        {
            _sniffer = new WebpSniffer();
        }

        [Fact]
        public void GoodSignatureTest()
        {
            var sig = new byte[12];
            // "RIFF"
            sig[0] = 0x52;
            sig[1] = 0x49;
            sig[2] = 0x46;
            sig[3] = 0x46;
            // Size - uint
            sig[4] = 0x00;
            sig[5] = 0x00;
            sig[6] = 0x00;
            sig[7] = 0x00;
            // "WEBP"
            sig[8] = 0x57;
            sig[9] = 0x45;
            sig[10] = 0x42;
            sig[11] = 0x50;

            Assert.Equal(true, _sniffer.CanSniff(sig));
        }

        [Fact]
        public void BadSignatureTest()
        {
            var sig = new byte[12];
            // "RIFG"
            sig[0] = 0x52;
            sig[1] = 0x49;
            sig[2] = 0x46;
            sig[3] = 0x47; // bad!
            // Size - uint
            sig[4] = 0x00;
            sig[5] = 0x00;
            sig[6] = 0x00;
            sig[7] = 0x00;
            // "WEBP"
            sig[8] = 0x57;
            sig[9] = 0x45;
            sig[10] = 0x42;
            sig[11] = 0x50;

            Assert.Equal(false, _sniffer.CanSniff(sig));
        }

        [Fact]
        public void SimpleFileSignatureTest()
        {
            Assert.Equal(true, _sniffer.CanSniff(GetSimpleFileBytes()));
        }

        [Fact]
        public void SimpleFileMetadataTest()
        {
            var metadata = _sniffer.SniffMetadata(GetSimpleFileBytes());

            Assert.Equal(550, metadata.Width);
            Assert.Equal(368, metadata.Height);
        }

        public static byte[] GetSimpleFileBytes()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/1.webp"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }

        [Fact]
        public void ComplexFileSignatureTest()
        {
            Assert.Equal(true, _sniffer.CanSniff(GetComplexFileBytes()));
        }

        [Fact]
        public void ComplexFileMetadataTest()
        {
            var metadata = _sniffer.SniffMetadata(GetComplexFileBytes());

            Assert.Equal(400, metadata.Width);
            Assert.Equal(301, metadata.Height);
        }

        public static byte[] GetComplexFileBytes()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/1_webp_a.webp"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }

        [Fact]
        public void LosslessFileSignatureTest()
        {
            Assert.Equal(true, _sniffer.CanSniff(GetLosslessFileBytes()));
        }

        [Fact]
        public void LosslessFileMetadataTest()
        {
            var metadata = _sniffer.SniffMetadata(GetLosslessFileBytes());

            Assert.Equal(400, metadata.Width);
            Assert.Equal(301, metadata.Height);
        }

        public static byte[] GetLosslessFileBytes()
        {
            var buffer = new byte[8192];
            using (var file = File.OpenRead("../../../Images/1_webp_ll.webp"))
            {
                file.Read(buffer, 0, 8192);
            }
            return buffer;
        }
    }
}