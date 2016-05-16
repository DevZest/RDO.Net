using DevZest.Data.Windows.Primitives;
using System;

namespace DevZest.Data.Windows
{
    /// <summary>Specifies how to bypass auto size measuring for <see cref="TemplateItem"/>.</summary>
    [Flags]
    public enum AutoSizeWaiver
    {
        /// <summary>Both template auto width grid columns and auto height grid rows are respected.</summary>
        None = 0,
        /// <summary>Bypasses template auto width grid columns.</summary>
        Width = 0x1,
        /// <summary>Bypasses template auto height grid rows.</summary>
        Height = 0x2,
        /// <summary>Bypasses both template auto width grid columns and auto height grid rows.</summary>
        All = Width | Height
    }
}
