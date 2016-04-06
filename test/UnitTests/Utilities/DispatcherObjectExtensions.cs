using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevZest
{
    internal static class DispatcherObjectExtensions
    {
        private static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static T FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
        {
            return depObj.FindVisualChildren<T>().FirstOrDefault();
        }

        public static void RunAfterLoaded<T>(this T depObj, Action<T> testAction)
            where T : DependencyObject
        {
            var window = new Window()
            {
                Content = depObj,
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            window.Show();
            var operation = depObj.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                testAction(depObj);
                window.Close();
            }));
            operation.Wait();
        }
    }
}
