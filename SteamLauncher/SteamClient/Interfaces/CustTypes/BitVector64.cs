using System;

namespace SteamLauncher.SteamClient.Interfaces.CustTypes
{
    internal class BitVector64
    {
        public BitVector64()
        {
        }
        public BitVector64(UInt64 value)
        {
            Data = value;
        }

        public UInt64 Data { get; set; }

        public UInt64 this[uint bitOffset, UInt64 valueMask]
        {
            get => (Data >> (ushort)bitOffset) & valueMask;
            set => Data = (Data & ~(valueMask << (ushort)bitOffset)) | ((value & valueMask) << (ushort)bitOffset);
        }
    }
}
