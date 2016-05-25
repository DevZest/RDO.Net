using System;

namespace DevZest.Data.Windows
{
    [Flags]
    public enum GridLinePosition
    {
        PreviousTrack = 0x1,
        NextTrack = 0x2,
        Both = PreviousTrack | NextTrack
    }
}
