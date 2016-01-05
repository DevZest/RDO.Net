using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    internal sealed class ValidationManager
    {
        internal ValidationManager(DataSetPresenter owner)
        {
            _owner = owner;
        }

        private DataSetPresenter _owner;
        private static IReadOnlyList<ValidationMessage> s_emptyMessages = new ValidationMessage[0];

        private Dictionary<DataRow, List<ValidationMessage>> _localMessages = new Dictionary<DataRow, List<ValidationMessage>>();
        private Dictionary<DataRow, List<ValidationMessage>> _externalMessages = new Dictionary<DataRow, List<ValidationMessage>>();

        internal IReadOnlyCollection<ValidationMessage> GetLocalMessages(DataRowPresenter dataRowPresenter)
        {
            Debug.Assert(dataRowPresenter != null && dataRowPresenter.Owner == _owner);
            var result = GetMessages(_localMessages, dataRowPresenter);
            if (result == null)
            {

            }

            return result;
        }

        internal IReadOnlyCollection<ValidationMessage> GetExternalMessages(DataRowPresenter dataRowPresenter)
        {
            Debug.Assert(dataRowPresenter != null && dataRowPresenter.Owner == _owner);
            return GetMessages(_externalMessages, dataRowPresenter);
        }

        private static IReadOnlyCollection<ValidationMessage> GetMessages(Dictionary<DataRow, List<ValidationMessage>> messages, DataRowPresenter dataRowPresenter)
        {
            var dataRow = dataRowPresenter.DataRow;
            if (dataRow == null || !messages.ContainsKey(dataRow))
                return s_emptyMessages;

            return messages[dataRow];
        }

        internal void SetExternalMessages(ValidationResult validationResult)
        {
            _externalMessages.Clear();
            for (int i = 0; i < validationResult.Count; i++)
            {
                var entry = validationResult[i];
                //_externalMessages.Add(entry.DataRow, entry.Message);
            }
        }
    }
}
