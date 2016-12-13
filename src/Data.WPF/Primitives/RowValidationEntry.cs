using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    internal struct RowValidationEntry
    {
        public RowValidationEntry(RowPresenter row, IReadOnlyList<ValidationMessage<IColumnSet>> messages)
        {
            Debug.Assert(row != null && messages != null && messages.Count > 0);
            Row = row;
            Messages = messages;
        }

        public readonly RowPresenter Row;
        public readonly IReadOnlyList<ValidationMessage<IColumnSet>> Messages;
    }
}
