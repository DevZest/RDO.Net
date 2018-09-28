using System.Windows;
using System;
using System.Windows.Media;
using System.Collections;
using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls;

namespace DevZest.Data.Views
{
    public class InPlaceEditor : FrameworkElement, IScalarElement, IRowElement, IContainerElement
    {
        public interface ICommandService : IService
        {
            IEnumerable<CommandEntry> GetCommandEntries(InPlaceEditor inPlaceEditor);
        }

        protected virtual ICommandService GetCommandService(DataPresenter dataPresenter)
        {
            return dataPresenter.GetService<ICommandService>();
        }

        private void SetupCommands(DataPresenter dataPresenter)
        {
            var commandService = GetCommandService(dataPresenter);
            if (commandService != null)
                this.SetupCommandEntries(commandService, GetCommandEnties);
        }

        private static IEnumerable<CommandEntry> GetCommandEnties(ICommandService commandService, InPlaceEditor inPlaceEditor)
        {
            return commandService.GetCommandEntries(inPlaceEditor);
        }

        private void CleanupCommands()
        {
            this.CleanupCommandEntries();
        }

        internal static RowBinding<InPlaceEditor> AddToInPlaceEditor<TEditing, TInert>(RowInput<TEditing> editingInput, RowBinding<TInert> inertBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            var result = new RowBinding<InPlaceEditor>(null);
            result.Input = new ProxyRowInput<TEditing, TInert>(result, editingInput, inertBinding);
            return result;
        }

        private interface IProxyRowInput
        {
            RowBinding EditorBinding { get; }
            RowBinding InertBinding { get; }
        }

        private sealed class ProxyRowInput<TEditor, TInert> : RowInput<InPlaceEditor>, IRowValidation, IProxyRowInput
            where TEditor : UIElement, new()
            where TInert : UIElement, new()
        {
            private readonly RowInput<TEditor> _editorInput;
            private readonly RowBinding<TInert> _inertBinding;

            public ProxyRowInput(RowBinding<InPlaceEditor> binding, RowInput<TEditor> editorInput, RowBinding<TInert> inertBinding)
                : base(binding, new ExplicitTrigger<InPlaceEditor>(), null)
            {
                Debug.Assert(editorInput != null);
                Debug.Assert(inertBinding != null);
                _editorInput = editorInput;
                _inertBinding = inertBinding;
                _editorInput.InjectRowValidation(this);
                InertBinding.Seal(binding, 0);
                EditorBinding.Seal(binding, 1);
            }

            public RowBinding EditorBinding
            {
                get { return _editorInput.Binding; }
            }

            public RowBinding InertBinding
            {
                get { return _inertBinding; }
            }

            private IRowValidation BaseRowValidation
            {
                get { return InputManager.RowValidation; }
            }

            public ValidationInfo GetInfo(RowPresenter rowPresenter, Input<RowBinding, IColumns> input)
            {
                return input == this ? BaseRowValidation.GetInfo(rowPresenter, input) : ValidationInfo.Empty;
            }

            public bool HasError(RowPresenter rowPresenter, Input<RowBinding, IColumns> input, bool? blockingPrecedence)
            {
                return input == this ? BaseRowValidation.HasError(rowPresenter, input, blockingPrecedence) : false;
            }

            public void OnFlushed<T>(RowInput<T> rowInput, bool makeProgress, bool valueChanged) where T : UIElement, new()
            {
                BaseRowValidation.OnFlushed(this, makeProgress, valueChanged);
            }

            public override IColumns Target
            {
                get { return _editorInput.Target; }
            }

            internal override void Flush(InPlaceEditor element)
            {
                var editorElement = element.EditorElement;
                if (editorElement != null)
                    _editorInput.Flush((TEditor)editorElement);
            }

            bool IRowValidation.IsLockedByFlushingError(UIElement element)
            {
                var currentInPlaceEditor = GetCurrentInPlaceEditor(element);
                return currentInPlaceEditor != null ? BaseRowValidation.IsLockedByFlushingError(currentInPlaceEditor) : false;
            }

            public override FlushingError GetFlushingError(UIElement element)
            {
                var currentInPlaceEditor = GetCurrentInPlaceEditor(element);
                Debug.Assert(currentInPlaceEditor != null);
                return BaseRowValidation.GetFlushingError(currentInPlaceEditor);
            }

            void IRowValidation.SetFlushingError(UIElement element, string flushingErrorMessage)
            {
                var currentInPlaceEditor = GetCurrentInPlaceEditor(element);
                Debug.Assert(currentInPlaceEditor != null);
                BaseRowValidation.SetFlushingError(currentInPlaceEditor, flushingErrorMessage);
            }
        }

