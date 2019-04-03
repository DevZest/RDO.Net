using System;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    public abstract class DataSetTagHelperBase : TagHelper
    {
        protected const string DataSetAttributes = DataSetForAttributeName + "," + ColumnAttributeName;

        protected const string DataSetForAttributeName = "dataset-for";
        protected const string ColumnAttributeName = "dataset-column";
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
        public virtual ModelExpression DataSetFor { get; set; }


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

        protected internal string FullHtmlFieldName { get; private set; }

        public override void Init(TagHelperContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            FullHtmlFieldName = ViewContext.GetFullHtmlFieldName(DataSetFor, Column, DataRow);
        }
    }
}
