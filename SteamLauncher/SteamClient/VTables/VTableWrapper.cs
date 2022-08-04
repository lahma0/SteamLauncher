using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SteamLauncher.SteamClient.VTables
{
    //public class VTableWrapper
    //{
    //    /// <summary>
    //    /// Creates a new <see cref="VTableWrapper"/> instance.
    //    /// </summary>
    //    /// <param name="interfaceName">Name of the vtable interface.</param>
    //    /// <param name="interfacePtr">A ptr to the vtable interface.</param>
    //    public VTableWrapper(string interfaceName, IntPtr interfacePtr) : this(
    //        interfaceName, 
    //        interfacePtr, 
    //        null, 
    //        null, 
    //        false) { }

    //    /// <summary>
    //    /// Creates a new <see cref="VTableWrapper"/> instance.
    //    /// </summary>
    //    /// <param name="interfaceName">Name of the vtable interface.</param>
    //    /// <param name="interfacePtr">A ptr to the vtable interface.</param>
    //    /// <param name="interfaceVersion">The version string descriptor of the interface (ex:
    //    /// 'CLIENTSHORTCUTS_INTERFACE_VERSION001').</param>
    //    public VTableWrapper(string interfaceName, IntPtr interfacePtr, string interfaceVersion) : this(
    //        interfaceName,
    //        interfacePtr,
    //        interfaceVersion, 
    //        null, 
    //        false) { }

    //    /// <summary>
    //    /// Creates a new <see cref="VTableWrapper"/> instance.
    //    /// </summary>
    //    /// <param name="interfaceName">Name of the vtable interface.</param>
    //    /// <param name="interfacePtr">A ptr to the vtable interface.</param>
    //    /// <param name="interfaceVersion">The version string descriptor of the interface (ex:
    //    /// 'CLIENTSHORTCUTS_INTERFACE_VERSION001').</param>
    //    /// <param name="entries">A list of <see cref="VTableEntryWrapper"/> objects representing the entries pointed
    //    /// to by this vtable/interface.</param>
    //    /// <param name="useBetaVTableIndex">A bool defining if the beta version of each entry's index should be
    //    /// used.</param>
    //    public VTableWrapper(string interfaceName,
    //                         IntPtr interfacePtr,
    //                         string interfaceVersion,
    //                         IEnumerable<VTableEntryWrapper> entries = null,
    //                         bool useBetaVTableIndex = false)
    //    {
    //        InterfaceName = interfaceName;

    //        InterfacePtr = interfacePtr;
    //        if (InterfacePtr == IntPtr.Zero)
    //            throw new ArgumentException($"'{nameof(interfacePtr)}' must contain a valid non-zero pointer value.");


    //        VTablePtr = Marshal.ReadIntPtr(InterfacePtr);
    //        if (VTablePtr == IntPtr.Zero)
    //            throw new ArgumentException($"'{nameof(interfacePtr)}' does not point to a valid virtual function table.");

    //        InterfaceVersion = interfaceVersion;

    //        if (entries == null)
    //            Entries = new List<VTableEntryWrapper>();
            
    //        UseBetaVTableIndex = useBetaVTableIndex;
    //    }

    //    public string InterfaceName { get; }

    //    public string InterfaceVersion { get; }

    //    public IntPtr InterfacePtr { get; }

    //    public IntPtr VTablePtr { get; }

    //    public bool UseBetaVTableIndex { get; }

    //    public IEnumerable<VTableEntryWrapper> Entries { get; }
    //}
}
