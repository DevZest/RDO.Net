using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ComputationAttribute : ParameterlessModelWireupAttribute
    {
        public bool IsAggregate { get; set; }

        protected override ModelWireupEvent WireupEvent
        {
            get { return IsAggregate ? ModelWireupEvent.ChildDataSetsCreated : ModelWireupEvent.Constructing; }
        }
    }
}
