﻿using DevZest.Data.Windows.Controls;
using System.Windows;

namespace DevZest.Data.Windows.Helpers
{
    internal class AutoInitBlockView : BlockView
    {
        public AutoInitBlockView()
        {
            Setup(null);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return LayoutManager.MeasureBlock(this, constraint);
        }
    }
}
