﻿using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    public abstract class DbTableConstraint : IExtension
    {
        protected DbTableConstraint(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        object IExtension.Key
        {
            get { return typeof(DbTableConstraint).FullName + "." + SystemName; }
        }

        private string _systemName;
        public virtual string SystemName
        {
            get { return _systemName ?? (_systemName = string.IsNullOrEmpty(Name) ? Guid.NewGuid().ToString() : Name); }
        }
    }
}
