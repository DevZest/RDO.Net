using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a row of in-memory data.
    /// </summary>
    public sealed class DataRow
    {
        /// <summary>Initializes a new instance of <see cref="DataRow"/> object.</summary>
        public DataRow()
        {
            Ordinal = -1;
            ChildOrdinal = -1;
        }

        private DataSet[] _childDataSets;

        /// <summary>Gets the <see cref="Model"/> which associated with this <see cref="DataRow"/>.</summary>
        public Model Model { get; private set; }

        internal int Ordinal { get; private set; }

        internal int ChildOrdinal { get; private set; }

        /// <summary>Gets the parent <see cref="DataRow"/>.</summary>
        public DataRow Parent { get; private set; }

        internal void InitializeBySubDataSet(DataRow parent, int childOrdinal)
        {
            Debug.Assert(Parent == null);
            Debug.Assert(parent != null);

            Parent = parent;
            ChildOrdinal = childOrdinal;
        }

        internal void DisposeBySubDataSet()
        {
            Parent = null;
        }

        internal void InitializeByMainDataSet(Model model, int ordinal)
        {
            Debug.Assert(Model == null);
            Debug.Assert(model != null);

            Model = model;
            Ordinal = ordinal;

            model.EnsureChildModelsInitialized();
            var childModels = model.ChildModels;
            _childDataSets = new DataSet[childModels.Count];
            for (int i = 0; i < childModels.Count; i++)
                _childDataSets[i] = childModels[i].DataSet.CreateSubDataSet(this);

            var columns = model.Columns;
            foreach (var column in columns)
                column.InsertRow(this);
        }

        internal void DisposeByMainDataSet()
        {
            ClearChildren();

            var columns = Model.Columns;
            foreach (var column in columns)
                column.RemoveRow(this);

            Model = null;
        }

        internal void AdjustOrdinal(int value)
        {
            Debug.Assert(Ordinal != value);
            Ordinal = value;
        }

        internal void AdjustChildOrdinal(int value)
        {
            Debug.Assert(Ordinal != value);
            ChildOrdinal = value;
        }

        public DataSet this[Model childModel]
        {
            get
            {
                Check.NotNull(childModel, nameof(childModel));
                if (childModel.ParentModel != Model)
                    throw new ArgumentException(Strings.InvalidChildModel, nameof(childModel));
                return _childDataSets[childModel.Ordinal];
            }
        }

        public DataSet this[int ordinal]
        {
            get
            {
                if (ordinal < 0 || ordinal >= _childDataSets.Length)
                    throw new ArgumentOutOfRangeException(nameof(ordinal));
                return _childDataSets[ordinal];
            }
        }

        private void ClearChildren()
        {
            foreach (var dataSet in _childDataSets)
                dataSet.Clear();
        }

        /// <summary>Gets the children data set of this <see cref="DataRow"/>.</summary>
        /// <typeparam name="T">The type of child model.</typeparam>
        /// <param name="childModel">The child model.</param>
        /// <returns>The children data set.</returns>
        public DataSet<T> Children<T>(T childModel)
            where T : Model, new()
        {
            Utilities.Check.NotNull(childModel, nameof(childModel));
            if (childModel.ParentModel != Model)
                throw new ArgumentException(Strings.InvalidChildModel, nameof(childModel));

            return (DataSet<T>)this[childModel.Ordinal];
        }

        internal void BuildJsonString(StringBuilder stringBuilder)
        {
            stringBuilder.Append('{');

            var columns = Model.Columns;
            int count = 0;
            foreach (var column in columns)
            {
                if (!column.ShouldSerialize)
                    continue;

                if (count > 0)
                    stringBuilder.Append(',');
                BuildJsonObjectName(stringBuilder, column.Name);
                column.Serialize(Ordinal).Write(stringBuilder);
                count++;
            }

            foreach (var dataSet in _childDataSets)
            {
                if (count > 0)
                    stringBuilder.Append(',');
                BuildJsonObjectName(stringBuilder, dataSet.Model.Name);
                dataSet.BuildJsonString(stringBuilder);
                count++;
            }

            stringBuilder.Append('}');
        }

        private static void BuildJsonObjectName(StringBuilder stringBuilder, string name)
        {
            stringBuilder.Append("\"");
            stringBuilder.Append(name);
            stringBuilder.Append("\"");
            stringBuilder.Append(":");
        }

        public object this[Column column]
        {
            get { return column.GetValue(this); }
            set { column.SetValue(this, value); }
        }

        public object this[string columnName]
        {
            get { return Model.Columns[columnName].GetValue(this); }
            set { Model.Columns[columnName].SetValue(this, value); }
        }

        public IEnumerable<ValidationResult> Validate()
        {
            foreach (var validator in Model.Validators)
            {
                foreach (var result in validator.Validate(this))
                    yield return result;
            }
        }
    }
}
