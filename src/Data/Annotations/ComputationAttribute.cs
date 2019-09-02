using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the column computation declaration from model level.
    /// </summary>
    [CrossReference(typeof(_ComputationAttribute))]
    [ModelDeclarationSpec(false, typeof(void))]
    public sealed class ComputationAttribute : ModelDeclarationAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ComputationAttribute"/>, by name.
        /// </summary>
        /// <param name="name">The name of the computation.</param>
        public ComputationAttribute(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ComputationAttribute"/>, by name and computation mode.
        /// </summary>
        /// <param name="name">The name of the computation.</param>
        /// <param name="mode">The computation mode.</param>
        public ComputationAttribute(string name, ComputationMode mode)
            : this(name)
        {
            Mode = mode;
        }

        /// <summary>
        /// Gets the computation mode.
        /// </summary>
        public ComputationMode? Mode { get; }

        private Action<Model> _wireupAction;
        /// <inheritdoc />
        protected sealed override void Initialize()
        {
            var methodInfo = GetMethodInfo(Array.Empty<Type>(), typeof(void));
            _wireupAction = BuildWireupAction(ModelType, methodInfo);
        }

        private Action<Model> BuildWireupAction(Type modelType, MethodInfo methodInfo)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, methodInfo);
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        /// <inheritdoc />
        protected override void Wireup(Model model)
        {
            _wireupAction(model);
        }

        /// <inheritdoc />
        protected override ModelWireupEvent WireupEvent
        {
            get { return Mode.HasValue ? ModelWireupEvent.ChildDataSetsCreated : ModelWireupEvent.Constructing; }
        }
    }
}
