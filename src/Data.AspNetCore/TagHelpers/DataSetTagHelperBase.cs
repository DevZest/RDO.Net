using System;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    /// <summary>
    /// Base class to provide <see cref="ITagHelper"/> implementation with <c>dataset-*</c> attributes.
    /// </summary>
    public abstract class DataSetTagHelperBase : TagHelper
    {
        /// <summary>
        /// Gets the string of <c>dataset-*</c> attributes, separated by comma.
        /// </summary>
        protected const string DataSetAttributes = DataSetForAttributeName + "," + ColumnAttributeName;

        /// <summary>
        /// Gets the <c>dataset-for</c> attribute name.
        /// </summary>
        protected const string DataSetForAttributeName = "dataset-for";

        /// <summary>
        /// Gets the <c>dataset-row</c> attribute name.
        /// </summary>
        protected const string ColumnAttributeName = "dataset-column";

        private const string DataRowAttributeName = "dataset-row";

        /// <summary>
        /// Intializes a new instance of <see cref="DataSetTagHelperBase"/> class.
        /// </summary>
        /// <param name="generator">The HTML generator.</param>
        protected DataSetTagHelperBase(IDataSetHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <summary>
        /// Gets the view context.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets the HTML generator.
        /// </summary>
        protected IDataSetHtmlGenerator Generator { get; }

        /// <summary>
        /// An <see cref="DataSet"/> expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(DataSetForAttributeName)]
        public virtual ModelExpression DataSetFor { get; set; }

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        [HtmlAttributeName(ColumnAttributeName)]
        public Column Column { get; set; }


        private DataRow _dataRow;
        /// <summary>
        /// Gets or sets the data row.
        /// </summary>
        [HtmlAttributeName(DataRowAttributeName)]
        public DataRow DataRow
        {
            get { return _dataRow ?? (IsScalar && DataSet.Count > 0 ? DataSet[0] : null); }
            set { _dataRow = value; }
        }

        /// <summary>
        /// Gets the DataSet.
        /// </summary>
        protected DataSet DataSet
        {
            get { return DataSetFor.Model as DataSet; }
        }

        /// <summary>
        /// Gets a value indicates whether DataSet is scalar.
        /// </summary>
        protected bool IsScalar
        {
            get { return DataSetFor.IsScalar(); }
        }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        protected object DataValue
        {
            get { return DataRow == null ? null : Column.GetValue(DataRow); }
        }

        /// <summary>
        /// Gets the full HTML field name.
        /// </summary>
        protected internal string FullHtmlFieldName { get; private set; }

        /// <inheritdoc/>
        public override void Init(TagHelperContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            FullHtmlFieldName = ViewContext.GetFullHtmlFieldName(DataSetFor, Column, DataRow);
        }
    }
}
