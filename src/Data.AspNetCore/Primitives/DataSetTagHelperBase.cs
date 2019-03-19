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

        [HtmlAttributeName(DataRowAttributeName)]
        public DataRow DataRow { get; set; }

        public sealed override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (DataSetFor == null)
                throw new ArgumentNullException(nameof(DataSetFor));

            var dataSet = DataSetFor.Model as DataSet;
            var isScalar = DataSetFor.Metadata.IsScalar();

            var fullHtmlFieldName = GetFullHtmlFieldName(ViewContext, DataSetFor.Name, dataSet, isScalar, Column, DataRow);

            return ProcessAsync(context, output, fullHtmlFieldName, Column);
        }

        private static string GetFullHtmlFieldName(ViewContext viewContext, string expression, DataSet dataSet, bool isScalar, Column column, DataRow dataRow)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            if (column == null)
                throw new ArgumentNullException(nameof(column));

            VerifyDataRow(dataRow, column, dataSet, isScalar);

            var memberName = ResolveMemberName(dataSet, isScalar, column, dataRow);
            var fullDataSetName = NameAndIdProvider.GetFullHtmlFieldName(viewContext, expression);
            return ModelNames.CreatePropertyModelName(fullDataSetName, memberName);
        }

        private static void VerifyDataRow(DataRow dataRow, Column column, DataSet dataSet, bool isScalar)
        {
            if (dataRow == null)
            {
                if (!isScalar)
                    throw new ArgumentNullException(nameof(dataRow));

                return;
            }

            if (dataRow.Model != column.GetParent())
                throw new ArgumentException(DiagnosticMessages.InvalidDataRowForColumn, nameof(dataRow));

            if (!IsValidDataRow(dataRow, dataSet))
                throw new ArgumentException(DiagnosticMessages.InvalidDataRowForDataSet, nameof(dataRow));
        }

        private static bool IsValidDataRow(DataRow dataRow, DataSet dataSet)
        {
            if (dataRow.Model == dataSet.Model)
                return true;

            var parentDataRow = dataRow.ParentDataRow;
            return parentDataRow == null ? false : IsValidDataRow(parentDataRow, dataSet);
        }

        private static string ResolveMemberName(DataSet dataSet, bool isScalar, Column column, DataRow dataRow)
        {
            if (column.GetParent() == dataSet.Model)
                return isScalar ? column.Name : ModelNames.CreatePropertyModelName(ModelNames.CreateIndexModelName(string.Empty, dataRow.Index), column.Name);

            Debug.Assert(dataRow != null);
            return ResolveMemberName(dataSet, isScalar, dataRow, column.Name);
        }

        private static string ResolveMemberName(DataSet dataSet, bool isScalar, DataRow dataRow, string memberName)
        {
            if (dataRow.Model == dataSet.Model)
                return isScalar ? memberName : ModelNames.CreatePropertyModelName(ModelNames.CreateIndexModelName(string.Empty, dataRow.Index), memberName);

            memberName = ModelNames.CreatePropertyModelName(ModelNames.CreateIndexModelName(dataRow.Model.GetName(), dataRow.Index), memberName);
            Debug.Assert(dataRow.ParentDataRow != null);
            return ResolveMemberName(dataSet, isScalar, dataRow.ParentDataRow, memberName);
        }

        protected abstract Task ProcessAsync(TagHelperContext context, TagHelperOutput output, string fullHtmlFieldName, Column column);
    }
}
