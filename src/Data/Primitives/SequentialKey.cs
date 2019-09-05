namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents model for system temporary table of parent query, which contains a system identity column and primary key columns.
    /// </summary>
    public sealed class SequentialKey : KeyOutput
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SequentialKey"/> class.
        /// </summary>
        public SequentialKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SequentialKey"/> class.
        /// </summary>
        /// <param name="model">The source model.</param>
        public SequentialKey(Model model)
            : base(model)
        {
            AddTempTableIdentity();
        }

        /// <inheritdoc/>
        protected override string DbAliasPrefix
        {
            get { return "sys_sequential_"; }
        }
    }
}
