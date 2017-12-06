using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DbConstraintAttribute : Attribute
    {
        public DbConstraintAttribute(string name)
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
