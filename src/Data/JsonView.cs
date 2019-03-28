using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    internal interface IJsonView
    {
        Model Model { get; }
        JsonFilter Filter { get; }
        IJsonView GetChildView(DataSet childDataSet);
    }

    public abstract class JsonView : IJsonView
    {
        internal JsonView(JsonFilter jsonFilter)
        {
            jsonFilter.VerifyNotNull(nameof(jsonFilter));
            Filter = jsonFilter;
        }

        public abstract Model Model { get; }

        public JsonFilter Filter { get; private set; }

        public abstract IReadOnlyList<JsonView> Children { get; }

        IJsonView IJsonView.GetChildView(DataSet childDataSet)
        {
            var result = Children[childDataSet.Model.Ordinal];
            if (result != null)
                return result;
            else
                return childDataSet;
        }

        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return ToJsonString(Model.DataSet, isPretty, customizer);
        }

        public string ToJsonString(IEnumerable<DataRow> dataRows, bool isPretty, IJsonCustomizer customizer = null)
        {
            dataRows.VerifyNotNull(nameof(dataRows));
            return JsonWriter.Create(customizer).Write(this, dataRows).ToString(isPretty);
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }
    }

    public sealed class JsonView<T> : JsonView
        where T : class, IModelReference, new()
    {
        internal JsonView(T modelRef, JsonFilter jsonFilter)
            : base(jsonFilter)
        {
            Debug.Assert(modelRef != null);
            Debug.Assert(jsonFilter != null);
            _ = modelRef;
            _children = new JsonView[_.Model.ChildModels.Count];
        }

        public T _ { get; private set; }

        public override Model Model { get { return _.Model; } }

        private readonly JsonView[] _children;
        public override IReadOnlyList<JsonView> Children
        {
            get { return _children; }
        }

        public JsonView<T> FilterChildren(params JsonView[] childJsonViews)
        {
            childJsonViews.VerifyNotNull(nameof(childJsonViews));
            
            for (int i = 0; i < childJsonViews.Length; i++)
            {
                var childJsonView = childJsonViews[i];
                if (childJsonView == null)
                    throw new ArgumentNullException(string.Format("{0}.[{1}]", nameof(childJsonViews), i));
                var childModel = childJsonView.Model;
                if (childModel.ParentModel != Model)
                    throw new ArgumentException("", string.Format("{0}.[{1}]", nameof(childJsonViews), i));
                _children[childModel.Ordinal] = childJsonView;
            }
            return this;
        }
    }
}
