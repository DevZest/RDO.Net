namespace DevZest.Data
{
    public struct ValidationEntry
    {
        public ValidationEntry(DataRow dataRow, ValidationMessage message)
        {
            DataRow = dataRow;
            Message = message;
        }

        public readonly DataRow DataRow;

        public readonly ValidationMessage Message;
    }
}
