using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SteamLauncher.Tools
{
    public sealed class Crc32 : HashAlgorithm
    {
        public const UInt32 DEFAULT_POLYNOMIAL = LITTLE_ENDIAN_POLYNOMIAL;
        public const UInt32 LITTLE_ENDIAN_POLYNOMIAL = 0xedb88320u;
        public const UInt32 BIG_ENDIAN_POLYNOMIAL = 0x04C11DB7u;

        public const UInt32 DEFAULT_SEED = 0xffffffffu;

        private static UInt32[] _defaultTable;

        private readonly UInt32 _seed;
        private readonly UInt32[] _table;
        private UInt32 _hash;

        public Crc32() : this(DEFAULT_POLYNOMIAL, DEFAULT_SEED)
        {
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            _table = InitializeTable(polynomial);
            _seed = _hash = seed;
        }

        public override void Initialize()
        {
            _hash = _seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize => 32;

        public static UInt32 Compute(byte[] buffer)
        {
            return Compute(DEFAULT_SEED, buffer);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return Compute(DEFAULT_POLYNOMIAL, seed, buffer);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer, bool reflectIn, bool reflectOut, UInt32 xorOut)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length, reflectIn, reflectOut, xorOut);
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DEFAULT_POLYNOMIAL && _defaultTable != null)
                return _defaultTable;

            var createTable = new UInt32[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (UInt32)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DEFAULT_POLYNOMIAL)
                _defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size, bool reflectIn = false, bool reflectOut = false, UInt32 xorOut = 0)
        {
            if (reflectIn)
                buffer.Select(b => ReverseByte(b));

            var hash = seed;
            for (var i = start; i < start + size; i++)
                hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];

            if (reflectOut)
                hash = ReverseBits(hash);

            if (xorOut != 0)
                hash = hash ^ xorOut;

            return hash;
        }

        private static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        private static byte ReverseByte(byte originalByte)
        {
            var result = 0;
            for (var i = 0; i < 8; i++)
            {
                result = result << 1;
                result += originalByte & 1;
                originalByte = (byte)(originalByte >> 1);
            }

            return (byte)result;
        }

        private static UInt32 ReverseBits(UInt32 n)
        {
            n = (n >> 1) & 0x55555555 | (n << 1) & 0xaaaaaaaa;
            n = (n >> 2) & 0x33333333 | (n << 2) & 0xcccccccc;
            n = (n >> 4) & 0x0f0f0f0f | (n << 4) & 0xf0f0f0f0;
            n = (n >> 8) & 0x00ff00ff | (n << 8) & 0xff00ff00;
            n = (n >> 16) & 0x0000ffff | (n << 16) & 0xffff0000;
            return n;
        }
    }
}
