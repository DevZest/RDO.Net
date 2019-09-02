namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Indicates when to wireup attribute with model.
    /// </summary>
    public enum ModelWireupEvent
    {
        /// <summary>
        /// Model is constructing.
        /// </summary>
        Constructing,
        
        /// <summary>
        /// Model is initializing.
        /// </summary>
        Initializing,

        /// <summary>
        /// Child models mounted.
        /// </summary>
        ChildModelsMounted,

        /// <summary>
        /// Child DataSets are created.
        /// </summary>
        ChildDataSetsCreated,

        /// <summary>
        /// Model is initialized.
        /// </summary>
        Initialized
    }
}
