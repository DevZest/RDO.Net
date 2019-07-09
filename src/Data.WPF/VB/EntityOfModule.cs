﻿using DevZest.Data;
using DevZest.Data.Presenters;
using Microsoft.VisualBasic.CompilerServices;

/// <summary>
/// Provides ModelOf() operator for VB.Net because _ is not a valid identifier in VB.
/// </summary>
[StandardModule]
public sealed class EntityOfModule
{
    /// <exclude />
    public static T EntityOf<T>(DataPresenter<T> dataPresenter) where T : class, IEntity, new()
    {
        return dataPresenter._;
    }
}