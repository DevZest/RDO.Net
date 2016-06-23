using DevZest.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using DevZest.Data.Windows.Factories;
using DevZest.Data.Windows.Primitives;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed partial class DataPresenter
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

            templateBuilder.GridColumns(columns.Select(x => "Auto").ToArray())
                .GridRows("Auto", "Auto")
                .RowRange(0, 1, columns.Count - 1, 1);

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                templateBuilder
                    .ColumnHeader(column).At(i, 0)
                    .TextBlock(column);
            }
        }

        private readonly RowPresenter _parent;
        public RowPresenter Parent
        {
            get { return _parent; }
        }

        private readonly DataSet _dataSet;
        public DataSet DataSet
        {
            get { return _dataSet; }
        }

        private DataPresenter(RowPresenter parent, DataSet dataSet)
        {
            Debug.Assert(dataSet != null);
            Debug.Assert(dataSet.ParentRow == null || dataSet.ParentRow == parent.DataRow);

            _parent = parent;
            _dataSet = dataSet;
        }

        private readonly Template _template = new Template();
        public Template Template
        {
            get { return _template; }
        }

        public bool IsRecursive
        {
            get { return Template.IsRecursive; }
        }

        private LayoutManager _layoutManager;
        internal LayoutManager LayoutManager
        {
            get { return _layoutManager ?? (_layoutManager = LayoutManager.Create(this)); }
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager.CurrentRow; }
        }

        public RowPresenter EditingRow
        {
            get { return LayoutManager.EditingRow; }
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager.SelectedRows; }
        }

        public IReadOnlyList<IBlockPresenter> Blocks
        {
            get { return LayoutManager.Blocks; }
        }

        public IReadOnlyList<UIElement> Elements
        {
            get { return LayoutManager.Elements; }
        }
    }
}
