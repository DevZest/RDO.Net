using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public abstract class ScalarData
    {
        private sealed class _Model : Model
        {
        }

        private _Model _model = new _Model();
        internal ColumnCollection Accessors
        {
            get { return _model.Columns; }
        }

        private DataSet<_Model> _dataSet;

        protected T CreateAccessor<T>(string name, Action<T> initializer = null)
            where T : Column, new()
        {
            Check.NotEmpty(name, nameof(name));

            var result = new T();
            if (initializer != null)
                initializer(result);
            result.Initialize(_model, typeof(_Model), name, ColumnKind.User, null);
            return result;
        }

        private void EnsureDataSetInitialized()
        {
            if (_dataSet == null)
            {
                _dataSet = DataSet<_Model>.Create(_model);
                _dataSet.AddRow();
            }
        }

        private void VerifyAccessor<T>(Column<T> accessor, string paramName)
        {
            Check.NotNull(accessor, paramName);

            if (accessor.ParentModel == null || accessor.ParentModel != _model)
                throw new ArgumentException(Strings.ScalarData_InvalidAccessor, paramName);
        }

        protected T GetValue<T>(Column<T> accessor)
        {
            VerifyAccessor(accessor, nameof(accessor));
            EnsureDataSetInitialized();
            return accessor[0];
        }

        protected void SetValue<T>(Column<T> accessor, T value)
        {
            VerifyAccessor(accessor, nameof(accessor));
            EnsureDataSetInitialized();
            accessor[0] = value;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        public string ToJsonString(bool isPretty)
        {
            EnsureDataSetInitialized();
            return _dataSet.ToJsonString(isPretty);
        }

        public static T ParseJson<T>(string json)
            where T : ScalarData, new()
        {
            Check.NotEmpty(json, nameof(json));

            var result = new T();
            var dataSet = result._dataSet;
            if (dataSet != null)
                dataSet.RemoveAt(0);
            new DataSetJsonParser(json).Parse(result._dataSet, true);
            return dataSet.Count == 1 ? result : null;
        }
    }
}
