﻿using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LocalColumnsInitializerAttribute : ParameterlessModelWireupAttribute
    {
        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.ChildDataSetsCreated; }
        }
    }
}