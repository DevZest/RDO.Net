﻿using System;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    public abstract class BlockViewBehavior
    {
        protected internal abstract void Setup(BlockView blockView);

        protected internal abstract void Refresh(BlockView blockView);

        protected internal abstract void Cleanup(BlockView blockView);
    }
}
