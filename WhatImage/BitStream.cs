using System;
using System.Collections.Generic;
using System.IO;

namespace WhatImage
{
    public class BitStream
    {
        private readonly MemoryStream _ms;
        private readonly BinaryReader _reader;

        private int _bitOffset;
        private byte? _currentByte;

        public BitStream(byte[] data)
        {
            _ms = new MemoryStream(data);
            _reader = new BinaryReader(_ms);
        }
        
        

        public short ReadShort(int bits = 16, Endianness endianness = Endianness.BigEndian)
        {
            if (bits <= 8)
                throw new ArgumentException("Use 'ReadByte' or 'ReadSByte' instead");
            if (bits >= 32)
                throw new ArgumentException("Use 'ReadInt' or 'ReadUInt' instead");

            var bytes = ReadBits(bits, endianness);
            if (endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        private byte[] ReadBits(int bits, Endianness endianness)
        {
            // byte-aligned
            if (bits % 8 == 0 && _bitOffset == 0)
                return _reader.ReadBytes(bits / 8);

            // non-byte aligned
            var result = new List<byte>();
            var bitsRead = 0;
            var outputByte = 0;
            while (bitsRead < bits)
            {
                if (_currentByte == null)
                    _currentByte = _reader.ReadByte();

                var mask = 1 << _bitOffset;
                var bit = _currentByte.Value & mask;
                // realign bit if necessary
                var offset = _bitOffset % 8 - bitsRead % 8;
                if (offset > 0)
                    bit >>= offset;
                if (offset < 0)
                    bit <<= Math.Abs(offset);

                outputByte |= bit;

                bitsRead++;
                _bitOffset++;

                // output byte
                if (bitsRead % 8 == 0 || bitsRead == bits)
                {
                    if (endianness == Endianness.LittleEndian)
                        result.Add((byte) outputByte);
                    else
                        result.Insert(0, (byte) outputByte);

                    outputByte = 0;
                }

                // next input byte
                if (_bitOffset > 7)
                {
                    _bitOffset = 0;
                    _currentByte = null;
                }
            }

            return result.ToArray();
        }
    }

    public enum Endianness
    {
        LittleEndian,
        BigEndian
    }
}