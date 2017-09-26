using WhatImage;
using Xunit;

namespace WhatImageTests
{
    public class MetaDetectorTest
    {
        private readonly MetaDetector _detector;

        public MetaDetectorTest()
        {
            _detector = new MetaDetector();
        }

        [Fact]
        public void SniffGifMetadataTest()
        {
            var metadata = _detector.SniffMetadata(
                GifSnifferTest.GetFileBytes());
            Assert.Equal(ImageFormat.Gif, metadata.Format);
        }

        [Fact]
        public void SniffPngMetadataTest()
        {
            var metadata = _detector.SniffMetadata(
                PngSnifferTest.GetFileBytes());
            Assert.Equal(ImageFormat.Png, metadata.Format);
        }

        [Fact]
        public void SniffJpegMetadataTest()
        {
            var metadata = _detector.SniffMetadata(
                JpegSnifferTest.GetFileBytes());
            Assert.Equal(ImageFormat.Jpeg, metadata.Format);
        }

        [Fact]
        public void SniffWebpSimpleMetadataTest()
        {
            var metadata = _detector.SniffMetadata(
                WebpSnifferTest.GetSimpleFileBytes());
            Assert.Equal(ImageFormat.Webp, metadata.Format);
        }

        [Fact]
        public void SniffWebpComplexMetadataTest()
        {
            var metadata = _detector.SniffMetadata(
                WebpSnifferTest.GetComplexFileBytes());
            Assert.Equal(ImageFormat.Webp, metadata.Format);
        }

        [Fact]
        public void SniffWebpLosslessMetadataTest()
        {
            var metadata = _detector.SniffMetadata(
                WebpSnifferTest.GetLosslessFileBytes());
            Assert.Equal(ImageFormat.Webp, metadata.Format);
        }
    }
}