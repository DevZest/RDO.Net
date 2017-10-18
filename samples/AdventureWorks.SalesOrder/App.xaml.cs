using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;

namespace AdventureWorks.SalesOrders
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly string ConnectionString = GetConnectionString();

        private static string GetConnectionString()
        {
            string mdfFilename = "AdventureWorksLT.mdf";
            string outputFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string attachDbFilename = Path.Combine(outputFolder, mdfFilename);
            return string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
        }

        public static bool Execute(Func<CancellationToken, Task> func, Window ownerWindow, string windowTitle = null, string label = null)
        {
            var dialogResult = ProgressDialog.Execute(func, ownerWindow, windowTitle, label);
            var exception = dialogResult.Exception;
            if (exception != null)
            {
                MessageBox.Show(exception.Message, windowTitle);
                return false;
            }
            return true;
        }

        public static T Execute<T>(Func<CancellationToken, Task<T>> func, Window ownerWindow, string windowTitle = null, string label = null)
        {
            var dialogResult = ProgressDialog.Execute(func, ownerWindow, windowTitle, label);
            var exception = dialogResult.Exception;
            if (exception != null)
            {
                MessageBox.Show(exception.Message, windowTitle);
                return default(T);
            }
            return dialogResult.Value;
        }

        //private static void SaveDefaultTemplate(Type type, string fileName)
        //{
        //    var control = Application.Current.FindResource(type);
        //    using (var writer = new XmlTextWriter(fileName, System.Text.Encoding.UTF8))
        //    {
        //        writer.Formatting = Formatting.Indented;
        //        XamlWriter.Save(control, writer);
        //    }
        //}

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    SaveDefaultTemplate(typeof(ComboBox), "ComboBox.xaml");
        //}
    }
}
