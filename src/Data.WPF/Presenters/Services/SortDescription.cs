using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Services
{
    internal sealed class SortDescription : Model
    {
        public Column<Column> Column { get; private set; }
        public Column<SortDirection> Direction { get; private set; }

        protected override void OnInitializing()
        {
            Column = CreateLocalColumn<Column>(builder => builder.DisplayName = Strings.SortDescription_Column);
            Direction = CreateLocalColumn<SortDirection>(builder => builder.DisplayName = Strings.SortDescription_Direction);
            base.OnInitializing();
        }

        protected override IValidationMessageGroup Validate(DataRow dataRow, ValidationSeverity? severity)
        {
            var result = base.Validate(dataRow, severity);
            if (severity == ValidationSeverity.Warning)
                return result;

            if (Column[dataRow] == null)
                result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, Strings.SortDescription_InputRequired(Column.DisplayName), Column));
            if (Direction[dataRow] == SortDirection.Unspecified)
                result = result.Add(new ValidationMessage(null, ValidationSeverity.Error, Strings.SortDescription_InputRequired(Direction.DisplayName), Direction));

            return result;
        }

        public static DataSet<SortDescription> Convert(IReadOnlyList<ColumnSort> orderBy)
        {
            var result = DataSet<SortDescription>.New();
            if (orderBy == null || orderBy.Count == 0)
                return result;

            for (int i = 0; i < orderBy.Count; i++)
            {
                var columnSort = orderBy[i];
                result.AddRow((_, dataRow) =>
                {
                    _.Column[dataRow] = columnSort.Column;
                    _.Direction[dataRow] = columnSort.Direction;
                });
            }

            return result;
        }

        public static IReadOnlyList<ColumnSort> Convert(DataSet<SortDescription> sortDescriptions)
        {
            if (sortDescriptions == null || sortDescriptions.Count == 0)
                return Array<ColumnSort>.Empty;

            Debug.Assert(sortDescriptions.Validate(ValidationSeverity.Error).Count == 0);

            var result = new ColumnSort[sortDescriptions.Count];
            for (int i = 0; i < sortDescriptions.Count; i++)
            {
                var column = sortDescriptions._.Column[i];
                var direction = sortDescriptions._.Direction[i];
                Debug.Assert(direction == SortDirection.Ascending || direction == SortDirection.Descending);
                result[i] = direction == SortDirection.Descending ? column.Desc() : column.Asc();
            }

            return result;
        }
    }
}
