using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public sealed class UniqueColumnsAttribute : ValidatorColumnsAttribute
    {
        public UniqueColumnsAttribute(string name)
            : base(name)
        {
        }

        public bool IsClustered { get; set; }

        protected override string GetDefaultMessage(IColumns columns, DataRow dataRow)
        {
            return Strings.UniqueColumnsAttribute_DefaultErrorMessage(columns);
        }
    }
}
