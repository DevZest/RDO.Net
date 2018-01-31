using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DevZest.Data.Views.Primitives;
using DevZest.Data.Presenters;
using System;

namespace DevZest.Data.Views
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowViewPanel))]
    public class RowView : ContainerView
    {
        public abstract class Commands
        {
            public static readonly RoutedUICommand ToggleEdit = new RoutedUICommand();
            public static readonly RoutedUICommand BeginEdit = new RoutedUICommand();
            public static readonly RoutedUICommand CancelEdit = new RoutedUICommand();
            public static readonly RoutedUICommand EndEdit = new RoutedUICommand();
            public static readonly RoutedUICommand Expand = new RoutedUICommand(UserMessages.RowViewCommands_ExpandCommandText, nameof(Expand), typeof(Commands));
            public static readonly RoutedUICommand Collapse = new RoutedUICommand(UserMessages.RowViewCommands_CollapseCommandText, nameof(Collapse), typeof(Commands));
        }

        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(RowView rowView);
        }

        private sealed class CommandService : ICommandService
        {
            public DataPresenter DataPresenter { get; private set; }

            private Template Template
            {
                get { return DataPresenter.Template; }
            }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public IEnumerable<CommandEntry> GetCommandEntries(RowView rowView)
            {
                yield return Commands.ToggleEdit.Bind(Template.RowViewToggleEditGestures, ToggleEdit, CanToggleEdit);
                yield return Commands.BeginEdit.Bind(Template.RowViewBeginEditGestures, BeginEdit, CanBeginEdit);
                yield return Commands.CancelEdit.Bind(Template.RowViewCancelEditGestures, CancelEdit, CanCancelEdit);
                yield return Commands.EndEdit.Bind(Template.RowViewEndEditGestures, EndEdit, CanCancelEdit);
                if (DataPresenter.IsRecursive)
                {
                    yield return Commands.Expand.Bind(ToggleExpandState, CanExpand, new KeyGesture(Key.OemPlus));
                    yield return Commands.Collapse.Bind(ToggleExpandState, CanCollapse, new KeyGesture(Key.OemMinus));
                }
            }

            private bool IsCurrent(RowView rowView)
            {
                return rowView.RowPresenter != null && rowView.RowPresenter == DataPresenter.CurrentRow;
            }

            private void CanToggleEdit(object sender, CanExecuteRoutedEventArgs e)
            {
                var rowView = (RowView)sender;
                e.CanExecute = IsCurrent(rowView);
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void ToggleEdit(object sender, ExecutedRoutedEventArgs e)
            {
                var rowPresenter = ((RowView)sender).RowPresenter;
                if (rowPresenter.IsEditing)
                    rowPresenter.EndEdit();
                else
                    rowPresenter.BeginEdit();
            }

            private void CanBeginEdit(object sender, CanExecuteRoutedEventArgs e)
            {
                var rowView = (RowView)sender;
                e.CanExecute = IsCurrent(rowView) && !rowView.RowPresenter.IsEditing;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void BeginEdit(object sender, ExecutedRoutedEventArgs e)
            {
                var rowPresenter = ((RowView)sender).RowPresenter;
                rowPresenter.BeginEdit();
            }

            private void CanCancelEdit(object sender, CanExecuteRoutedEventArgs e)
            {
                var rowView = (RowView)sender;
                e.CanExecute = IsCurrent(rowView) && rowView.RowPresenter.IsEditing;
                if (!e.CanExecute)
                    e.ContinueRouting = true;
            }

            private void CancelEdit(object sender, ExecutedRoutedEventArgs e)
            {
                var rowPresenter = ((RowView)sender).RowPresenter;
                rowPresenter.CancelEdit();
            }

            private void EndEdit(object sender, ExecutedRoutedEventArgs e)
            {
                var rowPresenter = ((RowView)sender).RowPresenter;
                rowPresenter.EndEdit();
            }

            private void ToggleExpandState(object sender, ExecutedRoutedEventArgs e)
            {
                var rowView = (RowView)sender;
                rowView.RowPresenter.ToggleExpandState();
            }

            private void CanExpand(object sender, CanExecuteRoutedEventArgs e)
            {
                var rowView = (RowView)sender;
                var rowPresenter = rowView.RowPresenter;
                e.CanExecute = rowPresenter.HasChildren && !rowPresenter.IsExpanded;
            }

            private void CanCollapse(object sender, CanExecuteRoutedEventArgs e)
            {
                var rowView = (RowView)sender;
                var rowPresenter = rowView.RowPresenter;
                e.CanExecute = rowPresenter.HasChildren && rowPresenter.IsExpanded;
            }
        }

        public abstract class Styles
        {
            public static readonly StyleId Selectable = new StyleId(typeof(RowView));
        }

        private static readonly DependencyPropertyKey CurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Current", typeof(RowView),
            typeof(RowView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;

        public static RowView GetCurrent(DependencyObject target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return (RowView)target.GetValue(CurrentProperty);
        }

        public event EventHandler<EventArgs> SettingUp = delegate { };
        public event EventHandler<EventArgs> Refreshing = delegate { };
        public event EventHandler<EventArgs> CleaningUp = delegate { };

        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
            FocusableProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(BooleanBoxes.False));
            ServiceManager.Register<ICommandService, CommandService>();
        }

        public RowView()
        {
            SetValue(CurrentPropertyKey, this);
        }

        private RowPresenter _rowPresenter;
        public RowPresenter RowPresenter
        {
            get { return _rowPresenter; }
            private set
            {
                _rowPresenter = value;
                _elementManager = _rowPresenter == null ? null : _rowPresenter.ElementManager;
            }
        }

        public sealed override int ContainerOrdinal
        {
            get { return RowPresenter == null ? -1 : RowPresenter.Index / RowPresenter.ElementManager.FlowRepeatCount; }
        }

        private ElementManager _elementManager;
        internal sealed override ElementManager ElementManager
        {
            get { return _elementManager; }
        }

        private IReadOnlyList<RowViewBehavior> Behaviors
        {
            get { return _elementManager.Template.RowViewBehaviors; }
        }

        internal RowBindingCollection RowBindings
        {
            get { return _elementManager == null ? null : _elementManager.Template.InternalRowBindings; }
        }

        private IElementCollection ElementCollection { get; set; }
        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal sealed override bool AffectedOnRowsChanged
        {
            get { return false; }
        }

        internal sealed override void ReloadCurrentRow(RowPresenter oldValue)
        {
            Debug.Assert(oldValue != null && RowPresenter == oldValue);
            var newValue = ElementManager.CurrentRow;
            Debug.Assert(newValue != null);

            if (oldValue == newValue)
                return;

            RowPresenter.View = null;
            Reload(newValue);
        }

        internal void Reload(RowPresenter rowPresenter)
        {
            CleanupElements(false);

            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            if (Elements != null)
            {
                foreach (var element in Elements)
                    element.SetRowPresenter(rowPresenter);
            }

            SetupElements(false);
        }

        internal sealed override void Setup(ElementManager elementManager, int containerOrdinal)
        {
            Setup(elementManager.Rows[containerOrdinal]);
        }

        public bool HasSetup { get; private set; }

        internal void Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            if (ElementCollection == null)
                ElementCollection = ElementCollectionFactory.Create(null);
            SetupElements(true);
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
            {
                behaviors[i].Setup(this);
                behaviors[i].Refresh(this);
            }
            this.SetupCommandEntries(rowPresenter?.DataPresenter?.GetService<ICommandService>().GetCommandEntries(this));
            SettingUp(this, EventArgs.Empty);
            HasSetup = true;
        }

        internal sealed override void Cleanup()
        {
            Debug.Assert(RowPresenter != null);
            Debug.Assert(ElementCollection != null);

            CleaningUp(this, EventArgs.Empty);
            this.CleanupCommandEntries();
            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Cleanup(this);
            CleanupElements(true);
            RowPresenter.View = null;
            RowPresenter = null;
            HasSetup = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template == null)
                return;

            var panel = Template.FindName("PART_Panel", this) as RowViewPanel;
            if (panel != null)
                Setup(panel);
        }

        private void Setup(FrameworkElement elementsPanel)
        {
            if (ElementCollection != null)
            {
                if (ElementCollection.Parent == elementsPanel)
                    return;

                if (ElementCollection.Parent == null)
                {
                    var newElementCollection = ElementCollectionFactory.Create(elementsPanel);
                    for (int i = 0; i < Elements.Count; i++)
                        newElementCollection.Add(Elements[i]);
                    ElementCollection = newElementCollection;
                    return;
                }

                CleanupElements(true);
            }

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);
            SetupElements(true);
        }

        private void SetupElements(bool addToCollection)
        {
            if (RowPresenter == null || ElementCollection == null)
                return;

            var rowBindings = RowBindings;
            BeginSetup(rowBindings, addToCollection ? null : Elements);

            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = rowBinding.Setup(RowPresenter);
                if (addToCollection)
                    ElementCollection.Add(element);
            }
            rowBindings.EndSetup();
        }

        private static void BeginSetup(BindingCollection<RowBinding> bindings, IReadOnlyList<UIElement> elements)
        {
            for (int i = 0; i < bindings.Count; i++)
                bindings[i].BeginSetup(elements == null ? null : elements[i]);
        }

        private void CleanupElements(bool removeFromCollection)
        {
            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.Cleanup(element);
            }
            if (removeFromCollection)
                ElementCollection.RemoveRange(0, Elements.Count);
        }

        internal sealed override void Refresh()
        {
            Debug.Assert(RowPresenter != null);

            if (Elements == null)
                return;

            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.Refresh(element);
            }

            var behaviors = Behaviors;
            for (int i = 0; i < behaviors.Count; i++)
                behaviors[i].Refresh(this);
            this.RefreshValidation(ValidationInfo);
            Refreshing(this, EventArgs.Empty);
        }

        internal void Flush()
        {
            Debug.Assert(RowPresenter != null && RowPresenter == ElementManager.CurrentRow);

            if (Elements == null)
                return;

            var rowBindings = RowBindings;
            Debug.Assert(Elements.Count == rowBindings.Count);
            for (int i = 0; i < rowBindings.Count; i++)
            {
                var rowBinding = rowBindings[i];
                var element = Elements[i];
                rowBinding.FlushInput(element);
            }
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            if (ElementManager == null || RowPresenter == null)
                return;

            if ((bool)e.NewValue)
                ElementManager.OnFocused(this);
        }

        internal int FlowIndex
        {
            get { return RowPresenter.Index % RowPresenter.ElementManager.FlowRepeatCount; }
        }

        public DataPresenter DataPresenter
        {
            get { return RowPresenter == null ? null : RowPresenter.DataPresenter; }
        }

        public ValidationInfo ValidationInfo
        {
            get
            {
                var inputManager = _elementManager as Presenters.Primitives.InputManager;
                return inputManager == null ? ValidationInfo.Empty : inputManager.GetValidationInfo(this);
            }
        }
    }
}
