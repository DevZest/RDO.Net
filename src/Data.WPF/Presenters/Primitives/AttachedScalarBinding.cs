using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class AttachedScalarBinding : ScalarBinding, IDisposable
    {
        internal static AttachedScalarBinding Attach<T>(Template template, T element, ScalarBinding<T> baseBinding)
            where T : UIElement, new()
        {
            Debug.Assert(template != null);
            Debug.Assert(element != null && element.GetAttachedTo() == null);
            Debug.Assert(baseBinding != null && !baseBinding.IsSealed);
            return new TypedAttachedScalarBinding<T>(template, element, baseBinding);
        }

        internal static AttachedScalarBinding Detach(UIElement element)
        {
            Debug.Assert(element != null && GetAttachedScalarBinding(element) != null);
            var result = GetAttachedScalarBinding(element);
            result.Dispose();
            return result;
        }

        private static ConditionalWeakTable<UIElement, AttachedScalarBinding> s_bindingsByElement = new ConditionalWeakTable<UIElement, AttachedScalarBinding>();

        internal static AttachedScalarBinding GetAttachedScalarBinding(UIElement element)
        {
            if (s_bindingsByElement.TryGetValue(element, out var result))
                return result;
            return null;
        }

        protected AttachedScalarBinding(Template template)
        {
            Debug.Assert(template != null);
            _template = template;
        }

        public abstract void Dispose();

        private readonly Template _template;
        public override Template Template
        {
            get { return _template; }
        }

        public BasePresenter Presenter
        {
            get { return Template.Presenter; }
        }

        public abstract UIElement Element { get; }

        public abstract ScalarBinding BaseBinding { get; }

        public abstract void Mount();

        private sealed class TypedAttachedScalarBinding<T> : AttachedScalarBinding
            where T : UIElement, new()
        {
            public TypedAttachedScalarBinding(Template template, T element, ScalarBinding<T> baseBinding)
                : base(template)
            {
                _element = element;
                s_bindingsByElement.Add(element, this);
                _baseBinding = baseBinding;
                _baseBinding.Seal(this, 0);
            }

            public override void Mount()
            {
                _baseBinding.BeginSetup(_element);
                _baseBinding.Setup(0);
                _baseBinding.Refresh(_element);
                _baseBinding.EndSetup();
                Presenter.ViewRefreshing += OnViewRefreshing;
            }

            private readonly T _element;
            public override UIElement Element
            {
                get { return _element; }
            }

            private readonly ScalarBinding<T> _baseBinding;
            public override ScalarBinding BaseBinding
            {
                get { return _baseBinding; }
            }

            public override IReadOnlyList<ScalarBinding> ChildBindings
            {
                get { return _baseBinding; }
            }

            public override Input<ScalarBinding, IScalars> ScalarInput
            {
                get { return null; }
            }

            public override bool IsRefreshing
            {
                get { return _baseBinding.IsRefreshing; }
            }

            private void OnViewRefreshing(object sender, EventArgs e)
            {
                _baseBinding.Refresh(_element);
            }

            public override Type ViewType
            {
                get { return null; }
            }

            internal override void BeginSetup(int startOffset, UIElement[] elements)
            {
                throw new NotSupportedException();
            }

            internal override void BeginSetup(UIElement element)
            {
                throw new NotSupportedException();
            }

            internal override UIElement Setup(int flowIndex)
            {
                throw new NotSupportedException();
            }

            internal override UIElement GetChild(UIElement parent, int index)
            {
                if (index == 0)
                    return _element;
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            internal override void FlushInput(UIElement element)
            {
                throw new NotSupportedException();
            }

            internal override UIElement GetSettingUpElement()
            {
                throw new NotSupportedException();
            }

            internal override void EndSetup()
            {
                throw new NotSupportedException();
            }

            internal override void Refresh(UIElement element)
            {
                throw new NotSupportedException();
            }

            internal override void Cleanup(UIElement element)
            {
                throw new NotSupportedException();
            }

            public override void Dispose()
            {
                Presenter.ViewRefreshing -= OnViewRefreshing;
                _baseBinding.Cleanup(_element);
                s_bindingsByElement.Remove(_element);
            }
        }
    }
}
