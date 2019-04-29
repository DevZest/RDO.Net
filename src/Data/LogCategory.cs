using System;

namespace DevZest.Data
{
    [Flags]
    public enum LogCategory
    {
        CommandText = 0x01,
        ConnectionOpening = 0x02,
        ConnectionOpened = 0x04,
        ConnectionClosing = 0x08,
        ConnectionClosed = 0x10,
        TransactionBeginning = 0x20,
        TransactionBegan = 0x40,
        TransactionCommitting = 0x80,
        TransactionCommitted = 0x100,
        TransactionRollingBack = 0x200,
        TransactionRolledBack = 0x400,
        CommandExecuting = 0x800,
        CommandExecuted = 0x1000,
        Command = CommandText | CommandExecuting | CommandExecuted,
        Connection = ConnectionOpening | ConnectionOpened | ConnectionClosing | ConnectionClosed,
        Trsansaction = TransactionBeginning | TransactionBegan | TransactionCommitting | TransactionCommitted | TransactionRollingBack | TransactionRolledBack,
        All = 0x1FFF
    }
}
