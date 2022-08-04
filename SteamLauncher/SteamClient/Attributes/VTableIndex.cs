using System;

namespace SteamLauncher.SteamClient.Attributes
{
    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
    public class VTableIndex : Attribute
    {
        public VTableIndex(int currentIndex, int oldIndex = (int)OldIndexStatus.Same)
        {
            CurrentIndex = currentIndex;
            OldIndex = oldIndex;
        }

        /// <summary>
        /// Returns the correct vtable index based on the value of 'useOldIndex'
        /// </summary>
        public int GetIndex(bool useOldIndex = false)
        {
            if (useOldIndex && OldIndex == (int)OldIndexStatus.NonExistent)
            {
                throw new NotImplementedException($"This vtable entry is non-existent in the " +
                                                  $"version of the application you're interfacing " +
                                                  $"with ({nameof(useOldIndex)}={useOldIndex}).");
            }

            if (useOldIndex && OldIndex >= 0)
                return OldIndex;

            return CurrentIndex;
        }

        /// <summary>
        /// Represents the most recent vtable index value (including Steam beta).
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// New property to deal with the problem of having a different number of vtable 
        /// entries simultaneously in the stable Steam client vs the beta Steam client. 
        /// OldIndex is populated only if there is currently an inconsistency in the offset; 
        /// otherwise it is -1. If it is populated, it reflects the previously valid vtable 
        /// index (likely the current stable Steam client).
        /// </summary>
        public int OldIndex { get; set; }

        
    }

    /// <summary>
    /// Used to define vtable entry index differences. Default value (-1/Same) means 
    /// there is no inconsistency in index between old and new versions. Vtable entries 
    /// labeled with (-2/NonExistent) means this is a new vtable entry and therefore there 
    /// is no valid offset for the old version.
    /// </summary>
    public enum OldIndexStatus
    {
        NonExistent = -2,
        Same = -1
    }
}
