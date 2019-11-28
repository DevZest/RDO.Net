using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using System.IO;
using DefGuidList = Microsoft.VisualStudio.Editor.DefGuidList;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace DevZest.Data.Tools
{
    internal static class Extensions
    {
        internal static IWpfTextView ToWpfTextView(this IVsWindowFrame vsWindowFrame)
        {
            IWpfTextView result = null;
            IVsTextView textView = VsShellUtilities.GetTextView(vsWindowFrame);
            if (textView != null)
            {
                Guid guidIWpfTextViewHost = DefGuidList.guidIWpfTextViewHost;
                if (((IVsUserData)textView).GetData(ref guidIWpfTextViewHost, out var obj) == 0 && obj != null)
                    result = ((IWpfTextViewHost)obj).TextView;
            }
            return result;
        }

        internal static SnapshotSpan ToSnapshotSpan(this TextSpan textSpan, ITextSnapshot snapshot)
        {
            return new SnapshotSpan(snapshot, new Span(textSpan.Start, textSpan.Length));
        }

        internal static TextSpan ToTextSpan(this SnapshotSpan snapshotSpan)
        {
            return new TextSpan(snapshotSpan.Start, snapshotSpan.Length);
        }

        public static string ValidateRequired(this string s)
        {
            s = s?.Trim();
            return string.IsNullOrEmpty(s) ? UserMessages.Validation_Required : null;
        }

        public static string ValidateRequired<T>(this T symbol)
            where T : class
        {
            return symbol == null ? UserMessages.Validation_Required : null;
        }

        public static void ReportLine(this IProgress<string> progress)
        {
            progress.Report(Environment.NewLine);
        }

        public static void ReportLine(this IProgress<string> progress, string value)
        {
            progress.Report(value);
            progress.Report(Environment.NewLine);
        }

        private static string GetFullPath(this EnvDTE.Project vsProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return vsProject.Properties.Item("FullPath").Value.ToString();
        }

        private static string GetOutputPath(this EnvDTE.Project vsProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
        }

        private static string GetOutputDir(this EnvDTE.Project vsProject)
        {
            return Path.Combine(vsProject.GetFullPath(), vsProject.GetOutputPath());
        }

        private static string GetOutputFileName(this EnvDTE.Project vsProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return vsProject.Properties.Item("OutputFileName").Value.ToString();
        }

        public static string GetAssemblyPath(this EnvDTE.Project vsProject)
        {
            return Path.Combine(vsProject.GetOutputDir(), vsProject.GetOutputFileName());
        }

        public static string GetTargetFrameworkMoniker(this EnvDTE.Project vsProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return (string)vsProject.Properties.Item("TargetFrameworkMoniker").Value;
        }

        public static int GetOutputType(this EnvDTE.Project vsProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return (int)vsProject.Properties.Item("OutputType").Value;
        }

        public static string AddQuotesIfRequired(this string argument)
        {
            Debug.Assert(!string.IsNullOrEmpty(argument));
            return argument[0] != '"' && argument.Contains(" ") ? "\"" + argument + "\"" : argument;
        }

        public static string[] AddQuotesIfRequired(this string[] arguments)
        {
            for (int i = 0; i < arguments.Length; i++)
                arguments[i] = arguments[i].AddQuotesIfRequired();
            return arguments;
        }

        private static TServiceInterface GetService<TServiceInterface, TService>(this IServiceProvider serviceProvider)
            where TServiceInterface : class
            where TService : class
        {
            return (TServiceInterface)(serviceProvider.GetService(typeof(TService)));
        }

        private static IVsTextView GetActiveVsTextView(this IServiceProvider serviceProvider)
        {
            IVsTextManager textManager = serviceProvider.GetService<IVsTextManager, SVsTextManager>();
            if (textManager == null)
                return null;
            textManager.GetActiveView(0, null, out var result);
            return result;
        }

        public static IWpfTextView GetCurrentWpfTextView(this IServiceProvider serviceProvider)
        {
            var vsTextView = serviceProvider.GetActiveVsTextView();
            return vsTextView == null ? null : serviceProvider.GetEditorAdaptersFactoryService().GetWpfTextView(vsTextView);
        }

        private static IVsEditorAdaptersFactoryService GetEditorAdaptersFactoryService(this IServiceProvider serviceProvider)
        {
            IComponentModel componentModel = serviceProvider.GetService<IComponentModel, SComponentModel>();
            return componentModel.GetService<IVsEditorAdaptersFactoryService>();
        }

        public static Document GetCurrentDocument(this IServiceProvider serviceProvider)
        {
            var wpfTextView = serviceProvider.GetCurrentWpfTextView();
            return wpfTextView.GetDocument();
        }

        public static Document GetDocument(this IWpfTextView wpfTextView)
        {
            if (wpfTextView == null)
                return null;
            ITextSnapshot currentSnapshot = wpfTextView.TextBuffer.CurrentSnapshot;
            return currentSnapshot.GetOpenDocumentInCurrentContextWithChanges();
        }

        public static TextSpan GetCurrentSelectionSpan(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetCurrentWpfTextView().Selection.GetSelectionSpan();
        }

        public static TextSpan GetSelectionSpan(this ITextSelection textSelection)
        {
            return textSelection.StreamSelectionSpan.SnapshotSpan.ToTextSpan();
        }

        public static EnvDTE.DTE GetDTE(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<EnvDTE.DTE, EnvDTE.DTE>();
        }
    }
}
