using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DescriptionAttribute : ColumnAttribute
    {
        public DescriptionAttribute(string description)
        {
            _description = description;
        }

        private readonly string _description;
        public string Description
        {
            get { return _description; }
        }

        protected override void Initialize(Column column)
        {
            column.Description = _description;
        }
    }
}
