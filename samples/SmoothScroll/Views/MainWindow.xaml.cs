using DevZest.Data;
using SmoothScroll.Models;
using SmoothScroll.Presenters;
using System;
using System.Windows;

namespace SmoothScroll.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var fooList = new FooList();
            fooList.Show(dataView, Data.GetTestData(10000));
        }
    }
}