        internal static ScalarBinding<InPlaceEditor> AddToInPlaceEditor<TEditing, TInert>(ScalarInput<TEditing> scalarInput, ScalarBinding<TInert> inertScalarBinding)
            where TEditing : UIElement, new()
            where TInert : UIElement, new()
        {
            var result = new ScalarBinding<InPlaceEditor>((Action<InPlaceEditor>)null);
            result.Input = new ProxyScalarInput<TEditing, TInert>(result, scalarInput, inertScalarBinding);
            return result;
        }

        private interface IProxyScalarInput
        {
            ScalarBinding EditorBinding { get; }
            ScalarBinding InertBinding { get; }
        }

        private sealed class ProxyScalarInput<TEditor, TInert> : ScalarInput<InPlaceEditor>, IScalarValidation, IProxyScalarInput
            where TEditor : UIElement, new()
            where TInert : UIElement, new()
        {
            private readonly ScalarInput<TEditor> _editorInput;
            private readonly ScalarBinding<TInert> _inertBinding;

            public ProxyScalarInput(ScalarBinding<InPlaceEditor> binding, ScalarInput<TEditor> editorInput, ScalarBinding<TInert> inertBinding)
                : base(binding, new ExplicitTrigger<InPlaceEditor>(), null)
            {
                Debug.Assert(editorInput != null);
                Debug.Assert(inertBinding != null);
                _editorInput = editorInput;
                _inertBinding = inertBinding;
                _editorInput.InjectScalarValidation(this);
                InertBinding.Seal(binding, 0);
                EditorBinding.Seal(binding, 1);
            }

            public ScalarBinding EditorBinding
            {
                get { return _editorInput.Binding; }
            }

            public ScalarBinding InertBinding
            {
                get { return _inertBinding; }
            }

            private IScalarValidation BaseScalarValidation
            {
                get { return InputManager.ScalarValidation; }
            }

            public ValidationInfo GetInfo(Input<ScalarBinding, IScalars> input, int flowIndex)
            {
                return input == this ? BaseScalarValidation.GetInfo(input, flowIndex) : ValidationInfo.Empty;
            }

            public bool HasError(Input<ScalarBinding, IScalars> input, int flowIndex, bool? blockingPrecedence)
            {
                return input == this ? BaseScalarValidation.HasError(input, flowIndex, blockingPrecedence) : false;
            }

            public void OnFlushed<T>(ScalarInput<T> scalarInput, bool makeProgress, bool valueChanged) where T : UIElement, new()
            {
                BaseScalarValidation.OnFlushed(this, makeProgress, valueChanged);
            }

            public override IScalars Target
            {
                get { return _editorInput.Target; }
            }

            internal override void Flush(InPlaceEditor element)
            {
                var editorElement = element.EditorElement;
                if (editorElement != null)
                    _editorInput.Flush((TEditor)editorElement);
            }

            bool IScalarValidation.IsLockedByFlushingError(UIElement element)
            {
                var currentInPlaceEditor = GetCurrentInPlaceEditor(element);
                return currentInPlaceEditor != null ? BaseScalarValidation.IsLockedByFlushingError(currentInPlaceEditor) : false;
            }

            public override FlushingError GetFlushingError(UIElement element)
            {
                var currentInPlaceEditor = GetCurrentInPlaceEditor(element);
                Debug.Assert(currentInPlaceEditor != null);
                return BaseScalarValidation.GetFlushingError(currentInPlaceEditor);
            }

            void IScalarValidation.SetFlushingError(UIElement element, string flushingErrorMessage)
            {
                var currentInPlaceEditor = GetCurrentInPlaceEditor(element);
                Debug.Assert(currentInPlaceEditor != null);
                BaseScalarValidation.SetFlushingError(currentInPlaceEditor, flushingErrorMessage);
            }
        }

