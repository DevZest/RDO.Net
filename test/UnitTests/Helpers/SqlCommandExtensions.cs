using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using DevZest.Data.SqlServer;
using System.Collections.Generic;

namespace DevZest.Data.Helpers
{
    internal static class SqlCommandExtensions
    {
        public static void Verify(this SqlCommand command, string expectedSql)
        {
            Assert.AreEqual(expectedSql.Trim(), command.ToTraceString().Trim());
        }

        internal static void Verify(this IList<SqlCommand> commands, params string[] commandTextList)
        {
            Assert.AreEqual(commandTextList.Length, commands.Count);
            for (int i = 0; i < commands.Count; i++)
                commands[i].Verify(commandTextList[i]);
        }
    }
}
