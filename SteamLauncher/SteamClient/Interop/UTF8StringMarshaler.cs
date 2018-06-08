using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamLauncher.SteamClient.Interop
{
    // Not being used currently
    class Utf8StringMarshaler : ICustomMarshaler
    {
        private GCHandle _handle;

        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new Utf8StringMarshaler();
        }

        public IntPtr MarshalManagedToNative(Object managedObj)
        {
            if (!(managedObj is string str))
                return IntPtr.Zero;

            var bytes = new byte[Encoding.UTF8.GetByteCount(str) + 1];
            Encoding.UTF8.GetBytes(str, 0, str.Length, bytes, 0);
            bytes[bytes.Length - 1] = 0x0;

            _handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            return _handle.AddrOfPinnedObject();
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            _handle.Free();
        }

        public void CleanUpManagedData(Object managedObj)
        {

        }

        public int GetNativeDataSize()
        {
            return 0;
        }

        public Object MarshalNativeToManaged(IntPtr utf8Ptr)
        {
            if (utf8Ptr == IntPtr.Zero)
                return null;

            var byteList = new List<byte>();
            int i = 0;
            do
            {
                byteList.Add(Marshal.ReadByte(utf8Ptr, i));
                i++;
            } while (byteList[i - 1] != 0x0);

            return Encoding.UTF8.GetString(byteList.ToArray());
        }
    }

}
