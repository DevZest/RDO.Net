using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelMemberAttributeSpec(typeof(ColumnDefault), true, typeof(_DateTime))]
    public sealed class AutoDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.SetDefault(Functions.GetDate(), Name, Description);
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
