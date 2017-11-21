using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnsMemberAttribute : ColumnAttribute
    {
        protected abstract class Manager<TColumnsAttribute, TColumnsMemberAttribute>
            where TColumnsAttribute : ColumnsAttribute
            where TColumnsMemberAttribute : ColumnsMemberAttribute
        {
            private sealed class ColumnsAttributeCollection : KeyedCollection<string, TColumnsAttribute>
            {
                protected override string GetKeyForItem(TColumnsAttribute item)
                {
                    return item.Name;
                }
            }

            protected struct Entry
            {
                internal Entry(TColumnsMemberAttribute memberAttribute, Column column)
                {
                    Debug.Assert(memberAttribute != null);
                    Debug.Assert(column != null);
                    MemberAttribute = memberAttribute;
                    Column = column;
                }

                public readonly TColumnsMemberAttribute MemberAttribute;
                public readonly Column Column;
            }

            private Dictionary<Type, ColumnsAttributeCollection> _columnsAttributesByType = new Dictionary<Type, ColumnsAttributeCollection>();
            private ConditionalWeakTable<Model, TColumnsAttribute> _columnsAttributeByModel = new ConditionalWeakTable<Model, TColumnsAttribute>();
            private ConditionalWeakTable<Model, List<Entry>> _entriesByModel = new ConditionalWeakTable<Model, List<Entry>>();

            public void Initialize(TColumnsMemberAttribute columnsMemberAttribute, Column column)
            {
                Check.NotNull(columnsMemberAttribute, nameof(columnsMemberAttribute));
                Check.NotNull(column, nameof(column));

                var model = column.ParentModel;
                List<Entry> entries;
                if (!_entriesByModel.TryGetValue(model, out entries))
                {
                    entries = new List<Entry>();
                    _entriesByModel.Add(model, entries);
                    model.Initializing += OnModelInitializing;
                }
                entries.Add(new Entry(columnsMemberAttribute, column));
            }

            private void OnModelInitializing(object sender, EventArgs e)
            {
                var model = (Model)sender;
                TColumnsAttribute columnsAttribute;
                _columnsAttributeByModel.TryGetValue(model, out columnsAttribute);
                Debug.Assert(columnsAttribute != null);
                List<Entry> entries;
                _entriesByModel.TryGetValue(model, out entries);
                Debug.Assert(entries != null);
                Initialize(model, columnsAttribute, entries);
                _columnsAttributeByModel.Remove(model);
                _entriesByModel.Remove(model);
            }

            protected abstract void Initialize(Model model, TColumnsAttribute columnsAttribute, IReadOnlyList<Entry> entries);
        }

        protected ColumnsMemberAttribute(string name)
        {
            Check.NotEmpty(name, nameof(name));
            Name = name;
        }

        public string Name { get; private set; }
    }
}
