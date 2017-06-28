using DevZest.Data.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Presenters.Services
{
    internal sealed class SortModel : Model
    {
        public Column<ColumnHeader> Column { get; private set; }
        public Column<SortDirection> Direction { get; private set; }

        protected override void OnInitializing()
        {
            Column = CreateLocalColumn<ColumnHeader>(builder => builder.DisplayName = UIText.SortModel_Column);
            Direction = CreateLocalColumn<SortDirection>(builder => builder.DisplayName = UIText.SortModel_Direction);
            base.OnInitializing();
        }

        protected override IValidationMessageGroup Validate(DataRow dataRow, ValidationSeverity? severity)
        {
            var result = base.Validate(dataRow, severity);
            if (severity == ValidationSeverity.Warning)
                return result;

            if (Column[dataRow] == null)
                result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, UIText.SortModel_InputRequired(Column.DisplayName), Column));
            if (Direction[dataRow] == SortDirection.Unspecified)
                result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, UIText.SortModel_InputRequired(Direction.DisplayName), Direction));

            return result;
        }

        public static DataSet<SortModel> GetData(ISortService sortService)
        {
            var dataPresenter = sortService.DataPresenter;
            var orderBy = sortService.OrderBy;
            var result = DataSet<SortModel>.New();
            if (orderBy == null || orderBy.Count == 0)
                return result;

            var columnHeaders = ColumnHeader.GetColumnHeaders(dataPresenter); 
            for (int i = 0; i < orderBy.Count; i++)
            {
                var orderByItem = orderBy[i];
                result.AddRow((_, dataRow) =>
                {
                    var column = orderByItem.GetColumn(dataPresenter.DataSet.Model);
                    _.Column[dataRow] = columnHeaders.First(x => x.Column == column);
                    _.Direction[dataRow] = orderByItem.Direction;
                });
            }

            return result;
        }

        public static void Apply(ISortService sortService, DataSet<SortModel> sortData)
        {
            sortService.OrderBy = GetOrderBy(sortData);
        }

        private static IReadOnlyList<IColumnComparer> GetOrderBy(DataSet<SortModel> sortData)
        {
            if (sortData == null || sortData.Count == 0)
                return Array<IColumnComparer>.Empty;

            Debug.Assert(sortData.Validate(ValidationSeverity.Error).Count == 0);

            var _ = sortData._;
            var result = new IColumnComparer[sortData.Count];
            for (int i = 0; i < sortData.Count; i++)
            {
                var column = _.Column[i];
                var direction = _.Direction[i];
                Debug.Assert(direction == SortDirection.Ascending || direction == SortDirection.Descending);
                result[i] = column.GetColumnComparer(direction);
            }

            return result;
        }
    }
}