        public interface IConfiguration : IService
        {
            bool QueryEditingMode(InPlaceEditor inPlaceEditor);
            bool QueryEditorElementFocus(InPlaceEditor inPlaceEditor);
        }

        private sealed class DefaultConfiguration : IConfiguration
        {
            public static DefaultConfiguration Singleton = new DefaultConfiguration();

            private DefaultConfiguration()
            {
            }

            public DataPresenter DataPresenter
            {
                get { return null; }
            }

            public void Initialize(DataPresenter dataPresenter)
            {
            }

            public bool QueryEditingMode(InPlaceEditor inPlaceEditor)
            {
                return inPlaceEditor.IsMouseOver || inPlaceEditor.IsKeyboardFocusWithin;
            }

            public bool QueryEditorElementFocus(InPlaceEditor inPlaceEditor)
            {
                return inPlaceEditor.IsKeyboardFocusWithin;
            }
        }

        public interface IChildInitializer : IService
        {
            void InitializeEditorElement(InPlaceEditor inPlaceEditor);
            void InitializeInertElement(InPlaceEditor inPlaceEditor);
        }

        private sealed class ChildInitializer : IChildInitializer
        {
            public DataPresenter DataPresenter { get; private set; }

            public void Initialize(DataPresenter dataPresenter)
            {
                DataPresenter = dataPresenter;
            }

            public void InitializeEditorElement(InPlaceEditor inPlaceEditor)
            {
                if (inPlaceEditor.EditorElement is TextBox textBox)
                    textBox.SelectAll();
            }

            public void InitializeInertElement(InPlaceEditor inPlaceEditor)
            {
            }
        }

