using System;

namespace DevZest.Data
{
    /// <summary>
    /// Specifies database operations logging category.
    /// </summary>
    [Flags]
    public enum LogCategory
    {
        /// <summary>
        /// Database command text.
        /// </summary>
        CommandText = 0x01,

        /// <summary>
        /// Database connection opening.
        /// </summary>
        ConnectionOpening = 0x02,

        /// <summary>
        /// Database connection opened.
        /// </summary>
        ConnectionOpened = 0x04,

        /// <summary>
        /// Database connection closing.
        /// </summary>
        ConnectionClosing = 0x08,

        /// <summary>
        /// Database connection closed.
        /// </summary>
        ConnectionClosed = 0x10,

        /// <summary>
        /// Database transaction beginning.
        /// </summary>
        TransactionBeginning = 0x20,

        /// <summary>
        /// Database transaction began.
        /// </summary>
        TransactionBegan = 0x40,

        /// <summary>
        /// Database transation committing.
        /// </summary>
        TransactionCommitting = 0x80,

        /// <summary>
        /// Database transaction committed.
        /// </summary>
        TransactionCommitted = 0x100,

        /// <summary>
        /// Database transaction rolling back.
        /// </summary>
        TransactionRollingBack = 0x200,

        /// <summary>
        /// Database transaction rolled back.
        /// </summary>
        TransactionRolledBack = 0x400,

        /// <summary>
        /// Database command executing.
        /// </summary>
        CommandExecuting = 0x800,

        /// <summary>
        /// Database command executed.
        /// </summary>
        CommandExecuted = 0x1000,

        /// <summary>
        /// All database command related operations.
        /// </summary>
        Command = CommandText | CommandExecuting | CommandExecuted,

        /// <summary>
        /// All database connection related operations.
        /// </summary>
        Connection = ConnectionOpening | ConnectionOpened | ConnectionClosing | ConnectionClosed,

        /// <summary>
        /// All database transation related operations.
        /// </summary>
        Trsansaction = TransactionBeginning | TransactionBegan | TransactionCommitting | TransactionCommitted | TransactionRollingBack | TransactionRolledBack,

        /// <summary>
        /// All database operations.
        /// </summary>
        All = 0x1FFF
    }
}
