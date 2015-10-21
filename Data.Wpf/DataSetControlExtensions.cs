using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Wpf
{
    public static class DataSetControlExtensions
    {
        public static T Scrollable<T>(this T dataSetControl, bool value)
            where T : DataSetControl
        {
            dataSetControl.Scrollable = value;
            return dataSetControl;
        }
    }
}
