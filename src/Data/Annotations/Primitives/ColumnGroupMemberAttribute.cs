using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ColumnGroupMemberAttribute : ColumnAttribute
    {
        protected abstract class Manager<TColumnGroupAttribute, TColumnGroupMemberAttribute>
            where TColumnGroupAttribute : ColumnGroupAttribute
            where TColumnGroupMemberAttribute : ColumnGroupMemberAttribute
        {
            private sealed class ColumnsAttributeCollection : KeyedCollection<string, TColumnGroupAttribute>
            {
                protected override string GetKeyForItem(TColumnGroupAttribute item)
                {
                    return item.Name;
                }
            }

            protected struct Entry
            {
                internal Entry(TColumnGroupMemberAttribute memberAttribute, Column column)
                {
                    Debug.Assert(memberAttribute != null);
                    Debug.Assert(column != null);
                    MemberAttribute = memberAttribute;
                    Column = column;
                }

                public readonly TColumnGroupMemberAttribute MemberAttribute;
                public readonly Column Column;
            }

            private sealed class EntryCollection : KeyedCollection<Column, Entry>
            {
                protected override Column GetKeyForItem(Entry item)
                {
                    return item.Column;
                }

                public void Add(TColumnGroupMemberAttribute memberAttribute, Column column)
                {
                    if (Contains(column))
                        throw new InvalidOperationException(DiagnosticMessages.ColumnsMemberAttribute_Duplicate(column, typeof(TColumnGroupMemberAttribute), memberAttribute.Name));

                    Insert(GetInsertIndex(memberAttribute), new Entry(memberAttribute, column));
                }

                private int GetInsertIndex(TColumnGroupMemberAttribute memberAttribute)
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
            private ConditionalWeakTable<Model, Dictionary<TColumnGroupAttribute, EntryCollection>> _entriesByModel = new ConditionalWeakTable<Model, Dictionary<TColumnGroupAttribute, EntryCollection>>();

            private void EnsureColumnsAttributesInitialilzed(Type modelType)
            {
                Debug.Assert(modelType != null);
                if (_columnsAttributesByType.ContainsKey(modelType))
                    return;

                var columnsAttributeCollection = new ColumnsAttributeCollection();
                _columnsAttributesByType.Add(modelType, columnsAttributeCollection);
                foreach (var columnsAttribute in modelType.GetTypeInfo().GetCustomAttributes<TColumnGroupAttribute>(true))
                    columnsAttributeCollection.Add(columnsAttribute);
            }

            private TColumnGroupAttribute GetColumnsAttribute(Type modelType, string attributeName)
            {
                EnsureColumnsAttributesInitialilzed(modelType);
                var columnsAttributes = _columnsAttributesByType[modelType];
                Debug.Assert(columnsAttributes != null);
                if (!columnsAttributes.Contains(attributeName))
                    throw new InvalidOperationException(DiagnosticMessages.ColumnsMemberAttribute_CannotResolveColumnsAttribute(modelType, typeof(TColumnGroupAttribute), attributeName));
                var result = columnsAttributes[attributeName];
                Debug.Assert(result != null);
                return result;
            }

            public void Initialize(TColumnGroupMemberAttribute columnsMemberAttribute, Column column)
            {
                columnsMemberAttribute.VerifyNotNull(nameof(columnsMemberAttribute));
                column.VerifyNotNull(nameof(column));

                var model = column.ParentModel;
                var columnsAttribute = GetColumnsAttribute(model.GetType(), columnsMemberAttribute.Name);
                Dictionary<TColumnGroupAttribute, EntryCollection> entryDictionary;
                if (!_entriesByModel.TryGetValue(model, out entryDictionary))
                {
                    entryDictionary = new Dictionary<TColumnGroupAttribute, EntryCollection>();
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
                Dictionary<TColumnGroupAttribute, EntryCollection> entriesDictionary;
                _entriesByModel.TryGetValue(model, out entriesDictionary);
                Debug.Assert(entriesDictionary != null);
                foreach (var keyValuePair in entriesDictionary)
                    Initialize(model, keyValuePair.Key, keyValuePair.Value);
                _entriesByModel.Remove(model);
            }

            protected abstract void Initialize(Model model, TColumnGroupAttribute columnsAttribute, IReadOnlyList<Entry> entries);
        }

        protected ColumnGroupMemberAttribute(string name)
        {
            Name = name.VerifyNotEmpty(nameof(name));
        }

        public string Name { get; private set; }

        public int Order { get; set; }

        protected sealed override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
