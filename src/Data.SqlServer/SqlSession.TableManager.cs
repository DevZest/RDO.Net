using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevZest.Data.SqlServer
{
    partial class SqlSession
    {
        private abstract class TableManager
        {
            protected TableManager(SqlConnection sqlConnection)
            {
                Debug.Assert(sqlConnection != null);
                SqlConnection = sqlConnection;
            }

            protected SqlConnection SqlConnection { get; private set; }

            public abstract string GetTableName(string name);

            public abstract string AssignTempTableName(Model model);

            private sealed class ReleaseMode : TableManager
            {
                public ReleaseMode(SqlConnection sqlConnection)
                    : base(sqlConnection)
                {
                }

                ConditionalWeakTable<Model, string> _tempTableNamesByModel = new ConditionalWeakTable<Model, string>();
                Dictionary<string, int> _tempTableNameSuffixes = new Dictionary<string, int>();

                public override string GetTableName(string name)
                {
                    return name;
                }

                private string GetUniqueTempTableName(Model model)
                {
                    Debug.Assert(model != null);

                    return "#" + _tempTableNameSuffixes.GetUniqueName(model.GetType().Name);
                }

                public override string AssignTempTableName(Model model)
                {
                    return _tempTableNamesByModel.GetValue(model, GetUniqueTempTableName);
                }
            }
        }
    }
}
