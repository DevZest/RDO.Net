using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    [TestClass]
    public class ServiceManagerTests
    {
        private static RoutedCommand Command = new RoutedCommand();

        private interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(UIElement element);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(UIElement element)
            {
                yield return Command.Bind(ExecuteRoutedEvent, CanExecuteRoutedEvent, new KeyGesture(Key.F1));
            }

            private void CanExecuteRoutedEvent(object sender, CanExecuteRoutedEventArgs e)
            {
            }

            private void ExecuteRoutedEvent(object sender, ExecutedRoutedEventArgs e)
            {
            }
        }

        private static IEnumerable<CommandEntry> GetCommandEntries(ICommandService commandService, UIElement element)
        {
            return commandService.GetCommandEntries(element);
        }

        [TestMethod]
        public void ServiceManager()
        {
            var commandService = new CommandService();
            var element = new UIElement();
            element.CommandBindings.Add(new CommandBinding());
            element.InputBindings.Add(new KeyBinding());
            Assert.AreEqual(1, element.CommandBindings.Count);
            Assert.AreEqual(1, element.InputBindings.Count);
            element.SetupCommandEntries(commandService, GetCommandEntries);
            Assert.AreEqual(2, element.CommandBindings.Count);
            Assert.AreEqual(2, element.InputBindings.Count);
            element.SetupCommandEntries(commandService, GetCommandEntries);
            Assert.AreEqual(2, element.CommandBindings.Count);
            Assert.AreEqual(2, element.InputBindings.Count);
            element.CleanupCommandEntries();
            Assert.AreEqual(1, element.CommandBindings.Count);
            Assert.AreEqual(1, element.InputBindings.Count);
        }
    }
}
