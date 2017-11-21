using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
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

            private sealed class EntryCollection : KeyedCollection<Column, Entry>
            {
                protected override Column GetKeyForItem(Entry item)
                {
                    return item.Column;
                }

                public void Add(TColumnsMemberAttribute memberAttribute, Column column)
                {
                    if (Contains(column))
                        throw new InvalidOperationException(Strings.ColumnsMemberAttribute_Duplicate(column, typeof(TColumnsMemberAttribute), memberAttribute.Name));

                    Insert(GetInsertIndex(memberAttribute), new Entry(memberAttribute, column));
                }

                private int GetInsertIndex(TColumnsMemberAttribute memberAttribute)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        if (memberAttribute.Order < this[i].MemberAttribute.Order)
                            return i;
                    }
                    return Count;
                }
            }

            private Dictionary<Type, ColumnsAttributeCollection> _columnsAttributesByType = new Dictionary<Type, ColumnsAttributeCollection>();
            private ConditionalWeakTable<Model, Dictionary<TColumnsAttribute, EntryCollection>> _entriesByModel = new ConditionalWeakTable<Model, Dictionary<TColumnsAttribute, EntryCollection>>();

            private void EnsureColumnsAttributesInitialilzed(Type modelType)
            {
                Debug.Assert(modelType != null);
                if (_columnsAttributesByType.ContainsKey(modelType))
                    return;

                var columnsAttributeCollection = new ColumnsAttributeCollection();
                _columnsAttributesByType.Add(modelType, columnsAttributeCollection);
                foreach (var columnsAttribute in modelType.GetTypeInfo().GetCustomAttributes<TColumnsAttribute>(true))
                    columnsAttributeCollection.Add(columnsAttribute);
            }

            private TColumnsAttribute GetColumnsAttribute(Type modelType, string attributeName)
            {
                EnsureColumnsAttributesInitialilzed(modelType);
                var columnsAttributes = _columnsAttributesByType[modelType];
                Debug.Assert(columnsAttributes != null);
                if (!columnsAttributes.Contains(attributeName))
                    throw new InvalidOperationException(Strings.ColumnsMemberAttribute_CannotResolveColumnsAttribute(modelType, typeof(TColumnsAttribute), attributeName));
                var result = columnsAttributes[attributeName];
                Debug.Assert(result != null);
                return result;
            }

            public void Initialize(TColumnsMemberAttribute columnsMemberAttribute, Column column)
            {
                Check.NotNull(columnsMemberAttribute, nameof(columnsMemberAttribute));
                Check.NotNull(column, nameof(column));

                var model = column.ParentModel;
                var columnsAttribute = GetColumnsAttribute(model.GetType(), columnsMemberAttribute.Name);
                Dictionary<TColumnsAttribute, EntryCollection> entryDictionary;
                if (!_entriesByModel.TryGetValue(model, out entryDictionary))
                {
                    entryDictionary = new Dictionary<TColumnsAttribute, EntryCollection>();
                    _entriesByModel.Add(model, entryDictionary);
                    model.Initializing += OnModelInitializing;
                }
                EntryCollection entries;
                if (!entryDictionary.TryGetValue(columnsAttribute, out entries))
                {
                    entries = new EntryCollection();
                    entryDictionary.Add(columnsAttribute, entries);
                }
                entries.Add(columnsMemberAttribute, column);
            }

            private void OnModelInitializing(object sender, EventArgs e)
            {
                var model = (Model)sender;
                Dictionary<TColumnsAttribute, EntryCollection> entriesDictionary;
                _entriesByModel.TryGetValue(model, out entriesDictionary);
                Debug.Assert(entriesDictionary != null);
                foreach (var keyValuePair in entriesDictionary)
                    Initialize(model, keyValuePair.Key, keyValuePair.Value);
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

        public int Order { get; set; }
    }
}
