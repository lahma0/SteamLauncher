using System;
using System.Collections.Generic;

namespace SteamLauncher.DataStore.VTablesStore
{
    public interface IVTable
    {
        public IntPtr InterfacePtr { get; }

        public IntPtr VTablePtr { get; }

        public Dictionary<Type, Delegate> DelegateCache { get; }

        public bool IsAttached { get; }

        public VtEntry GetVtEntry(string entryName);
    }
}
