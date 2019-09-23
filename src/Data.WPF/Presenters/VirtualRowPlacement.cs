namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Specifies where to display virtual row for inserting.
    /// </summary>
    public enum VirtualRowPlacement
    {
        /// <summary>
        /// The virtual row will be displayed at place explicitly specified.
        /// </summary>
        Explicit = 0,

        /// <summary>
        /// The virtual row will be displayed at head of the view.
        /// </summary>
        Head,

        /// <summary>
        /// The virtual row will be displayed at tail of the view.
        /// </summary>
        Tail,

        /// <summary>
        /// The virtual row will be displayed only when no existing row.
        /// </summary>
        Exclusive
    }
}
