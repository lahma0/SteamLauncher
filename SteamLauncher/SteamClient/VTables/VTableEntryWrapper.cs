using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SteamLauncher.SteamClient.VTables
{
    //public abstract class VTableEntryWrapper
    //{
    //    protected VTableEntryWrapper(VTableWrapper parentVTable)
    //    {
    //        ParentVTable = parentVTable;
    //    }

    //    protected VTableWrapper ParentVTable { get; }

    //    protected int VTableIndex { get; }

    //    protected static Dictionary<Type, Delegate> DelegateCache { get; } = new Dictionary<Type, Delegate>();

    //    protected TDelegate GetDelegate<TDelegate>() where TDelegate : class
    //    {
    //        if (!DelegateCache.TryGetValue(typeof(TDelegate), out Delegate func))
    //        {
    //            func = GetFunction(typeof(TDelegate));
    //            if (func != null)
    //                DelegateCache.Add(typeof(TDelegate), func);
    //        }

    //        return func as TDelegate; // returns null if type conversion fails
    //    }

    //    /// <summary>
    //    /// Uses the vtable index tied to the provided delegate type to calculate its unmanaged function pointer and
    //    /// wire it up to its associated 'wrapper' method.
    //    /// </summary>
    //    /// <param name="d">The delegate type to lookup</param>
    //    /// <returns>A delegate that now references its associated native function</returns>
    //    protected Delegate GetFunction(Type d)
    //    {
    //        var functionPtr = Marshal.ReadIntPtr(ParentVTable.VTablePtr, 
    //                                             VTableIndex * IntPtr.Size);
    //        return Marshal.GetDelegateForFunctionPointer(functionPtr, d);
    //    }

    //    protected void Execute<TDelegate>(IntPtr ptr, params object[] args)
    //    {
    //        GetFunction(typeof(TDelegate)).DynamicInvoke(args);
    //    }

    //    protected TReturn Execute<TReturn, TDelegate>(IntPtr ptr, params object[] args)
    //    {
    //        return (TReturn)GetFunction(typeof(TDelegate)).DynamicInvoke(args);
    //    }
    //}
}
