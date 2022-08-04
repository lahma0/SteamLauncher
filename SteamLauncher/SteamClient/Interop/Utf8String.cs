using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamLauncher.SteamClient.Interop
{
    public class Utf8String : IDisposable
    {
        private IntPtr _nativeString;

        public Utf8String(string managedString)
        {
            if (string.IsNullOrEmpty(managedString))
            {
                _nativeString = IntPtr.Zero;
                return;
            }

            var buffer = new byte[Encoding.UTF8.GetByteCount(managedString) + 1];
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            _nativeString = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, _nativeString, buffer.Length);
            Marshal.WriteByte(_nativeString, buffer.Length, 0);
        }

        ~Utf8String()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_nativeString == IntPtr.Zero) 
                return;

            Marshal.FreeHGlobal(_nativeString);
            _nativeString = IntPtr.Zero;
        }

        public static implicit operator IntPtr(Utf8String that)
        {
            return that._nativeString;
        }
    }
}
