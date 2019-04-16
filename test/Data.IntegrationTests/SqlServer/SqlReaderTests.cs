using DevZest.Data.Annotations;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.SqlServer
{
    [TestClass]
    public class SqlReaderTests
    {
        private class Db : SqlSession
        {
            public Db() : base(new SqlConnection(App.GetConnectionString()))
            {
            }

            private DbTable<TestModel> _testModel;
            public DbTable<TestModel> TestModel
            {
                get { return GetTable(ref _testModel); }
            }
        }

        private enum TestEnum
        {
            A = 'A',
            B = 'B',
        }

        private class TestModel : Model<TestModel.PK>
        {
            public sealed class PK : CandidateKey
            {
                public PK(_Int32 id) : base(id)
                {
                }
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(Id);
            }

            public class Key : Key<PK>
            {
                static Key()
                {
                    Register((Key _) => _.Id, _Id);
                }

                protected sealed override PK CreatePrimaryKey()
                {
                    return new PK(Id);
                }

                public _Int32 Id { get; private set; }
            }

            public static readonly Mounter<_Int32> _Id = RegisterColumn((TestModel _) => _.Id);
            public static readonly Mounter<_Char> _CharColumn = RegisterColumn((TestModel _) => _.CharColumn);
            public static readonly Mounter<_CharEnum<TestEnum>> _TestEnumColumn = RegisterColumn((TestModel _) => _.TestEnumColumn);

            public _Int32 Id { get; private set; }

            public _Char CharColumn { get; private set; }

            public _CharEnum<TestEnum> TestEnumColumn { get; private set; }
        }

        private sealed class MockTestModel : DbMock<Db>
        {
            public static Task<Db> CreateAsync(Db db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken))
            {
                return new MockTestModel().MockAsync(db, progress, ct);
            }

            private static DataSet<TestModel> GetTestModel()
            {
                var result = DataSet<TestModel>.Create().AddRows(3);
                var _ = result._;
                _.Id[0] = 1;
                _.Id[1] = 2;
                _.Id[2] = 3;
                _.CharColumn[0] = 'C';
                _.CharColumn[1] = 'D';
                _.TestEnumColumn[0] = TestEnum.A;
                _.TestEnumColumn[1] = TestEnum.B;
                return result;
            }

            protected override void Initialize()
            {
                Mock(Db.TestModel, GetTestModel);
            }
        }

        [TestMethod]
        public async Task SqlReader_ReadChar()
        {
            using (var db = await MockTestModel.CreateAsync(new Db()))
            {
                var result = await db.TestModel.ToDataSetAsync();
                var expected =
@"[
   {
      ""Id"" : 1,
      ""CharColumn"" : ""C"",
      ""TestEnumColumn"" : ""A""
   },
   {
      ""Id"" : 2,
      ""CharColumn"" : ""D"",
      ""TestEnumColumn"" : ""B""
   },
   {
      ""Id"" : 3,
      ""CharColumn"" : null,
      ""TestEnumColumn"" : null
   }
]";
                Assert.AreEqual(expected, result.ToJsonString(true));
            }
        }
    }
}
