using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(ColumnDefault) }, validOnTypes: new Type[] { typeof(_Guid) })]
    public sealed class AutoGuidAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Guid guidColumn)
                guidColumn.SetDefault(_Guid.NewGuid(), Name, Description);
        }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
