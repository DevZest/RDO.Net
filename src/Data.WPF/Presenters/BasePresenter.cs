using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class of <see cref="DataPresenter"/> and <see cref="SimplePresenter"/>.
    /// </summary>
    public abstract class BasePresenter : ScalarContainer.IOwner
    {
        /// <summary>
        /// Specifies how presenter is mounted.
        /// </summary>
        public enum MountMode
        {
            /// <summary>
            /// The presenter is initially shown.
            /// </summary>
            Show,

            /// <summary>
            /// The presenter is refreshed with view.
            /// </summary>
            Refresh,

            /// <summary>
            /// The underlying data of the presenter is reloaded.
            /// </summary>
            Reload
        }

        /// <summary>
        /// Provides data for <see cref="Mounted"/> event.
        /// </summary>
        public sealed class MountEventArgs
        {
            private static readonly MountEventArgs Show = new MountEventArgs(MountMode.Show);
            private static readonly MountEventArgs Refresh = new MountEventArgs(MountMode.Refresh);
            private static readonly MountEventArgs Reload = new MountEventArgs(MountMode.Reload);

            internal static MountEventArgs Select(MountMode mode)
            {
                if (mode == MountMode.Show)
                    return Show;
                else if (mode == MountMode.Refresh)
                    return Refresh;
                else if (mode == MountMode.Reload)
                    return Reload;
                else
                    return null;
            }

            private MountEventArgs(MountMode mode)
            {
                Mode = mode;
            }

            /// <summary>
            /// Gets the mode to specify how presenter is mounted.
            /// </summary>
            public MountMode Mode { get; private set; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BasePresenter"/> class.
        /// </summary>
        protected BasePresenter()
        {
            _scalarContainer = new ScalarContainer(this);
        }

        /// <summary>
        /// Occurs before view is invalidating.
        /// </summary>
        public event EventHandler ViewInvalidating = delegate { };

        /// <summary>
        /// Occurs after view is invalidated.
        /// </summary>
        public event EventHandler ViewInvalidated = delegate { };

        /// <summary>
        /// Occurs before view is refreshing.
        /// </summary>
        public event EventHandler ViewRefreshing = delegate { };

        /// <summary>
        /// Occurs after view is refreshed.
        /// </summary>
        public event EventHandler ViewRefreshed = delegate { };

        internal IBaseView View
        {
            get { return GetView(); }
            set { SetView(value); }
        }

        internal abstract IBaseView GetView();

        internal abstract void SetView(IBaseView value);

        /// <summary>
        /// Detaches this presenter from the view.
        /// </summary>
        public virtual void DetachView()
        {
            if (View == null)
                return;
            Template.UnmountAttachedScalarBindings();
            View.Presenter = null;
            View = null;
        }

        internal void AttachView(IBaseView value)
        {
            if (View != null)
                DetachView();

            Debug.Assert(View == null && value != null);
            View = value;
            View.Presenter = this;
        }

        /// <summary>
        /// Occurs when the view has been changed.
        /// </summary>
        public event EventHandler<EventArgs> ViewChanged = delegate { };

        /// <summary>
        /// Raises <see cref="ViewChanged"/> event.
        /// </summary>
        protected virtual void OnViewChanged()
        {
            CommandManager.InvalidateRequerySuggested();
            ViewChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="ViewInvalidating"/> event.
        /// </summary>
        protected internal virtual void OnViewInvalidating()
        {
            ViewInvalidating(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="ViewInvalidated"/> event.
        /// </summary>
        protected internal virtual void OnViewInvalidated()
        {
            ViewInvalidated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="ViewRefreshing"/> event.
        /// </summary>
        protected internal virtual void OnViewRefreshing()
        {
            ViewRefreshing(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="ViewRefreshed"/> event.
        /// </summary>
        protected internal virtual void OnViewRefreshed()
        {
            ViewRefreshed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets a value indicates whether this presenter is mounted.
        /// </summary>
        public bool IsMounted
        {
            get { return LayoutManager != null; }
        }

        /// <summary>
        /// Occurs when this presenter is mounted.
        /// </summary>
        public event EventHandler<MountEventArgs> Mounted = delegate { };

        internal void OnMounted(MountMode mode)
        {
            Template.MountAttachedScalarBindings();
            IsAttachedScalarBindingsInvalidated = true;
            OnMounted(MountEventArgs.Select(mode));
        }

        internal bool IsAttachedScalarBindingsInvalidated { get; private set; }

        internal void ResetIsAttachedScalarBindingsInvalidated()
        {
            IsAttachedScalarBindingsInvalidated = false;
        }

        /// <summary>
        /// Raises <see cref="Mounted"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMounted(MountEventArgs e)
        {
            Mounted(this, e);
        }

        internal abstract LayoutManager LayoutManager { get; }

        /// <summary>
        /// Gets the <see cref="Template"/> associated with this presenter.
        /// </summary>
        public Template Template
        {
            get { return LayoutManager?.Template; }
        }

        internal LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(DiagnosticMessages.BasePresenter_NotMounted);
            return LayoutManager;
        }

        private readonly ScalarContainer _scalarContainer;
        /// <summary>
        /// Gets the container for all scalar values.
        /// </summary>
        public ScalarContainer ScalarContainer
        {
            get { return _scalarContainer; }
        }

        /// <summary>
        /// Invalidates the rendering of the view.
        /// </summary>
        public void InvalidateView()
        {
            LayoutManager?.InvalidateView();
        }

        /// <summary>
        /// Temporarily suspends the view rendering.
        /// </summary>
        /// <remarks>Call <see cref="SuspendInvalidateView"/> and <see cref="ResumeInvalidateView"/> in tandem to suppress multiple
        /// view rendering for performance.</remarks>
        public void SuspendInvalidateView()
        {
            RequireLayoutManager().SuspendInvalidateView();
        }

        /// <summary>
        /// Resumes the usual view rendering.
        /// </summary>
        /// <remarks>Call <see cref="SuspendInvalidateView"/> and <see cref="ResumeInvalidateView"/> in tandem to suppress multiple
        /// view rendering for performance.</remarks>
        public void ResumeInvalidateView()
        {
            RequireLayoutManager().ResumeInvalidateView();
        }

        /// <summary>
        /// Gets an object which contains the scalar data validation logic.
        /// </summary>
        public ScalarValidation ScalarValidation
        {
            get { return LayoutManager?.ScalarValidation; }
        }

        /// <summary>
        /// Creates a new strongly typed scalar data which can be used as data binding source.
        /// </summary>
        /// <typeparam name="T">The type of the scalar data.</typeparam>
        /// <param name="value">The initial value of the scalar data.</param>
        /// <param name="equalityComparer">The equality comparer.</param>
        /// <returns>A new strongly typed scalar data which can be used as data binding source.</returns>
        protected Scalar<T> NewScalar<T>(T value = default(T), IEqualityComparer<T> equalityComparer = null)
        {
            return ScalarContainer.CreateNew(value, equalityComparer);
        }

        /// <summary>
        /// Creates a new strongly typed scalar data which can be used as data binding source, from exising property or field .
        /// </summary>
        /// <typeparam name="T">The type of the scalar data.</typeparam>
        /// <param name="propertyOrFieldName">The name of the property or field.</param>
        /// <param name="equalityComparer">The equality comparer.</param>
        /// <returns>A new strongly typed scalar data which can be used as data binding source, from exising property or field.</returns>
        protected Scalar<T> NewLinkedScalar<T>(string propertyOrFieldName, IEqualityComparer<T> equalityComparer = null)
        {
            var getter = this.GetPropertyOrFieldGetter<T>(propertyOrFieldName);
            var setter = this.GetPropertyOrFieldSetter<T>(propertyOrFieldName);
            return ScalarContainer.CreateNew(getter, setter, equalityComparer);
        }

        /// <summary>
        /// Creates a new strongly typed scalar data which can be used as data binding source, from existing getter and setter.
        /// </summary>
        /// <typeparam name="T">The type of the scalar data.</typeparam>
        /// <param name="getter">The exising getter of the scalar data.</param>
        /// <param name="setter">The existing setter of the scalar data.</param>
        /// <param name="equalityComparer">The equality comparer.</param>
        /// <returns>A new strongly typed scalar data which can be used as data binding source, from exising getter and setter.</returns>
        protected Scalar<T> NewLinkedScalar<T>(Func<T> getter, Action<T> setter, IEqualityComparer<T> equalityComparer = null)
        {
            getter.VerifyNotNull(nameof(getter));
            setter.VerifyNotNull(nameof(setter));
            return ScalarContainer.CreateNew(getter, setter, equalityComparer);
        }

        internal IScalarValidationErrors ValidateScalars()
        {
            var result = ScalarValidationErrors.Empty;
            for (int i = 0; i < ScalarContainer.Count; i++)
                result = ScalarContainer[i].Validate(result);
            return ValidateScalars(result);
        }

        /// <summary>
        /// Performs custom scalar data validation, typically cross multiple <see cref="Scalar{T}"/> objects.
        /// </summary>
        /// <param name="existingErrors">The existing validation errors.</param>
        /// <returns>Validation errors contains both existing errors and custom scalar data validation errors.</returns>
        /// <remarks>The default implementation returns <paramref name="existingErrors"/> directly.</remarks>
        protected virtual IScalarValidationErrors ValidateScalars(IScalarValidationErrors existingErrors)
        {
            return existingErrors;
        }

        /// <summary>
        /// Determines whether data input can be submitted without any validation error.
        /// </summary>
        public bool CanSubmitInput
        {
            get { return LayoutManager == null ? false : LayoutManager.CanSubmitInput; }
        }

        /// <summary>
        /// Tries to submit data input with validation.
        /// </summary>
        /// <param name="focusToErrorInput">A value indicates whether the UI element with validation error should have keyboard focus.</param>
        /// <returns><see langword="true"/> if data input submitted sccessfully, otherwise <see langword="false"/>.</returns>
        public virtual bool SubmitInput(bool focusToErrorInput = true)
        {
            RequireLayoutManager();

            ScalarValidation.Validate();
            if (!CanSubmitInput)
            {
                if (focusToErrorInput)
                    LayoutManager.FocusToInputError();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Invalidates the measurement state (layout) for the view.
        /// </summary>
        public void InvalidateMeasure()
        {
            RequireLayoutManager().InvalidateMeasure();
        }

        /// <summary>
        /// Invoked when the scalar data value is changed.
        /// </summary>
        /// <param name="scalars">The scalar data.</param>
        /// <remarks>The default implementation does nothing.</remarks>
        protected virtual void OnValueChanged(IScalars scalars)
        {
        }

        /// <summary>
        /// Invoked before ending editing mode for scalar data.
        /// </summary>
        /// <returns><see langword="true"/> if editing mode should be ended with saved changes, otherwise <see langword="false"/>.</returns>
        /// <remarks>The default implmentation returns <see langword="false"/> if there is any scalar validation error.</remarks>
        protected internal virtual bool QueryEndEditScalars()
        {
            return RequireLayoutManager().QueryEndEditScalars();
        }

        /// <summary>
        /// Override this method to let the user to confirm ending editing mode for scalar data.
        /// </summary>
        /// <returns><see langword="true"/> if editing mode should be ended with saved changes, otherwise <see langword="false"/>.</returns>
        /// <remarks>The default implementation always returns <see langword="true"/>.</remarks>
        protected internal virtual bool ConfirmEndEditScalars()
        {
            return true;
        }

        void ScalarContainer.IOwner.OnValueChanged(IScalars scalars)
        {
            OnValueChanged(scalars);
        }

        bool ScalarContainer.IOwner.QueryEndEdit()
        {
            return QueryEndEditScalars();
        }

        void ScalarContainer.IOwner.OnBeginEdit()
        {
            ScalarValidation.EnterEdit();
        }

        void ScalarContainer.IOwner.OnCancelEdit()
        {
            ScalarValidation.CancelEdit();
        }

        void ScalarContainer.IOwner.OnEndEdit()
        {
            ScalarValidation.ExitEdit();
        }

        void ScalarContainer.IOwner.OnEdit(Scalar scalar)
        {
            OnEdit(scalar);
        }

        /// <summary>
        /// Invoked when scalar data is being edited.
        /// </summary>
        /// <param name="scalar">The scalar data.</param>
        /// <remarks>The default implementation does nothing.</remarks>
        protected virtual void OnEdit(Scalar scalar)
        {
        }

        /// <summary>
        /// Formats an error message for failed async validator execution.
        /// </summary>
        /// <param name="asyncValidator">The async validator.</param>
        /// <returns>The result error message.</returns>
        protected internal virtual string FormatFaultMessage(AsyncValidator asyncValidator)
        {
            return AsyncValidationFault.FormatMessage(asyncValidator);
        }
    }
}
