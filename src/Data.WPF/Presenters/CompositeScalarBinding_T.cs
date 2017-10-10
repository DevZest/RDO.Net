//using DevZest.Data.Presenters.Primitives;
//using System.Windows;
//using System;
//using System.Collections.Generic;

//namespace DevZest.Data.Presenters
//{
//    public sealed class CompositeScalarBinding<T> : ScalarBindingBase<T>
//        where T : UIElement, new()
//    {
//        private List<ScalarBinding> _childBindings = new List<ScalarBinding>();
//        private List<Func<T, UIElement>> _childGetters = new List<Func<T, UIElement>>();

//        public CompositeScalarBinding<T> AddChild<TChild>(ScalarBinding<TChild> childBinding, Func<T, TChild> childGetter)
//            where TChild : UIElement, new()
//        {
//            Binding.VerifyAdding(childBinding, nameof(childBinding));

//            if (childGetter == null)
//                throw new ArgumentNullException(nameof(childGetter));

//            VerifyNotSealed();
//            _childBindings.Add(childBinding);
//            _childGetters.Add(childGetter);
//            childBinding.Seal(this, _childBindings.Count - 1);
//            return this;
//        }

//        public IReadOnlyList<Binding> ChildBindings
//        {
//            get { return _childBindings; }
//        }

//        internal override UIElement GetChild(UIElement parent, int index)
//        {
//            return _childGetters[index]((T)parent);
//        }

//        private ICompositeView[] InitializeFrom(int startOffset)
//        {
//            _settingUpStartOffset = startOffset;

//            if (startOffset == FlowRepeatCount)
//                return Array<ICompositeView>.Empty;

//            var count = FlowRepeatCount - startOffset;
//            var result = new ICompositeView[count];
//            for (int i = 0; i < count; i++)
//                result[i] = InitializeAt(startOffset + i);
//            return result;
//        }

//        private ICompositeView InitializeAt(int flowIndex)
//        {
//            var result = CreateView();
//            var resultElement = (UIElement)result;
//            resultElement.SetScalarFlowIndex(flowIndex);
//            ScalarPresenter.SetFlowIndex(flowIndex);
//            result.BindingDispatcher.Initialize(this);
//            OnCreated(resultElement);
//            ScalarPresenter.SetFlowIndex(-1);
//            return result;
//        }

//        private int _settingUpStartOffset;
//        private ICompositeView[] _settingUpViews;
//        private IReadOnlyList<ICompositeView> SettingUpViews
//        {
//            get { return _settingUpViews; }
//        }

//        private ICompositeView SettingUpView { get; set; }

//        internal sealed override UIElement GetSettingUpElement()
//        {
//            Debug.Assert(!FlowRepeatable);
//            return (UIElement)SettingUpView;
//        }

//        internal sealed override void Initialize(int startOffset)
//        {
//            if (FlowRepeatable)
//                _settingUpViews = InitializeFrom(startOffset);
//            else if (startOffset == 0)
//                SettingUpView = InitializeAt(0);
//        }

//        internal sealed override void BeginSetup(UIElement value)
//        {
//            Debug.Assert(!FlowRepeatable);
//            if (value == null)
//                SettingUpView = InitializeAt(0);
//            else
//            {
//                SettingUpView = (ICompositeView)value;
//                SettingUpView.BindingDispatcher.BeginSetup();
//            }
//        }

//        internal sealed override void PrepareSettingUpElement(int flowIndex)
//        {
//            if (FlowRepeatable)
//            {
//                Debug.Assert(SettingUpViews != null);
//                SettingUpView = SettingUpViews[flowIndex - _settingUpStartOffset];
//            }
//        }

//        internal override void ClearSettingUpElement()
//        {
//            if (FlowRepeatable)
//                SettingUpView = null;
//        }

//        internal sealed override UIElement Setup(int flowIndex)
//        {
//            EnterSetup(flowIndex);

//            var result = SettingUpView;
//            for (int i = 0; i < _childBindings.Count; i++)
//                _childBindings[i].Setup(flowIndex);

//            ExitSetup();
//            return (UIElement)result;
//        }

//        private bool _isRefreshing;
//        public sealed override bool IsRefreshing
//        {
//            get { return _isRefreshing; }
//        }

//        internal sealed override void Refresh(UIElement element)
//        {
//            _isRefreshing = true;
//            ((ICompositeView)element).BindingDispatcher.Refresh();
//            _isRefreshing = false;
//        }

//        internal sealed override void Cleanup(UIElement element)
//        {
//            var view = (ICompositeView)element;
//            view.BindingDispatcher.Cleanup();
//            element.SetScalarFlowIndex(0);
//        }

//        internal sealed override void EndSetup()
//        {
//            if (FlowRepeatable)
//            {
//                for (int i = 0; i < SettingUpViews.Count; i++)
//                    SettingUpViews[i].BindingDispatcher.EndSetup();
//            }
//            else
//                SettingUpView.BindingDispatcher.EndSetup();
//            _settingUpViews = null;
//            SettingUpView = null;
//        }

//        internal sealed override void FlushInput(UIElement element)
//        {
//            ((ICompositeView)element).BindingDispatcher.FlushInput();
//        }
//    }
//}
