using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SteamLauncher.SteamClient.Native;

namespace SteamLauncher.SteamClient.Interop
{
    public class SteamParamStringArray : IDisposable
    {
        IntPtr structPtr;
        IntPtr stringArrayPtr;
        IntPtr[] strings;

        public SteamParamStringArray(IList<string> strings)
        {
            if (strings == null)
            {
                structPtr = IntPtr.Zero;
                return;
            }

            this.strings = new IntPtr[strings.Count];
            for (var index = 0; index < strings.Count; ++index)
            {
                this.strings[index] = new Utf8String(strings[index]);
            }

            var ptrSize = Marshal.SizeOf(typeof(IntPtr));
            var arrayLen = this.strings.Length;
            var allocSize = ptrSize * arrayLen;
            stringArrayPtr = Marshal.AllocHGlobal(allocSize);
            Marshal.Copy(this.strings, 0, stringArrayPtr, this.strings.Length);

            var paramStringArrayT = new SteamParamStringArray_t()
            {
                stringArrayPtr = stringArrayPtr,
                numOfStrings = strings.Count
            };

            structPtr = Marshal.AllocHGlobal(Marshal.SizeOf(paramStringArrayT));
            Marshal.StructureToPtr((object)paramStringArrayT, structPtr, false);
        }

        ~SteamParamStringArray()
        {
            Dispose();
        }

        public void Dispose()
        {
            foreach (var s in strings)
            {
                Marshal.FreeHGlobal(s);
            }

            if (stringArrayPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(stringArrayPtr);

            if (structPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(structPtr);
        }

        public static implicit operator IntPtr(SteamParamStringArray that)
        {
            return that.structPtr;
        }
    }

}
