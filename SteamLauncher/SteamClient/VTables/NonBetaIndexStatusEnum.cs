using System;
using System.Collections.Generic;
using System.Text;

namespace SteamLauncher.SteamClient.VTables
{
    ///// <summary>
    ///// Sometimes a Steam beta update will be released which adds/removes/changes entries within its interface's
    ///// vtables, resulting in the beta and non-beta releases having significant differences for a period of time.
    ///// 'OldIndex' normally refers to the index of a vtable entry in the non-beta version of Steam. This enum is used
    ///// to represent special states for the value of 'NonBetaIndex'.  The '<see cref="NonBetaIndexStatus.Same"/>' value
    ///// means there is no inconsistency in index between the beta and non-beta. The '<see
    ///// cref="NonBetaIndexStatus.NonExistent"/>' value means the beta introduced a new vtable entry and therefore there
    ///// is no valid index value for the non-beta version.
    ///// </summary>
    //public enum NonBetaIndexStatus
    //{
    //    NonExistent = -2,
    //    Same = -1
    //}
}
