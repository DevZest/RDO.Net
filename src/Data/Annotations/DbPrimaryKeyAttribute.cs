using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [ModelMemberAttributeSpec(null, false, typeof(PrimaryKey))]
    public sealed class DbPrimaryKeyAttribute : Attribute
    {
        public DbPrimaryKeyAttribute(string name)
        {
            _name = name;
        }

        private readonly string _name;
        public string Name
        {
            get { return _name; }
        }

        public string Description { get; set; }
    }
}
