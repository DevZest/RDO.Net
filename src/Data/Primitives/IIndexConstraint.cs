using System;

namespace DevZest.Data.Primitives
{
    internal interface IIndexConstraint
    {
        bool IsClustered { get; }

        string SystemName { get; }

        void AsNonClustered();
    }
}
