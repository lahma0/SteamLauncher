using SteamLauncher.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore.VTablesStore
{
    /// <summary>
    /// Represents a Steam virtual function table and its associated interface including all of its properties, methods,
    /// and method signatures.
    /// </summary>
    public class VTable : IVTable
    {
        /// <summary>
        /// Private parameterless constructor for deserialization
        /// </summary>
        private VTable()
        {
            WeakEventManager<ObservableCollection<VtEntry>, NotifyCollectionChangedEventArgs>.AddHandler(VtEntries, "CollectionChanged", OnVtEntriesChanged);
        }

        public VTable(string interfaceName, string interfaceVersion, IEnumerable<VtEntry> vtEntries)
        {
            InterfaceName = interfaceName;
            InterfaceVersion = interfaceVersion;
            WeakEventManager<ObservableCollection<VtEntry>, NotifyCollectionChangedEventArgs>.AddHandler(VtEntries, "CollectionChanged", OnVtEntriesChanged);
            VtEntries.AddRange(vtEntries);
        }

        /// <summary>
        /// The vtable's interface name (ex: IClientShortcuts).
        /// </summary>
        [XmlAttribute("Name")]
        public string InterfaceName { get; set; }

        /// <summary>
        /// The Steam interface version identifier (ex: CLIENTSHORTCUTS_INTERFACE_VERSION001).
        /// </summary>
        [XmlAttribute("Version")]
        public string InterfaceVersion { get; set; }

        /// <summary>
        /// Collection of vtable's entries.
        /// </summary>
        public ObservableCollection<VtEntry> VtEntries { get; } = new ObservableCollection<VtEntry>();

        /// <summary>
        /// Event handler that adds a reference to this class instance in each instance of <see cref="VtEntry"/> added
        /// to <see cref="VtEntries"/>.
        /// </summary>
        /// <param name="sender">Identifies the sender responsible for triggering the event.</param>
        /// <param name="args">Provides information about how the collection has changed and what items were
        /// changed.</param>
        private void OnVtEntriesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var item in args.NewItems)
            {
                if (item is VtEntry vtEntry)
                    vtEntry.VTable = this;
            }
        }

        /// <summary>
        /// A pointer to the vtable's interface in memory.
        /// </summary> 
        [XmlIgnore]
        public IntPtr InterfacePtr { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Address of the vtable in memory (obtained by dereferencing <see cref="InterfacePtr"/>).
        /// </summary>
        [XmlIgnore]
        public IntPtr VTablePtr { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Gets a specific <see cref="VtEntry"/> by name.
        /// </summary>
        /// <param name="vtEntryName">The name of the <see cref="VtEntry"/> desired.</param>
        /// <returns>A <see cref="VtEntry"/> instance or null if no matching entry is found.</returns>
        public VtEntry GetVtEntry(string vtEntryName)
        {
            return VtEntries.SingleOrDefault(v => v.Name.EqualsIgnoreCase(vtEntryName));
        }

        /// <summary>
        /// A cache for all VtEntries delegates.
        /// </summary>
        [XmlIgnore]
        public Dictionary<Type, Delegate> DelegateCache { get; } = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Defines whether this vtable instance has been attached to an unmanaged interface pointer.
        /// </summary>
        [XmlIgnore]
        public bool IsAttached => InterfacePtr != IntPtr.Zero;

        /// <summary>
        /// Attaches an unmanaged pointer to this vtable instance.
        /// </summary>
        /// <param name="interfacePtr">An unmanaged pointer to an instance of this vtable's interface in memory.</param>
        public void Attach(IntPtr interfacePtr)
        {
            if (DelegateCache.Count > 0)
                DelegateCache.Clear();

            if (interfacePtr == IntPtr.Zero)
                throw new ArgumentException($"The '{nameof(interfacePtr)}' argument must contain a valid non-zero " +
                                            $"pointer value.");

            InterfacePtr = interfacePtr;
            VTablePtr = Marshal.ReadIntPtr(InterfacePtr);

            if (VTablePtr == IntPtr.Zero)
                throw new ArgumentException($"The '{nameof(interfacePtr)}' argument does not point to a valid " +
                                            $"virtual function table.");
        }

        /// <summary>
        /// Removes all referenced pointers and clears the delegate cache.
        /// </summary>
        public void Detach()
        {
            if (DelegateCache.Count > 0)
                DelegateCache.Clear();

            InterfacePtr = IntPtr.Zero;
            VTablePtr = IntPtr.Zero;
        }
    }
}
