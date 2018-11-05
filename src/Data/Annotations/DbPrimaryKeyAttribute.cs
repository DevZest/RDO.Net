using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [ModelMemberAttributeSpec(addonTypes: null, validOnTypes: new Type[] { typeof(PrimaryKey) })]
    public sealed class DbPrimaryKeyAttribute : Attribute
    {
        public DbPrimaryKeyAttribute(string name)
        {
            Name = name;
        }

        public string Name { get;  }

        public string Description { get; set; }
    }
}
