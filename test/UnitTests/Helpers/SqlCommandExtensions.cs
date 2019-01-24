//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Data.SqlClient;
//using DevZest.Data.SqlServer;
//using System.Collections.Generic;

//namespace DevZest.Data.Helpers
//{
//    internal static class SqlCommandExtensions
//    {
//        internal static void Verify(this SqlCommand command, string expectedSql, bool removeGuids = false)
//        {
//            var commandText = command.ToTraceString().Trim();
//            if (removeGuids)
//                commandText = commandText.RemoveGuids();
//            Assert.AreEqual(expectedSql.Trim(), commandText);
//        }

//        internal static void Verify(this IList<SqlCommand> commands, params string[] commandTextList)
//        {
//            commands.Verify(false, commandTextList);
//        }

//        internal static void Verify(this IList<SqlCommand> commands, bool removeGuids, params string[] commandTextList)
//        {
//            Assert.AreEqual(commandTextList.Length, commands.Count);
//            for (int i = 0; i < commands.Count; i++)
//                commands[i].Verify(commandTextList[i], removeGuids);
//        }
//    }
//}
