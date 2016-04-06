using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System;
using System.Linq;
using DevZest.Data.Windows.Factories;
using DevZest.Data.Windows.Primitives;

namespace DevZest.Data.Windows
{
    public sealed partial class DataPresenter : LayoutManager
    {
        internal static DataPresenter Create<T>(RowPresenter owner, T childModel, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            Debug.Assert(owner != null);
            Debug.Assert(childModel != null);
            Debug.Assert(buildTemplateAction != null);

            var result = new DataPresenter(owner, owner.DataRow[childModel]);
            using (var templateBuilder = new TemplateBuilder(result.Template, childModel))
            {
                buildTemplateAction(templateBuilder, childModel);
            }

            return result;
        }

        internal static DataPresenter Create<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction = null)
            where T : Model, new()
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            return Create<T>(null, dataSet, buildTemplateAction);
        }

        private static DataPresenter Create<T>(RowPresenter owner, DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var model = dataSet._;
            var result = new DataPresenter(owner, dataSet);
            using (var templateBuilder = new TemplateBuilder(result.Template, dataSet.Model))
            {
                if (buildTemplateAction != null)
                    buildTemplateAction(templateBuilder, model);
                else
                    BuildDefaultTemplate(templateBuilder, model);
            }

            return result;
        }

        private static void BuildDefaultTemplate(TemplateBuilder templateBuilder, Model model)
        {
            var columns = model.GetColumns();
            if (columns.Count == 0)
                return;

            templateBuilder.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .RowRange(0, 1, columns.Count - 1, 1);

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                templateBuilder[i, 0].ColumnHeader(column)
                    [i, 1].TextBlock(column);
            }
        }

        private readonly RowPresenter _parent;
        public RowPresenter Parent
        {
            get { return _parent; }
        }


        private DataPresenter(RowPresenter parent, DataSet dataSet)
            : base(dataSet)
        {
            Debug.Assert(dataSet != null);
            Debug.Assert(dataSet.ParentRow == null || dataSet.ParentRow == parent.DataRow);

            _parent = parent;
        }
    }
}
