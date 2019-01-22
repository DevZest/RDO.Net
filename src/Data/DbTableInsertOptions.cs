using System;

namespace DevZest.Data
{
    [Flags]
    public enum DbTableInsertOptions
    {
        None = 0,
        SkipExisting = 1,
        UpdateIdentity = 2
    }
}
