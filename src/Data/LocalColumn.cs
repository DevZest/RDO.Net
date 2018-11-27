using DevZest.Data.Primitives;
using System;
using System.Diagnostics;

namespace DevZest.Data
{
    internal interface ILocalColumn
    {
        void OnDataRowInserting(DataRow dataRow);
        void OnDataRowRemoving(DataRow dataRow);
    }

    public sealed class LocalColumn<T> : Column<T>, ILocalColumn
    {
        public LocalColumn()
        {
        }

        public LocalColumn(T value)
        {
            Expression = new LocalColumnExpression(dataRow => value);
        }

        internal override bool IsLocal
        {
            get { return true; }
        }

        public override bool IsSerializable
        {
            get { return false; }
        }

        void ILocalColumn.OnDataRowInserting(DataRow dataRow)
        {
            InsertRow(dataRow);
        }

        void ILocalColumn.OnDataRowRemoving(DataRow dataRow)
        {
            RemoveRow(dataRow);
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
            return new LocalColumn<T>(value);
        }

        protected internal override T DeserializeValue(JsonValue value)
        {
            throw new NotSupportedException();
        }

        protected internal override JsonValue SerializeValue(T value)
        {
            throw new NotSupportedException();
        }

        private abstract class LocalColumnExpressionBase : ColumnExpression<T>
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

        private sealed class LocalColumnExpression : LocalColumnExpressionBase
        {
            private static T GetDefaultValue(DataRow dataRow)
            {
                return default(T);
            }

            public LocalColumnExpression(Func<DataRow, T> expression)
            {
                _expression = expression;
            }

            private readonly Func<DataRow, T> _expression;
            public override T this[DataRow dataRow]
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

        private sealed class LocalColumnExpression<T1> : LocalColumnExpressionBase
            where T1 : Column
        {
            public LocalColumnExpression(T1 column, Func<DataRow, T1, T> expression)
            {
                _column = column;
                _expression = expression;
            }

            private readonly T1 _column;
            private readonly Func<DataRow, T1, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1>(column, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2> : LocalColumnExpressionBase
            where T1 : Column
            where T2 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, Func<DataRow, T1, T2, T> expression)
            {
                _column1 = column1;
                _column2 = column2;
                _baseColumns = column1.BaseColumns.Union(column2.BaseColumns).Seal();
                _expression = expression;
            }

            private readonly T1 _column1;
            private readonly T2 _column2;
            private readonly IColumns _baseColumns;
            private readonly Func<DataRow, T1, T2, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2>(column1, column2, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3> : LocalColumnExpressionBase
            where T1 : Column
            where T2 : Column
            where T3 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, Func<DataRow, T1, T2, T3, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3>(column1, column2, column3, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4> : LocalColumnExpressionBase
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, Func<DataRow, T1, T2, T3, T4, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4>(column1, column2, column3, column4, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5> : LocalColumnExpressionBase
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, Func<DataRow, T1, T2, T3, T4, T5, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5>(column1, column2, column3, column4, column5, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6> : LocalColumnExpressionBase
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, Func<DataRow, T1, T2, T3, T4, T5, T6, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6>(column1, column2, column3, column4, column5, column6, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7> : LocalColumnExpressionBase
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
        {
            public LocalColumnExpression(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7>(column1, column2, column3, column4, column5, column6, column7, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8> : LocalColumnExpressionBase
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
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8>
                        (column1, column2, column3, column4, column5, column6, column7, column8, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9> : LocalColumnExpressionBase
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
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : LocalColumnExpressionBase
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
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, _expression);
                else
                    return this;
            }
        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : LocalColumnExpressionBase
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
                Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, _expression);
                else
                    return this;
            }

        }

        private sealed class LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : LocalColumnExpressionBase
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
                T11 column11, T12 column12, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> expression)
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
            private readonly Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> _expression;
            public override T this[DataRow dataRow]
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
                    return new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
                        (column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, column12, _expression);
                else
                    return this;
            }
        }

        private void VerifyBaseColumn(Column column, string paramName)
        {
            column.VerifyNotNull(paramName);
            if (column.ParentModel != null && column.ParentModel.RootModel != this.ParentModel.RootModel)
                throw new ArgumentException(DiagnosticMessages.LocalColumn_InvalidBaseColumn, paramName);
        }

        private void VerifyExpressionAndDesignMode(Delegate expression, string paramName)
        {
            expression.VerifyNotNull(paramName);
            if (expression.Target != null)
                throw new ArgumentException(DiagnosticMessages.LocalColumn_ExpressionIsNotStatic, paramName);
            VerifyDesignMode();
        }

        private void SetExpression(LocalColumnExpressionBase expression, bool isConcrete)
        {
            Expression = expression;
            SetIsConcrete(isConcrete);
        }

        public void ComputedAs(Func<DataRow, T> expression, bool isConcrete)
        {
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression(expression), isConcrete);
        }

        public void ComputedAs<T1>(T1 column, Func<DataRow, T1, T> expression, bool isConcrete)
            where T1 : Column
        {
            VerifyBaseColumn(column, nameof(column));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1>(column, expression), isConcrete);
        }

        public void ComputedAs<T1, T2>(T1 column1, T2 column2, Func<DataRow, T1, T2, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2>(column1, column2, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3>(T1 column1, T2 column2, T3 column3, Func<DataRow, T1, T2, T3, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
            where T3 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3>(column1, column2, column3, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4>(T1 column1, T2 column2, T3 column3, T4 column4,
            Func<DataRow, T1, T2, T3, T4, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4>(column1, column2, column3, column4, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5,
            Func<DataRow, T1, T2, T3, T4, T5, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5>(column1, column2, column3, column4, column5, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6>(column1, column2, column3, column4, column5, column6, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6, T7>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7>(column1, column2, column3, column4, column5, column6, column7, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6, T7, T8>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T> expression, bool isConcrete)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
        {
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8>(
                column1, column2, column3, column4, column5, column6, column7, column8, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> expression, bool isConcrete)
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
            VerifyBaseColumn(column1, nameof(column1));
            VerifyBaseColumn(column2, nameof(column2));
            VerifyBaseColumn(column3, nameof(column3));
            VerifyBaseColumn(column4, nameof(column4));
            VerifyBaseColumn(column5, nameof(column5));
            VerifyBaseColumn(column6, nameof(column6));
            VerifyBaseColumn(column7, nameof(column7));
            VerifyBaseColumn(column8, nameof(column8));
            VerifyBaseColumn(column9, nameof(column9));
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> expression, bool isConcrete)
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
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> expression,
            bool isConcrete)
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
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, expression), isConcrete);
        }

        public void ComputedAs<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 column1, T2 column2, T3 column3, T4 column4,
            T5 column5, T6 column6, T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, T12 column12,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> expression, bool isConcrete)
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
            VerifyExpressionAndDesignMode(expression, nameof(expression));
            SetExpression(new LocalColumnExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, column11, column12, expression), isConcrete);
        }
    }
}
