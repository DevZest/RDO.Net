﻿using System;
using System.Threading;

namespace DevZest.Data.Addons
{
    public abstract class DbTableConstraint : IAddon
    {
        protected DbTableConstraint(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        object IAddon.Key
        {
            get { return typeof(DbTableConstraint).FullName + "." + SystemName; }
        }

        private string _systemName;
        public virtual string SystemName
        {
            get { return LazyInitializer.EnsureInitialized(ref _systemName, () => string.IsNullOrEmpty(Name) ? Guid.NewGuid().ToString() : Name); }
        }

        public abstract bool IsValidOnTable { get; }

        public abstract bool IsValidOnTempTable { get; }
    }
}
