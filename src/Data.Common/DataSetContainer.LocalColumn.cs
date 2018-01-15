using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data
{
    partial class DataSetContainer
    {
        private interface ILocalColumn
        {
            void OnDataRowInserting(DataRow dataRow);
            void OnDataRowRemoving(DataRow dataRow);
        }

        private sealed class LocalColumn<T> : Column<T>, ILocalColumn
        {
            public LocalColumn(Model model, LocalColumnExpressionBase<T> expression, Action<LocalColumnBuilder<T>> initializer)
            {
                Initialize(model, expression);
                if (initializer != null)
                {
                    _initializer = initializer;
                    using (var builder = new LocalColumnBuilder<T>(this))
                    {
                        initializer(builder);
                    }
                }
                if (model == null || model.IsInitialized)
                    SealLocalColumn();
            }

            private Action<LocalColumnBuilder<T>> _initializer;

            public override bool IsLocal
            {
                get { return true; }
            }

            private void Initialize(Model model, ColumnExpression<T> expression)
            {
                if (model != null)
                {
                    ParentModel = model;
                    OriginalDeclaringType = DeclaringType = model.GetType();
                    List<Column> localColumns = model.LocalColumnList;
                    if (localColumns == null)
                        model.LocalColumnList = localColumns = new List<Column>();
                    Ordinal = localColumns.Count;
                    OriginalName = Name = "LocalColumn" + localColumns.Count.ToString(CultureInfo.InvariantCulture);
                    model.LocalColumnList.Add(this);
                }
                else
                    Name = "LocalColumn";
                Kind = ColumnKind.ModelProperty;
                Expression = expression;
            }

            private void InitValues()
            {
                InitValueManager();
                if (IsConcrete)
                    InitConcreteValues();
            }

            private void InitConcreteValues()
            {
                Debug.Assert(IsConcrete);
                var dataSet = ParentModel.DataSet;
                for (int i = 0; i < dataSet.Count; i++)
                {
                    var dataRow = dataSet[i];
                    T value = Expression == null ? DefaultValue : Expression[dataRow];
                    InsertRow(dataRow, value);
                }
            }

            public void OnDataRowInserting(DataRow dataRow)
            {
                InsertRow(dataRow);
            }

            public void OnDataRowRemoving(DataRow dataRow)
            {
                RemoveRow(dataRow);
            }

            private bool _designMode = true;
            protected internal override bool DesignMode
            {
                get { return _designMode; }
            }

            internal override void SealLocalColumn()
            {
                InitValues();
                _designMode = false;
            }

            public override _String CastToString()
            {
                throw new NotSupportedException();
            }

            protected override Column<T> CreateParam(T value)
            {
                throw new NotSupportedException();
            }

            protected internal override Column<T> CreateConst(T value)
            {
                return new LocalColumn<T>(null, new LocalColumnExpression<T>(dataRow => value), null);
            }

            protected internal override T DeserializeValue(JsonValue value)
            {
                throw new NotSupportedException();
            }

            protected internal override JsonValue SerializeValue(T value)
            {
                throw new NotSupportedException();
            }
        }

        private abstract class LocalColumnExpressionBase<TDataType> : ColumnExpression<TDataType>
        {
            public sealed override DbExpression GetDbExpression()
            {
                throw new NotSupportedException();
            }

            protected sealed override IModels GetAggregateBaseModels()
            {
                throw new NotSupportedException();
            }

            protected sealed override IModels GetScalarSourceModels()
            {
                throw new NotSupportedException();
            }
        }

        private sealed class LocalColumnExpression<TDataType> : LocalColumnExpressionBase<TDataType>
        {
            private static TDataType GetDefaultValue(DataRow dataRow)
            {
                return default(TDataType);
            }

            public LocalColumnExpression(Func<DataRow, TDataType> expression)
            {
                _expression = expression;
            }

            private readonly Func<DataRow, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow); }
            }

            protected override IColumns GetBaseColumns()
            {
                return Columns.Empty;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                return this;
            }
        }

        private sealed class LocalColumnExpression<T1, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
        {
            public LocalColumnExpression(T1 column, Func<DataRow, T1, TDataType> expression)
            {
                _column = column;
                _expression = expression;
            }

            private readonly T1 _column;
            private readonly Func<DataRow, T1, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _column.BaseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column = _column.TranslateTo(model);
                if (column != _column)
                    return new LocalColumnExpression<T1, TDataType>(column, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, Func<DataRow, T1, T2, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2)
                    return new LocalColumnExpression<T1, T2, TDataType>(column1, column2, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, Func<DataRow, T1, T2, T3, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3)
                    return new LocalColumnExpression<T1, T2, T3, TDataType>(column1, column2, column3, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, Func<DataRow, T1, T2, T3, T4, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4)
                    return new LocalColumnExpression<T1, T2, T3, T4, TDataType>(column1, column2, column3, column4, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, Func<DataRow, T1, T2, T3, T4, T5, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, TDataType>(column1, column2, column3, column4, column5, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, Func<DataRow, T1, T2, T3, T4, T5, T6, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5 || column6 != _column6)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, TDataType>(column1, column2, column3, column4, column5, column6, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _column7 = column7;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Union(column7.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly T7 _column7;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6, _column7); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                var column7 = _column7.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5
                    || column6 != _column6 || column7 != _column7)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, TDataType>
                        (column1, column2, column3, column4, column5, column6, column7, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8,
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _column7 = column7;
                _column8 = column8;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Union(column7.BaseColumns).Union(column8.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly T7 _column7;
            private readonly T8 _column8;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6, _column7, _column8); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                var column7 = _column7.TranslateTo(model);
                var column8 = _column8.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5
                    || column6 != _column6 || column7 != _column7 || column8 != _column8)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, TDataType>
                        (column1, column2, column3, column4, column5, column6, column7, column8, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8, T9 column9,
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _column7 = column7;
                _column8 = column8;
                _column9 = column9;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Union(column7.BaseColumns).Union(column8.BaseColumns)
                    .Union(column9.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly T7 _column7;
            private readonly T8 _column8;
            private readonly T9 _column9;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6, _column7, _column8, _column9); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                var column7 = _column7.TranslateTo(model);
                var column8 = _column8.TranslateTo(model);
                var column9 = _column9.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5
                    || column6 != _column6 || column7 != _column7 || column8 != _column8 || column9 != _column9)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDataType>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8, T9 column9, T10 column10,
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _column7 = column7;
                _column8 = column8;
                _column9 = column9;
                _column10 = column10;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Union(column7.BaseColumns).Union(column8.BaseColumns)
                    .Union(column9.BaseColumns).Union(column10.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly T7 _column7;
            private readonly T8 _column8;
            private readonly T9 _column9;
            private readonly T10 _column10;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6, _column7, _column8, _column9, _column10); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                var column7 = _column7.TranslateTo(model);
                var column8 = _column8.TranslateTo(model);
                var column9 = _column9.TranslateTo(model);
                var column10 = _column10.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5
                    || column6 != _column6 || column7 != _column7 || column8 != _column8 || column9 != _column9 || column10 != _column10)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDataType>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8, T9 column9, T10 column10, T11 column11,
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _column7 = column7;
                _column8 = column8;
                _column9 = column9;
                _column10 = column10;
                _column11 = column11;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Union(column7.BaseColumns).Union(column8.BaseColumns)
                    .Union(column9.BaseColumns).Union(column10.BaseColumns).Union(column11.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly T7 _column7;
            private readonly T8 _column8;
            private readonly T9 _column9;
            private readonly T10 _column10;
            private readonly T11 _column11;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6, _column7, _column8, _column9, _column10, _column11); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                var column7 = _column7.TranslateTo(model);
                var column8 = _column8.TranslateTo(model);
                var column9 = _column9.TranslateTo(model);
                var column10 = _column10.TranslateTo(model);
                var column11 = _column11.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5
                    || column6 != _column6 || column7 != _column7 || column8 != _column8 || column9 != _column9 || column10 != _column10 || column11 != _column11)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDataType>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, _expression);
                else
                    return this;
            }

        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDataType> : LocalColumnExpressionBase<TDataType>
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
            where T12 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8, T9 column9, T10 column10,
                T11 column11, T12 column12, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDataType> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _column3 = column3;
                _column4 = column4;
                _column5 = column5;
                _column6 = column6;
                _column7 = column7;
                _column8 = column8;
                _column9 = column9;
                _column10 = column10;
                _column11 = column11;
                _column12 = column12;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Union(column3.BaseColumns).Union(column4.BaseColumns)
                    .Union(column5.BaseColumns).Union(column6.BaseColumns).Union(column7.BaseColumns).Union(column8.BaseColumns)
                    .Union(column9.BaseColumns).Union(column10.BaseColumns).Union(column11.BaseColumns).Union(column12.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly T3 _column3;
            private readonly T4 _column4;
            private readonly T5 _column5;
            private readonly T6 _column6;
            private readonly T7 _column7;
            private readonly T8 _column8;
            private readonly T9 _column9;
            private readonly T10 _column10;
            private readonly T11 _column11;
            private readonly T12 _column12;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDataType> _expression;
            public override TDataType this[DataRow dataRow]
            {
                get { return _expression(dataRow, _column1, _column2, _column3, _column4, _column5, _column6, _column7, _column8, _column9, _column10, _column11, _column12); }
            }

            protected override IColumns GetBaseColumns()
            {
                return _baseColumns;
            }

            protected internal override ColumnExpression PerformTranslateTo(Model model)
            {
                var column1 = _column1.TranslateTo(model);
                var column2 = _column2.TranslateTo(model);
                var column3 = _column3.TranslateTo(model);
                var column4 = _column4.TranslateTo(model);
                var column5 = _column5.TranslateTo(model);
                var column6 = _column6.TranslateTo(model);
                var column7 = _column7.TranslateTo(model);
                var column8 = _column8.TranslateTo(model);
                var column9 = _column9.TranslateTo(model);
                var column10 = _column10.TranslateTo(model);
                var column11 = _column11.TranslateTo(model);
                var column12 = _column12.TranslateTo(model);
                if (column1 != _column1 || column2 != _column2 || column3 != _column3 || column4 != _column4 || column5 != _column5 || column6 != _column6
                    || column7 != _column7 || column8 != _column8 || column9 != _column9 || column10 != _column10 || column11 != _column11 || column12 != _column12)
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDataType>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, column12, _expression);
                else
                    return this;
            }
        }

        private void VerifyModel(Model model, string paramName)
        {
            Check.NotNull(model, paramName);
            if (model.DataSetContainer != this)
                throw new ArgumentException(DiagnosticMessages.DataSetContainer_InvalidLocalColumnModel, paramName);
        }

        private void VerifyBaseColumn(Column column, string paramName)
        {
            Check.NotNull(column, paramName);
            if (column.ParentModel != null && column.ParentModel.DataSetContainer != this)
                throw new ArgumentException(DiagnosticMessages.DataSetContainer_InvalidBaseColumn, paramName);
        }

        private void VerifyExpression(Delegate expression, string paramName)
        {
            Check.NotNull(expression, paramName);
            if (expression.Target != null)
                throw new ArgumentException(DiagnosticMessages.DataSetContainer_InvalidLocalColumnExpression, paramName);
        }

        private Column<T> AddLocalColumn<T>(Column<T> localColumn)
        {
            if (localColumn.IsExpression)
                MergeComputation(localColumn);
            return localColumn;
        }

        public Column<T> CreateLocalColumn<T>(Model model, Action<LocalColumnBuilder<T>> builder = null)
        {
            VerifyModel(model, nameof(model));
            var result = new LocalColumn<T>(model, null, builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T>(Model model, T constValue, Action<LocalColumnBuilder<T>> builder = null)
        {
            VerifyModel(model, nameof(model));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T>(x => constValue), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T>(Model model, Func<DataRow, T> expression, Action<LocalColumnBuilder<T>> builder = null)
        {
            VerifyModel(model, nameof(model));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T>(expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T>(Model model, T1 column, Func<DataRow, T1, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column, nameof(column));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T>(column, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T>(Model model, T1 column1, T2 column2, Func<DataRow, T1, T2, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T>(column1, column2, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T>(Model model, T1 column1, T2 column2, T3 column3,
            Func<DataRow, T1, T2, T3, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T>(column1, column2, column3, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4,
            Func<DataRow, T1, T2, T3, T4, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T>(column1, column2, column3, column4, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5,
            Func<DataRow, T1, T2, T3, T4, T5, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T>(column1, column2, column3, column4, column5, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T>(column1, column2, column3, column4, column5, column6, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T>(
                column1, column2, column3, column4, column5, column6, column7, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T>(
                column1, column2, column3, column4, column5, column6, column7, column8, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyBaseColumn(column9, nameof(column9));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyBaseColumn(column9, nameof(column9));
            VerifyBaseColumn(column10, nameof(column10));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> expression,
            Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyBaseColumn(column9, nameof(column9));
            VerifyBaseColumn(column10, nameof(column10));
            VerifyBaseColumn(column11, nameof(column11));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, expression), builder);
            return AddLocalColumn(result);
        }

        public Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T>(Model model, T1 column1, T2 column2, T3 column3, T4 column4,
            T5 column5, T6 column6, T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, T12 column12,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
            where T12 : Column
        {
            VerifyModel(model, nameof(model));
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyBaseColumn(column9, nameof(column9));
            VerifyBaseColumn(column10, nameof(column10));
            VerifyBaseColumn(column11, nameof(column11));
            VerifyBaseColumn(column12, nameof(column12));
            VerifyExpression(expression, nameof(expression));
            var result = new LocalColumn<T>(model, new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, column12, expression), builder);
            return AddLocalColumn(result);
        }
    }
}
