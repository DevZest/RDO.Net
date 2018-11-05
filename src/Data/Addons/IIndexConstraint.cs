namespace DevZest.Data.Addons
{
    internal interface IIndexConstraint
    {
        bool IsClustered { get; }

        string SystemName { get; }

        void AsNonClustered();
    }
}
