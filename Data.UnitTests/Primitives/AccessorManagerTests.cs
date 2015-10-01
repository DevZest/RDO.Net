using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class AccessorManagerTests
    {
        class TargetType
        {
            static readonly AccessorManager<TargetType, PropertyType> s_accessorManager = new AccessorManager<TargetType, PropertyType>();

            static TargetType()
            {
                RegisterProperty<TargetType, PropertyType>(x => x.Property1);
                RegisterProperty<TargetType, PropertyType>(x => x.Property2);
            }

            public static void RegisterProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> getter)
                where TTarget : TargetType
                where TProperty : PropertyType, new()
            {
                s_accessorManager.Register<TTarget, TProperty>(getter, a => new TProperty(), null);
            }

            public static Accessor<TTarget, TProperty> RegisterAttachedProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> getter)
                where TTarget : TargetType
                where TProperty : PropertyType, new()
            {
                return s_accessorManager.Register<TTarget, TProperty>(getter, a => new TProperty(), null);
            }

            public TargetType()
            {
                foreach (var accessor in Accessors)
                    accessor.Construct(this);
            }

            public ReadOnlyCollection<IAccessor<TargetType, PropertyType>> Accessors
            {
                get { return s_accessorManager.GetAll(this.GetType()); }
            }

            public PropertyType Property1 { get; private set; }

            public PropertyType Property2 { get; private set; }
        }

        class PropertyType
        {
        }

        class TargetTypeDerived : TargetType
        {
            static TargetTypeDerived()
            {
                RegisterProperty<TargetTypeDerived, PropertyTypeDerived>(x => x.Property3);
            }

            public PropertyTypeDerived Property3 { get; private set; }
        }

        class TargetTypeDerived2 : TargetType
        {
            static TargetTypeDerived2()
            {
                RegisterProperty<TargetTypeDerived2, PropertyTypeDerived>(x => x.Property3);
            }

            public PropertyTypeDerived Property3 { get; private set; }
        }

        class PropertyTypeDerived : PropertyType
        {
        }

        class TargetTypeAttached : TargetType
        {
        }

        class Extension
        {
            public static readonly Accessor<TargetTypeAttached, PropertyType> Property3Accessor = TargetType.RegisterAttachedProperty<TargetTypeAttached, PropertyType>(x => GetProperty3(x));
            public static readonly Accessor<TargetTypeAttached, PropertyType> Property4Accessor = TargetType.RegisterAttachedProperty<TargetTypeAttached, PropertyType>(x => GetProperty4(x));

            public static PropertyType GetProperty3(TargetTypeAttached target)
            {
                return Property3Accessor.GetProperty(target);
            }

            public static PropertyType GetProperty4(TargetTypeAttached target)
            {
                return Property4Accessor.GetProperty(target);
            }

            public static TargetTypeAttached CreateTarget()
            {
                return new TargetTypeAttached();
            }
        }

        [TestMethod]
        public void AccessorManager_RegisterProperty()
        {
            TargetType target = new TargetType();
            Assert.AreEqual(2, target.Accessors.Count, "Target object should have 2 properties.");
            VerifyAccessor(target.Accessors[0], typeof(TargetType), "Property1", typeof(TargetType), typeof(PropertyType));
            VerifyAccessor(target.Accessors[1], typeof(TargetType), "Property2", typeof(TargetType), typeof(PropertyType));

            Assert.IsNotNull(target.Property1);
            Assert.IsNotNull(target.Property2);
            Assert.AreEqual(target.Property1, target.Accessors[0].GetProperty(target));
            Assert.AreEqual(target.Property2, target.Accessors[1].GetProperty(target));
        }

        [TestMethod]
        public void AccessorManager_RegisterProperty_in_derived_class()
        {
            TargetTypeDerived target = new TargetTypeDerived();
            Assert.AreEqual(3, target.Accessors.Count, "Derived target object should have 3 properties.");
            VerifyAccessor(target.Accessors[0], typeof(TargetType), "Property1", typeof(TargetType), typeof(PropertyType));
            VerifyAccessor(target.Accessors[1], typeof(TargetType), "Property2", typeof(TargetType), typeof(PropertyType));
            VerifyAccessor(target.Accessors[2], typeof(TargetTypeDerived), "Property3", typeof(TargetTypeDerived), typeof(PropertyTypeDerived));

            Assert.IsNotNull(target.Property1);
            Assert.IsNotNull(target.Property2);
            Assert.IsNotNull(target.Property3);
            Assert.AreEqual(target.Property1, target.Accessors[0].GetProperty(target));
            Assert.AreEqual(target.Property2, target.Accessors[1].GetProperty(target));
            Assert.AreEqual(target.Property3, target.Accessors[2].GetProperty(target));

            TargetTypeDerived2 target2 = new TargetTypeDerived2();
            Assert.AreEqual(3, target2.Accessors.Count);
        }

        [TestMethod]
        public void AccessorManager_RegisterAttachedProperty()
        {
            TargetTypeAttached target = Extension.CreateTarget();
            Assert.AreEqual(4, target.Accessors.Count, "Derived target object should have 4 properties.");
            VerifyAccessor(target.Accessors[0], typeof(TargetType), "Property1", typeof(TargetType), typeof(PropertyType));
            VerifyAccessor(target.Accessors[1], typeof(TargetType), "Property2", typeof(TargetType), typeof(PropertyType));
            VerifyAccessor(target.Accessors[2], typeof(Extension), "Property3", typeof(TargetTypeAttached), typeof(PropertyType));
            VerifyAccessor(target.Accessors[3], typeof(Extension), "Property4", typeof(TargetTypeAttached), typeof(PropertyType));

            Assert.IsNotNull(target.Property1);
            Assert.IsNotNull(target.Property2);
            Assert.IsNotNull(Extension.GetProperty3(target));
            Assert.IsNotNull(Extension.GetProperty4(target));
            Assert.AreEqual(target.Property1, target.Accessors[0].GetProperty(target));
            Assert.AreEqual(target.Property2, target.Accessors[1].GetProperty(target));
            Assert.AreEqual(Extension.GetProperty3(target), target.Accessors[2].GetProperty(target));
            Assert.AreEqual(Extension.GetProperty4(target), target.Accessors[3].GetProperty(target));
        }

        void VerifyAccessor(IAccessor<TargetType, PropertyType> accessor, Type expectedOwnerType, string expectedName, Type expectedTargetType, Type expectedPropertyType)
        {
            Assert.AreEqual(expectedOwnerType, accessor.OwnerType, string.Format("The owner type should be '{0}'", expectedOwnerType));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AccessorManager_throws_exception_when_register_after_use()
        {
            new TargetType();
            TargetType.RegisterProperty<TargetType, PropertyType>(x => x.Property1);
        }
    }
}
