﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public class DbGenerator<T> : DbInitializer<T>
        where T : DbSession
    {
        public sealed override async Task<T> GenerateAsync(T db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
        {
            await InitializeAsync(db, nameof(db), progress, ct);
            return db;
        }

        protected sealed override void Initialize()
        {
            InitializeData();
            foreach (var property in GetTableProperties(Db))
            {
                var dbTable = (IDbTable)property.GetValue(Db);
                AddTable(dbTable, GetAction(dbTable));
            }
        }

        private Func<CancellationToken, Task> GetAction(IDbTable dbTable)
        {
            if (_actions == null)
                return null;
            return _actions.TryGetValue(dbTable, out var result) ? result : null;
        }

        private Dictionary<IDbTable, Func<CancellationToken, Task>> _actions;
        protected void SetData<TModel>(DbTable<TModel> dbTable, Func<DataSet<TModel>> getDataSet)
            where TModel : Model, new()
        {
            dbTable.VerifyNotNull(nameof(dbTable));
            getDataSet.VerifyNotNull(nameof(getDataSet));

            if (_actions == null)
                _actions = new Dictionary<IDbTable, Func<CancellationToken, Task>>();
            _actions.Add(dbTable, async (ct) => {
                var dataSet = getDataSet();
                if (dataSet != null)
                {
                    dbTable._.SuspendIdentity();
                    try
                    {
                        await dbTable.InsertAsync(dataSet, ct);
                    }
                    finally
                    {
                        dbTable._.ResumeIdentity();
                    }
                }
            });
        }

        protected virtual void InitializeData()
        {
        }

        private static IEnumerable<PropertyInfo> GetTableProperties(DbSession dbSession)
        {
            var properties = dbSession.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (!typeof(IDbTable).IsAssignableFrom(property.PropertyType))
                    continue;

                if (property.GetGetMethod() == null || property.GetIndexParameters().Length > 0)
                    continue;

                yield return property;
            }
        }
    }
}