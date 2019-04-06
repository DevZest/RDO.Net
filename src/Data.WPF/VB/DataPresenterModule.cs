using DevZest.Data;
using DevZest.Data.Presenters;
using Microsoft.VisualBasic.CompilerServices;

[StandardModule]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// Provides ModelOf() operator for VB.Net because _ is not a valid identifier in VB.
public sealed class DataPresenterModule
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <exclude />
    public static T ModelOf<T>(DataPresenter<T> dataPresenter) where T : Model, new()
    {
        return dataPresenter._;
    }
}