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

    /// <summary>
    /// Represents a subset view of DataSet JSON serialization.
    /// </summary>
    public abstract class JsonView : IJsonView
    {
        internal JsonView(JsonFilter jsonFilter)
        {
            jsonFilter.VerifyNotNull(nameof(jsonFilter));
            Filter = jsonFilter;
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        public abstract Model Model { get; }

        /// <summary>
        /// Gets the <see cref="JsonFilter"/>.
        /// </summary>
        public JsonFilter Filter { get; private set; }

        /// <summary>
        /// Gets the child <see cref="JsonView"/> objects.
        /// </summary>
        public abstract IReadOnlyList<JsonView> Children { get; }

        IJsonView IJsonView.GetChildView(DataSet childDataSet)
        {
            var result = Children[childDataSet.Model.Ordinal];
            if (result != null)
                return result;
            else
                return childDataSet;
        }

        /// <summary>
        /// Serializes into JSON string.
        /// </summary>
        /// <param name="isPretty">Specifies whether the serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer for JSON serialization.</param>
        /// <returns>The result JSON string.</returns>
        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return ToJsonString(Model.DataSet, isPretty, customizer);
        }

        /// <summary>
        /// Serializes into JSON string for specified DataRow objects.
        /// </summary>
        /// <param name="dataRows">The specified DataRow objects.</param>
        /// <param name="isPretty">Specifies whether the serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer for JSON serialization.</param>
        /// <returns>The result JSON string.</returns>
        public string ToJsonString(IEnumerable<DataRow> dataRows, bool isPretty, IJsonCustomizer customizer = null)
        {
            dataRows.VerifyNotNull(nameof(dataRows));
            return JsonWriter.Create(customizer).Write(this, dataRows).ToString(isPretty);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToJsonString(true);
        }
    }

    /// <summary>
    /// Represents a subset view of strongly typed DataSet JSON serialization.
    /// </summary>
    /// <typeparam name="T">Type of Entity.</typeparam>
    public sealed class JsonView<T> : JsonView
        where T : class, IEntity, new()
    {
        internal JsonView(T entity, JsonFilter jsonFilter)
            : base(jsonFilter)
        {
            Debug.Assert(entity != null);
            Debug.Assert(jsonFilter != null);
            _ = entity;
            _children = new JsonView[_.Model.ChildModels.Count];
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        public T _ { get; private set; }

        /// <inheritdoc />
        public override Model Model { get { return _.Model; } }

        private readonly JsonView[] _children;
        /// <inheritdoc/>
        public override IReadOnlyList<JsonView> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Filters the child views.
        /// </summary>
        /// <param name="childJsonViews">The child JSON views.</param>
        /// <returns>The child views.</returns>
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
