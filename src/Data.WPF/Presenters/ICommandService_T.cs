using System.Collections.Generic;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public interface ICommandService<T> : IService
        where T : UIElement
    {
        IEnumerable<CommandEntry> GetCommandEntries(T element);
    }
}