        private static readonly DependencyPropertyKey IsRowEditingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsRowEditing), typeof(bool), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsRowEditingProperty = IsRowEditingPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsScalarEditingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsScalarEditing), typeof(bool), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsScalarEditingProperty = IsScalarEditingPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ConfigurationProperty = DependencyProperty.RegisterAttached("Configuration", typeof(IConfiguration), typeof(InPlaceEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static IConfiguration GetConfiguration(DependencyObject obj)
        {
            return (IConfiguration)obj.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(DependencyObject obj, IConfiguration value)
        {
            obj.SetValue(ConfigurationProperty, value);
        }

        static InPlaceEditor()
        {
            FocusableProperty.OverrideMetadata(typeof(InPlaceEditor), new FrameworkPropertyMetadata(BooleanBoxes.True));
            ServiceManager.Register<IConfiguration, DefaultConfiguration>(() => DefaultConfiguration.Singleton);
            ServiceManager.Register<IChildInitializer, ChildInitializer>();
        }

        public bool IsRowEditing
        {
            get { return (bool)GetValue(IsRowEditingProperty); }
            private set { SetValue(IsRowEditingPropertyKey, BooleanBoxes.Box(value)); }
        }

        public bool IsScalarEditing
        {
            get { return (bool)GetValue(IsScalarEditingProperty); }
            private set { SetValue(IsScalarEditingPropertyKey, BooleanBoxes.Box(value)); }
        }

        private IConfiguration GetConfiguration()
        {
            return GetConfiguration(this) ?? DataPresenter.GetService<IConfiguration>();
        }

        private bool _isEditing;
        private IInputElement _elementToRestoreFocus;
        public bool IsEditing
        {
            get { return _isEditing; }
            private set
            {
                if (_isEditing == value)
                    return;

                var oldFocusedElement = Keyboard.FocusedElement;
                var oldEditorElement = EditorElement;
                bool oldIsKeyboardFocusWithin = IsKeyboardFocusWithin;

                _isEditing = value;

                var setup = SetupProxyRowInput();
                if (!setup)
                    setup = SetupProxyScalarInput();

                if (_isEditing)
                {
                    if (setup)
                    {
                        if (GetConfiguration().QueryEditorElementFocus(this))
                            EditorElement.Focus();
                    }

                    if (Keyboard.FocusedElement != oldFocusedElement)
                        _elementToRestoreFocus = oldFocusedElement;
                }
                else
                {
                    if (_elementToRestoreFocus != null)
                    {
                        if (oldIsKeyboardFocusWithin && !HasFocusAfterEditorElementUnloaded(oldEditorElement))
                            _elementToRestoreFocus.Focus();
                        _elementToRestoreFocus = null;
                    }
                }
            }
        }

        private bool HasFocusAfterEditorElementUnloaded(UIElement oldEditorElement)
        {
            if (oldEditorElement != null && oldEditorElement.IsKeyboardFocusWithin)
                return false;
            return IsKeyboardFocusWithin;
        }

        private bool SetupProxyRowInput()
        {
            var proxyRowInput = GetProxyRowInput();
            if (proxyRowInput != null)
            {
                Cleanup(proxyRowInput);
                Setup(proxyRowInput, RowPresenter);
                return true;
            }
            return false;
        }

        private bool SetupProxyScalarInput()
        {
            var proxyScalarInput = GetProxyScalarInput();
            if (proxyScalarInput != null)
            {
                Cleanup(proxyScalarInput);
                Setup(proxyScalarInput, this.GetScalarFlowIndex());
                return true;
            }
            return false;
        }

        private RowPresenter RowPresenter
        {
            get { return this.GetRowPresenter(); }
        }

        private UIElement _inertElement;
        public UIElement InertElement
        {
            get { return _inertElement;  }
            private set
            {
                if (_inertElement == value)
                    return;

                var oldValue = _inertElement;
                _inertElement = value;
                OnChildChanged(oldValue, value);
            }
        }

        private void OnChildChanged(UIElement oldValue, UIElement newValue)
        {
            if (oldValue != null)
            {
                RemoveLogicalChild(oldValue);
                RemoveVisualChild(oldValue);
            }
            if (newValue != null)
            {
                AddLogicalChild(newValue);
                AddVisualChild(newValue);
            }
        }

        private UIElement _editorElement;
        public UIElement EditorElement
        {
            get { return _editorElement; }
            private set
            {
                if (_editorElement == value)
                    return;

                var oldValue = _editorElement;
                _editorElement = value;
                OnChildChanged(oldValue, value);
            }
        }

        public UIElement Child
        {
            get { return IsEditing ? EditorElement : InertElement; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Child;
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                var child = Child;
                return child == null ? EmptyEnumerator.Singleton : new SingleChildEnumerator(child);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return Child == null ? 0 : 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            if (child != null)
            {
                child.Measure(constraint);
                return child.DesiredSize;
            }
            return default(Size);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child != null)
                child.Arrange(new Rect(arrangeSize));
            return arrangeSize;
        }

        private DataView DataView
        {
            get { return DataView.GetCurrent(this); }
        }

        private DataPresenter DataPresenter
        {
            get { return DataView?.DataPresenter; }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (DataView == null)
                return;

            if (this.GetBinding() != null)  // binding can be null when removed from UIElementCollection.
                IsEditing = GetConfiguration().QueryEditingMode(this);
        }

        void IRowElement.Setup(RowPresenter p)
        {
            Setup(GetProxyRowInput(), p);
            SetupCommands(p.DataPresenter);
        }

        void IScalarElement.Setup(ScalarPresenter p)
        {
            Setup(GetProxyScalarInput(), p.FlowIndex);
            SetupCommands(p.DataPresenter);
        }

        private void Setup(IProxyRowInput proxyRowInput, RowPresenter rowPresenter)
        {
            if (proxyRowInput == null)
                return;

            if (IsEditing)
            {
                InertElement = null;
                EditorElement = GenerateElement(proxyRowInput.EditorBinding, rowPresenter);
                DataPresenter?.GetService<IChildInitializer>()?.InitializeEditorElement(this);
            }
            else
            {
                EditorElement = null;
                InertElement = GenerateElement(proxyRowInput.InertBinding, rowPresenter);
                DataPresenter?.GetService<IChildInitializer>()?.InitializeInertElement(this);
            }
            InvalidateMeasure();
        }

        private void Setup(IProxyScalarInput proxyScalarInput, int flowIndex)
        {
            if (proxyScalarInput == null)
                return;

            if (IsEditing)
            {
                InertElement = null;
                EditorElement = GenerateElement(proxyScalarInput.EditorBinding, flowIndex);
                DataPresenter?.GetService<IChildInitializer>()?.InitializeEditorElement(this);
            }
            else
            {
                EditorElement = null;
                InertElement = GenerateElement(proxyScalarInput.InertBinding, flowIndex);
                DataPresenter?.GetService<IChildInitializer>()?.InitializeInertElement(this);
            }
            InvalidateMeasure();
        }

        private static UIElement GenerateElement(RowBinding binding, RowPresenter p)
        {
            binding.BeginSetup(null);
            var result = binding.Setup(p);
            binding.EndSetup();
            binding.Refresh(result);
            return result;
        }

        private static UIElement GenerateElement(ScalarBinding binding, int flowIndex)
        {
            binding.BeginSetup(null);
            var result = binding.Setup(flowIndex);
            binding.EndSetup();
            binding.Refresh(result);
            return result;
        }

        void IRowElement.Refresh(RowPresenter rowPresenter)
        {
            IsRowEditing = rowPresenter.IsEditing;

            var proxyRowInput = GetProxyRowInput();
            if (proxyRowInput == null)
                return;

            if (EditorElement != null)
                proxyRowInput.EditorBinding.Refresh(EditorElement);
            else if (InertElement != null)
                proxyRowInput.InertBinding.Refresh(InertElement);
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
            IsScalarEditing = scalarPresenter.DataPresenter.ScalarContainer.IsEditing;

            var proxyScalarInput = GetProxyScalarInput();
            if (proxyScalarInput == null)
                return;

            if (EditorElement != null)
                proxyScalarInput.EditorBinding.Refresh(EditorElement);
            else if (InertElement != null)
                proxyScalarInput.InertBinding.Refresh(InertElement);
        }

        void IRowElement.Cleanup(RowPresenter rowPresenter)
        {
            var proxyRowInput = GetProxyRowInput();
            Cleanup(proxyRowInput);
            CleanupCommands();
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
            var proxyScalarInput = GetProxyScalarInput();
            Cleanup(proxyScalarInput);
            CleanupCommands();
        }

        private void Cleanup(IProxyRowInput proxyRowInput)
        {
            if (proxyRowInput == null)
                return;

            if (EditorElement != null)
            {
                proxyRowInput.EditorBinding.Cleanup(EditorElement);
                EditorElement = null;
            }
            else if (InertElement != null)
            {
                proxyRowInput.InertBinding.Cleanup(InertElement);
                InertElement = null;
            }
        }

        private void Cleanup(IProxyScalarInput proxyScalarInput)
        {
            if (proxyScalarInput == null)
                return;

            if (EditorElement != null)
            {
                proxyScalarInput.EditorBinding.Cleanup(EditorElement);
                EditorElement = null;
            }
            else if (InertElement != null)
            {
                proxyScalarInput.InertBinding.Cleanup(InertElement);
                InertElement = null;
            }
        }

        private static InPlaceEditor GetCurrentInPlaceEditor(UIElement element)
        {
            var result = element as InPlaceEditor;
            return result ?? VisualTreeHelper.GetParent(element) as InPlaceEditor;
        }

        private static bool IsInPlaceEditor(UIElement element)
        {
            return GetCurrentInPlaceEditor(element) == element;
        }

        private IProxyRowInput GetProxyRowInput()
        {
            return (this.GetBinding() as RowBinding)?.RowInput as IProxyRowInput;
        }

        private IProxyScalarInput GetProxyScalarInput()
        {
            return (this.GetBinding() as ScalarBinding)?.ScalarInput as IProxyScalarInput;
        }

        UIElement IContainerElement.GetChild(int index)
        {
            if (index == 0)
                return InertElement;
            else if (index == 1)
                return EditorElement;
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
