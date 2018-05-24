using System;

namespace DevZest.Data.Primitives
{
    public interface IColumn
    {
        bool IsReadOnly(DataRow dataRow);
    }

    public interface IColumn<in TReader> : IColumn
        where TReader : DbReader
    {
        void Read(TReader reader, DataRow dataRow);
    }

    public interface IColumn<in TReader, TValue> : IColumn<TReader>
        where TReader : DbReader
    {
        TValue this[TReader reader] { get; }
    }
}
