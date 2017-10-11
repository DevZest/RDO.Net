using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DevZest.Data.Views.Primitives;
using DevZest.Data.Presenters;
using System;
using DevZest.Data.Presenters.Services;
using DevZest.Data.Presenters.Plugins;

namespace DevZest.Data.Views
{
    [TemplatePart(Name = "PART_Panel", Type = typeof(RowViewPanel))]
    public class RowView : ContainerView
    {
        public static readonly StyleKey SelectableStyleKey = new StyleKey(typeof(RowView));

        private static readonly DependencyPropertyKey CurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Current", typeof(RowView),
            typeof(RowView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;

        public static RowView GetCurrent(DependencyObject target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return (RowView)target.GetValue(CurrentProperty);
        }

        public event EventHandler<EventArgs> Refreshing = delegate { };

        static RowView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(typeof(RowView)));
            FocusableProperty.OverrideMetadata(typeof(RowView), new FrameworkPropertyMetadata(BooleanBoxes.False));
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

        private IReadOnlyList<RowViewPlugin> Plugins
        {
            get { return _elementManager.Template.RowViewPlugins; }
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

        internal void Setup(RowPresenter rowPresenter)
        {
            Debug.Assert(RowPresenter == null && rowPresenter != null);
            RowPresenter = rowPresenter;
            rowPresenter.View = this;
            if (ElementCollection == null)
                ElementCollection = ElementCollectionFactory.Create(null);
            SetupElements(true);
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
            {
                plugins[i].Setup(this);
                plugins[i].Refresh(this);
            }
        }

        internal sealed override void Cleanup()
        {
            Debug.Assert(RowPresenter != null);
            Debug.Assert(ElementCollection != null);

            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Cleanup(this);
            CleanupElements(true);
            RowPresenter.View = null;
            RowPresenter = null;
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

            EnsureCommandEntriesSetup();
            var plugins = Plugins;
            for (int i = 0; i < plugins.Count; i++)
                plugins[i].Refresh(this);
            Refreshing(this, EventArgs.Empty);
        }

        private bool _commandEntriesSetup;
        private void EnsureCommandEntriesSetup()
        {
            if (_commandEntriesSetup)
                return;

            SetupCommandEntries();
            _commandEntriesSetup = true;
        }

        private void SetupCommandEntries()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            this.SetupCommandEntries(dataPresenter.GetService<RowViewCommands>(() => new RowViewCommands()).GetCommandEntries(this));
        }

        private bool _rowSelectorCommandEntriesSetup;
        /// <remarks>
        /// Adding CommandBinding into RowSelector's CommandBindings does not work. There is no way to figure out why.
        /// I suspect it's because RowSelector is in ControlTemplate of RowView. Workaround this by adding into RowView (the root element)'s CommandBindings.
        /// </remarks>
        internal void EnsureRowSelectorCommandEntriesSetup()
        {
            if (_rowSelectorCommandEntriesSetup)
                return;

            SetupRowSelectorCommandEntries();
            _rowSelectorCommandEntriesSetup = true;
        }

        private void SetupRowSelectorCommandEntries()
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            this.SetupCommandEntries(dataPresenter.GetService<RowSelectorCommands>(() => new RowSelectorCommands()).GetCommandEntries(this));
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
    }
}
