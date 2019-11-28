using DevZest.Data.Annotations;
using DevZest.Data.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Tools
{
    public delegate void AddForeignKeyDelegate(INamedTypeSymbol fkType, string name, DataSet<ModelMapper.ForeignKeyEntry> entries);

    public delegate void AddIndexDelegate(string name, string description, string dbName, bool isUnique, bool isValidOnTable, bool isValidOnTempTable, DataSet<ModelMapper.IndexEntry> entries);

    public delegate void AddKeyOrRefDelegate(string name, DataSet<ModelMapper.PrimaryKeyEntry> entries);

    public delegate void AddPrimaryKeyDelegate(string pkTypeName, DataSet<ModelMapper.PrimaryKeyEntry> entries, string keyTypeName, string refTypeName);

    public delegate void AddProjectionDelegate(string typeName, DataSet<ModelMapper.ProjectionEntry> entries);

    public delegate void AddUniqueConstraintDelegate(string name, string description, string dbName,
        INamedTypeSymbol messageResourceType, IPropertySymbol messageResourceProperty, string message, DataSet<ModelMapper.IndexEntry> entries);

    public delegate void AddCheckConstraintDelegate(string name, string description, INamedTypeSymbol messageResourceType, IPropertySymbol messageResourceProperty, string message);

    public delegate void AddCustomValidatorDelegate(string name, string description);

    public delegate void AddComputationDelegate(string name, string description, ComputationMode? mode);

    public delegate void AddDbTableDelegate(INamedTypeSymbol model, string name, string dbName, string description);

    public delegate void AddRelationshipDelegate(string name, IPropertySymbol foreignKey, IPropertySymbol refTable, string description, ForeignKeyRule deleteRule, ForeignKeyRule updateRule);

    public delegate Task<int> ConsoleWindowOperationDelegate(IProgress<string> progress, CancellationToken ct);
}
