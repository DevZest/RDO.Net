namespace DevZest.Data.Windows
{
    /// <summary>
    /// Values to specify how <see cref="Primitives.ScalarItem"/> flows when <see cref="Primitives.Template.RepeatOrientation"/> is
    /// <see cref="RepeatOrientation.XY"/> or <see cref="RepeatOrientation.YX"/>.
    /// </summary>
    public enum FlowMode
    {
        /// <summary>Repeated <see cref="Primitives.ScalarItem"/> elements will be flowed.</summary>
        Repeat,
        /// <summary>Single <see cref="Primitives.ScalarItem"/> element will be displayed and stretched across the flow.</summary>
        Stretch
    }
}
