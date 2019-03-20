using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DevZest.Data.AspNetCore.Primitives
{
    public abstract class DataSetTagHelperBase : TagHelper
    {
        protected const string DataSetAttributes = DataSetForAttributeName + "," + ColumnAttributeName;

        private const string DataSetForAttributeName = "dataset-for";
        private const string ColumnAttributeName = "dataset-column";
        private const string DataRowAttributeName = "dataset-row";

        protected DataSetTagHelperBase(IDataSetHtmlGenerator generator)
        {
            Generator = generator;
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IDataSetHtmlGenerator Generator { get; }

        /// <summary>
        /// An <see cref="DataSet"/> expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(DataSetForAttributeName)]
        public ModelExpression DataSetFor { get; set; }


        [HtmlAttributeName(ColumnAttributeName)]
        public Column Column { get; set; }


        private DataRow _dataRow;
        [HtmlAttributeName(DataRowAttributeName)]
        public DataRow DataRow
        {
            get { return _dataRow ?? (IsScalar && DataSet.Count > 0 ? DataSet[0] : null); }
            set { _dataRow = value; }
        }

        protected DataSet DataSet
        {
            get { return DataSetFor.Model as DataSet; }
        }

        protected bool IsScalar
        {
            get { return DataSetFor.IsScalar(); }
        }

        protected object DataValue
        {
            get { return DataRow == null ? null : Column.GetValue(DataRow); }
        }

        protected string FullHtmlFieldName { get; private set; }

        public sealed override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            FullHtmlFieldName = ViewContext.GetFullHtmlFieldName(DataSetFor, Column, DataRow);

            return ProcessOverrideAsync(context, output);
        }

        protected abstract Task ProcessOverrideAsync(TagHelperContext context, TagHelperOutput output);
    }
}
