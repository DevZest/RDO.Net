using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ComputationAttribute : ParameterlessModelWireupAttribute
    {
        public ComputationAttribute()
        {
        }

        public ComputationAttribute(ComputationMode mode)
        {
            Mode = mode;
        }

        public ComputationMode? Mode { get; private set; }

        protected override ModelWireupEvent WireupEvent
        {
            get { return Mode.HasValue ? ModelWireupEvent.ChildDataSetsCreated : ModelWireupEvent.Constructing; }
        }
    }
}
