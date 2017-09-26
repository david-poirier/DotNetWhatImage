using WhatImage;
using Xunit;

namespace WhatImageTests
{
    public class BitStreamTest
    {
        [Fact]
        public void TestWebpDimensions()
        {
            byte[] bytes = {143, 001, 075, 016};
            var bs = new BitStream(bytes);
            var width = bs.ReadShort(bits: 14, endianness: Endianness.LittleEndian);
            width++;
            var height = bs.ReadShort(bits: 14, endianness: Endianness.LittleEndian);
            height++;

            Assert.Equal(400, width);
            Assert.Equal(301, height);
        }
    }
}