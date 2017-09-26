
namespace WhatImage
{
    internal static class BinaryHelpers
    {
        public static bool CompareBytes(
            byte[] data1, int data1offset,
            byte[] data2, int data2offset,
            int length)
        {
            for (var i = 0; i < length; i++)
            {
                if (data1[data1offset + i] != data2[data2offset + i])
                    return false;
            }
            return true;
        }
    }
}