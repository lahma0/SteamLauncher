using SteamLauncher.SteamClient.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamLauncher.SteamClient.Interop
{
    /// <summary>
    /// Generic wrapper class for any managed SteamClient DLL native interface implementation.
    /// </summary>
    public abstract class SteamInterfaceWrapper
    {
        protected SteamInterfaceWrapper(IntPtr interfacePtr)
        {
            if (interfacePtr == IntPtr.Zero)
                throw new ArgumentException($"The '{nameof(interfacePtr)}' argument must contain a valid non-zero pointer value.");

            InterfacePtr = interfacePtr;
            VTablePtr = Marshal.ReadIntPtr(InterfacePtr);

            if (VTablePtr == IntPtr.Zero)
                throw new ArgumentException($"The '{nameof(interfacePtr)}' argument does not point to a valid virtual function table.");
        }

        protected IntPtr InterfacePtr { get; }

        protected IntPtr VTablePtr { get; }

        protected Dictionary<Type, Delegate> DelegateCache { get; } = new Dictionary<Type, Delegate>();

        protected TDelegate GetDelegate<TDelegate>() where TDelegate : class
        {
            if (!DelegateCache.TryGetValue(typeof(TDelegate), out Delegate func))
            {
                func = GetFunction(typeof(TDelegate));
                if (func != null)
                    DelegateCache.Add(typeof(TDelegate), func);
            }
            
            return func as TDelegate; // returns null if type conversion fails
        }

        /// <summary>
        /// Uses the vtable index tied to the provided delegate type to calculate its unmanaged function pointer and wire it up to its associated 'wrapper' method.
        /// </summary>
        /// <param name="d">The delegate type to lookup</param>
        /// <returns>A delegate that now references its associated native function</returns>
        protected Delegate GetFunction(Type d)
        {
            int vtableIndex = ((VTableIndex)(d.GetCustomAttribute(typeof(VTableIndex)))).Index;
            IntPtr functionPtr = Marshal.ReadIntPtr(VTablePtr, vtableIndex * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer(functionPtr, d);
        }

        // Not currently being used
        //protected TDelegate GetFunction<TDelegate>(TDelegate d)
        //{
        //    int vtableIndex = ((VTableIndex)(typeof(TDelegate).GetCustomAttribute(typeof(VTableIndex)))).Index;
        //    IntPtr functionPtr = Marshal.ReadIntPtr(VTablePtr, vtableIndex * IntPtr.Size);
        //    return Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPtr);
        //}

        /// <summary>
        /// Converts a UTF-8 string stored at the provided pointer into a normal string object. This is needed because Steam stores strings in UTF-8.
        /// </summary>
        /// <param name="stringPtr">A pointer referencing a null-terminated UTF-8 string.</param>
        /// <returns>A properly encoded string object.</returns>
        protected static string DecodeUtf8String(IntPtr stringPtr)
        {
            if (stringPtr == IntPtr.Zero)
                return null;

            var unencodedString = Marshal.PtrToStringAnsi(stringPtr);

            return unencodedString == null ? null : Encoding.UTF8.GetString(Encoding.Default.GetBytes(unencodedString));
        }

        
    }
}
