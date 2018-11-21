﻿using DevZest.Data.Primitives;

namespace DevZest.Data.Addons
{
    [Addon(typeof(ColumnSort))]
    public abstract class ColumnDefault : IAddon
    {
        internal ColumnDefault(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public abstract DbExpression DbExpression { get; }

        object IAddon.Key
        {
            get { return typeof(ColumnDefault); }
        }
    }
}