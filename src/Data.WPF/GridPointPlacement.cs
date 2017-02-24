using System;

namespace DevZest.Data.Windows
{
    [Flags]
    public enum GridPointPlacement
    {
        PreviousTrack = 0x1,
        NextTrack = 0x2,
        Both = PreviousTrack | NextTrack
    }
}
