using Microsoft.CodeAnalysis;
using System;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Tools
{
    public abstract class DbInitWindowBase : CommonDialogWindow
    {
        protected DbInitWindowBase()
            : base(false)
        {
        }

        protected sealed override void ExecApply()
        {
            var staysOpen = Run();
            if (!staysOpen)
                Close();
        }

        protected abstract bool Run();
    }
}
