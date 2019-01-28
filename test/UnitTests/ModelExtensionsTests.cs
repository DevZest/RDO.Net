using DevZest.Data.Annotations;
using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    [TestClass]
    public class ModelExtensionsTests
    {
        private sealed class SampleDb : MySqlSession
        {
            public SampleDb()
            : base(new MySqlConnection())
            {
            }

            private DbTable<SampleModel> _sampleModel;
            [DbTable(Description = "Sample Model Description.")]
            [Relationship(nameof(FK_SampleModel_SampleModel_FkRef), Description = "Foreign key constraint referencing SampleModel.FkRef.")]
            public DbTable<SampleModel> SampleModel
            {
                get { return GetTable(ref _sampleModel); }
            }

            [_Relationship]
            private KeyMapping FK_SampleModel_SampleModel_FkRef(SampleModel _)
            {
                return _.FkRef.Join(_);
            }
        }

        [DbIndex(nameof(IX_SampleModel_Name))]
        private sealed class SampleModel : Model<SampleModel.PK>
        {
            public sealed class PK : CandidateKey
            {
                public PK(_Int32 id)
                    : base(id)
                {
                }
            }

            public static readonly Mounter<_Int32> _Id = RegisterColumn((SampleModel x) => x.Id);
            public static readonly Mounter<_String> _Name = RegisterColumn((SampleModel x) => x.Name);
            public static readonly Mounter<_Int32> _Unique1 = RegisterColumn((SampleModel x) => x.Unique1);
            public static readonly Mounter<_Int32> _Unique2 = RegisterColumn((SampleModel x) => x.Unique2);

            public SampleModel()
            {
                FkRef = new PK(Unique1);
                Name.SetDefaultValue("DEFAULT NAME", null, null);
                AddDbUniqueConstraint("UQ_Temp", null, false, Unique1, Unique2.Desc());
                AddDbCheckConstraint("CK_Temp", null, Name.IsNotNull());
            }

            protected sealed override PK CreatePrimaryKey()
            {
                return new PK(Id);
            }

            [Identity(1, 1)]
            [DbColumn(Description = "Id Description")]
            public _Int32 Id { get; private set; }

            [DbColumn(Description = "Name Description")]
            public _String Name { get; private set; }

            [DbColumn(Description = "Unique1 Description")]
            public _Int32 Unique1 { get; private set; }

            [DbColumn(Description = "Unique2 Description")]
            public _Int32 Unique2 { get; private set; }

            public PK FkRef { get; private set; }

            [_DbIndex]
            private ColumnSort[] IX_SampleModel_Name => new ColumnSort[] { Name };
        }

        [TestMethod]
        public void Model_GenerateCreateTableSql_permanent_table()
        {
            var _ = new SampleDb().SampleModel._;
            var sqlBuilder = new IndentedStringBuilder();

            _.GenerateCreateTableSql(sqlBuilder, MySqlVersion.LowestSupported, false);
            var expectedSql =
@"CREATE TABLE `SampleModel` (
    `Id` INT NOT NULL AUTO_INCREMENT COMMENT 'Id Description',
    `Name` VARCHAR(500) NULL DEFAULT('DEFAULT NAME') COMMENT 'Name Description',
    `Unique1` INT NULL COMMENT 'Unique1 Description',
    `Unique2` INT NULL COMMENT 'Unique2 Description',

    CONSTRAINT `PK_SampleModel` PRIMARY KEY (`Id`),
    CONSTRAINT `UQ_Temp` UNIQUE (`Unique1`, `Unique2` DESC),
    CONSTRAINT `CK_Temp` CHECK (`Name` IS NOT NULL),
    CONSTRAINT `FK_SampleModel_SampleModel_FkRef` FOREIGN KEY (`Unique1`)
        REFERENCES `SampleModel` (`Id`)
        ON DELETE NO ACTION
        ON UPDATE NO ACTION,
    INDEX `IX_SampleModel_Name` (`Name`)
) COMMENT 'Sample Model Description.';
";
            Assert.AreEqual(expectedSql, sqlBuilder.ToString());
        }

        [TestMethod]
        public void Model_GenerateCreateTableSql_temp_table()
        {
            var temp = new SampleModel();
            temp.AddTemporaryTableIdentity();
            var sqlBuilder = new IndentedStringBuilder();

            temp.GenerateCreateTableSql("#Temp", null, sqlBuilder, MySqlVersion.LowestSupported, true);
            var expectedSql =
@"SET @@sql_notes = 0;
DROP TEMPORARY TABLE IF EXISTS `#Temp`;
SET @@sql_notes = 1;

CREATE  TEMPORARY TABLE `#Temp` (
    `Id` INT NOT NULL COMMENT 'Id Description',
    `Name` VARCHAR(500) NULL DEFAULT('DEFAULT NAME') COMMENT 'Name Description',
    `Unique1` INT NULL COMMENT 'Unique1 Description',
    `Unique2` INT NULL COMMENT 'Unique2 Description',
    `sys_row_id` INT NOT NULL AUTO_INCREMENT,

    CONSTRAINT `PK_Temp_` UNIQUE (`Id`),
    CONSTRAINT `UQ_Temp_` UNIQUE (`Unique1`, `Unique2` DESC),
    CONSTRAINT `CK_Temp_` CHECK (`Name` IS NOT NULL),
    PRIMARY KEY (`sys_row_id` ASC)
);
";
            Assert.AreEqual(expectedSql, sqlBuilder.ToString().RemoveGuids());
        }
    }
}
