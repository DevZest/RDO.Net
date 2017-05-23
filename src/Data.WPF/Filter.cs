﻿using DevZest.Data;
using System;
using System.Diagnostics;

namespace DevZest.Windows
{
    public abstract class Filter
    {
        internal static Filter Create<T>(Func<T, DataRow, bool> condition)
            where T : Model
        {
            Debug.Assert(condition != null && condition.Target == null);
            return new FuncFilter<T>(condition);
        }

        private Filter()
        {
        }

        internal abstract bool Evaluate(DataRow dataRow);

        private sealed class FuncFilter<T> : Filter
            where T : Model
        {
            public FuncFilter(Func<T, DataRow, bool> condition)
            {
                Debug.Assert(condition != null && condition.Target == null);
                _condition = condition;
            }

            private readonly Func<T, DataRow, bool> _condition;

            internal override bool Evaluate(DataRow dataRow)
            {
                return _condition((T)dataRow.Model, dataRow);
            }
        }
    }
}
