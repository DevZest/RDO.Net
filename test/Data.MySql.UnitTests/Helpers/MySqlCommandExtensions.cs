using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace DevZest.Data.MySql.Helpers
{
    internal static class MySqlCommandExtensions
    {
        internal static void Verify(this MySqlCommand command, string expectedSql, bool removeGuids = false)
        {
            var commandText = command.ToTraceString().Trim();
            if (removeGuids)
                commandText = commandText.RemoveGuids();
            Assert.AreEqual(expectedSql.Trim(), commandText);
        }

        internal static void Verify(this IList<MySqlCommand> commands, params string[] commandTextList)
        {
            commands.Verify(false, commandTextList);
        }

        internal static void Verify(this IList<MySqlCommand> commands, bool removeGuids, params string[] commandTextList)
        {
            Assert.AreEqual(commandTextList.Length, commands.Count);
            for (int i = 0; i < commands.Count; i++)
                commands[i].Verify(commandTextList[i], removeGuids);
        }
    }
}
