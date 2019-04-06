using DevZest.Data;
using DevZest.Data.Presenters;
using Microsoft.VisualBasic.CompilerServices;

/// <summary>
/// Provides ModelOf() operator for VB.Net because _ is not a valid identifier in VB.
/// </summary>
[StandardModule]
public sealed class DataPresenterModule
{
    /// <exclude />
    public static T ModelOf<T>(DataPresenter<T> dataPresenter) where T : Model, new()
    {
        return dataPresenter._;
    }
}