using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Presenters.Services
{
    /// <summary>
    /// Interaction logic for SortWindow.xaml
    /// </summary>
    internal partial class SortWindow : Window
    {
        private sealed class Model : Data.Model
        {
            public Column<ColumnHeader> ColumnHeader { get; private set; }
            public Column<SortDirection> Direction { get; private set; }

            protected override void OnInitializing()
            {
                ColumnHeader = CreateLocalColumn<ColumnHeader>(builder => builder.DisplayName = UIText.SortModel_Column);
                Direction = CreateLocalColumn<SortDirection>(builder => builder.DisplayName = UIText.SortModel_Direction);
                base.OnInitializing();
            }

            protected override IValidationMessageGroup Validate(DataRow dataRow, ValidationSeverity? severity)
            {
                var result = base.Validate(dataRow, severity);
                if (severity == ValidationSeverity.Warning)
                    return result;

                if (ColumnHeader[dataRow] == null)
                    result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, UIText.SortModel_InputRequired(ColumnHeader.DisplayName), ColumnHeader));
                if (Direction[dataRow] == SortDirection.Unspecified)
                    result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, UIText.SortModel_InputRequired(Direction.DisplayName), Direction));

                return result;
            }

            public static DataSet<Model> GetData(ISortService sortService)
            {
                var dataPresenter = sortService.DataPresenter;
                var orderBy = sortService.OrderBy;
                var result = DataSet<Model>.New();
                if (orderBy == null || orderBy.Count == 0)
                    return result;

                var columnHeaders = GetColumnHeaders(dataPresenter);
                for (int i = 0; i < orderBy.Count; i++)
                {
                    var orderByItem = orderBy[i];
                    result.AddRow((_, dataRow) =>
                    {
                        var column = orderByItem.GetColumn(dataPresenter.DataSet.Model);
                        _.ColumnHeader[dataRow] = columnHeaders.First(x => x.Column == column);
                        _.Direction[dataRow] = orderByItem.Direction;
                    });
                }

                return result;
            }

            private static IReadOnlyList<ColumnHeader> GetColumnHeaders(DataPresenter dataPresenter)
            {
                List<ColumnHeader> result = null;
                var bindings = dataPresenter.Template.ScalarBindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    var columnHeader = bindings[i][0] as ColumnHeader;
                    if (columnHeader != null && columnHeader.Column != null)
                    {
                        if (result == null)
                            result = new List<ColumnHeader>();
                        result.Add(columnHeader);
                    }
                }

                if (result == null)
                    return Array<ColumnHeader>.Empty;
                else
                    return result;
            }

            public static void Apply(ISortService sortService, DataSet<Model> sortData)
            {
                sortService.OrderBy = GetOrderBy(sortData);
            }

            private static IReadOnlyList<IColumnComparer> GetOrderBy(DataSet<Model> sortData)
            {
                if (sortData == null || sortData.Count == 0)
                    return Array<IColumnComparer>.Empty;

                Debug.Assert(sortData.Validate(ValidationSeverity.Error).Count == 0);

                var _ = sortData._;
                var result = new IColumnComparer[sortData.Count];
                for (int i = 0; i < sortData.Count; i++)
                {
                    var columnHeader = _.ColumnHeader[i];
                    var direction = _.Direction[i];
                    Debug.Assert(direction == SortDirection.Ascending || direction == SortDirection.Descending);
                    result[i] = DataRow.OrderBy(columnHeader.Column, direction);
                }

                return result;
            }
        }

        private sealed class Presenter : DataPresenter<Model>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                throw new NotImplementedException();
            }
        }

        public SortWindow()
        {
            InitializeComponent();
        }
    }
}
