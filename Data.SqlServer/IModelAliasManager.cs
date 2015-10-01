using System;

namespace DevZest.Data.SqlServer
{
    internal interface IModelAliasManager
    {
        string this[Model model] { get; }
    }
}
