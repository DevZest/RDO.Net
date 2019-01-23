namespace DevZest.Data.MySql
{
    internal interface IModelAliasManager
    {
        string this[Model model] { get; }
    }
}
