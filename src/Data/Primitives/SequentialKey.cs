namespace DevZest.Data.Primitives
{
    public sealed class SequentialKey : KeyOutput
    {
        public SequentialKey()
        {
        }

        public SequentialKey(Model model)
            : base(model)
        {
            AddTempTableIdentity();
        }

        protected override string DbAliasPrefix
        {
            get { return "sys_sequential_"; }
        }
    }
}
