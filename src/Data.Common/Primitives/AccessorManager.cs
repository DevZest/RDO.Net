using DevZest.Data.Utilities;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    internal sealed class AccessorManager<TTarget, TProperty>
    {
        private struct Key
        {
            public Key(IAccessor<TTarget, TProperty> item)
            {
                OwnerType = item.OwnerType;
                Name = item.Name;
            }

            public readonly Type OwnerType;

            public readonly string Name;

            public override int GetHashCode()
            {
                return OwnerType.GetHashCode() ^ Name.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is Key && this == (Key)obj;
            }

            public static bool operator ==(Key x, Key y)
            {
                return x.OwnerType == y.OwnerType && x.Name == y.Name;
            }

            public static bool operator !=(Key x, Key y)
            {
                return !(x == y);
            }
        }

        private struct PropertyInfo<TDerivedTarget, TDerivedProperty>
        {
            public static PropertyInfo<TDerivedTarget, TDerivedProperty>? FromGetter(Expression<Func<TDerivedTarget, TDerivedProperty>> getter)
            {
                var result = GetPropertyInfo(getter);
                return result.HasValue ? result : GetAttachedPropertyInfo(getter);
            }

            private static PropertyInfo<TDerivedTarget, TDerivedProperty>? GetPropertyInfo(Expression<Func<TDerivedTarget, TDerivedProperty>> getter)
            {
                var propExp = getter.Body as MemberExpression;
                if (propExp == null)
                    return null;
                return new PropertyInfo<TDerivedTarget, TDerivedProperty>(false, typeof(TDerivedTarget), propExp.Member.Name, getter);
            }

            private static PropertyInfo<TDerivedTarget, TDerivedProperty>? GetAttachedPropertyInfo(Expression<Func<TDerivedTarget, TDerivedProperty>> getter)
            {
                var callExp = getter.Body as MethodCallExpression;
                if (callExp == null)
                    return null;

                var callMethod = callExp.Method;
                if (!callMethod.IsStatic)
                    return null;

                var ownerType = callMethod.DeclaringType;
                var name = callExp == null ? null : callMethod.Name;
                if (!name.StartsWith("Get"))
                    return null;
                name = name.Substring("Get".Length);
                return new PropertyInfo<TDerivedTarget, TDerivedProperty>(true, ownerType, name, getter);
            }

            private PropertyInfo(bool isAttached, Type ownerType, string name, Expression<Func<TDerivedTarget, TDerivedProperty>> getter)
            {
                IsAttached = isAttached;
                OwnerType = ownerType;
                Name = name;
                Getter = getter;
            }

            public readonly bool IsAttached;

            public readonly Type OwnerType;

            public readonly string Name;

            public readonly Expression<Func<TDerivedTarget, TDerivedProperty>> Getter;
        }

        private abstract class AccessorImplBase<TDerivedTarget, TDerivedProperty> : Accessor<TDerivedTarget, TDerivedProperty>, IAccessor<TTarget, TProperty>
            where TDerivedTarget : TTarget
            where TDerivedProperty : TProperty
        {
            protected AccessorImplBase()
            {
            }

            protected void Init(string name,
                Func<TDerivedTarget, TDerivedProperty> getter,
                Action<TDerivedTarget, TDerivedProperty> setter,
                Func<Accessor<TDerivedTarget, TDerivedProperty>, TDerivedProperty> constructor,
                Action<TDerivedProperty> initializer)
            {
                _name = name;
                _getter = getter;
                _setter = setter;
                _constructor = () => constructor(this);
                _initializer = initializer;
            }

            string _name;
            public override string Name
            {
                get { return _name; }
            }

            Func<TDerivedTarget, TDerivedProperty> _getter;
            internal override Func<TDerivedTarget, TDerivedProperty> Getter
            {
                get { return _getter; }
            }

            Action<TDerivedTarget, TDerivedProperty> _setter;
            internal override Action<TDerivedTarget, TDerivedProperty> Setter
            {
                get { return _setter; }
            }

            Func<TDerivedProperty> _constructor;
            internal override Func<TDerivedProperty> Constructor
            {
                get { return _constructor; }
            }

            Action<TDerivedProperty> _initializer;
            internal override Action<TDerivedProperty> Initializer
            {
                get { return _initializer; }
            }

            TProperty IAccessor<TTarget, TProperty>.GetProperty(TTarget target)
            {
                return this.GetProperty((TDerivedTarget)target);
            }

            TProperty IAccessor<TTarget, TProperty>.Construct(TTarget target)
            {
                return this.Construct((TDerivedTarget)target);
            }
        }
        private sealed class AccessorImpl<TDerivedTarget, TDerivedProperty> : AccessorImplBase<TDerivedTarget, TDerivedProperty>
            where TDerivedTarget : TTarget
            where TDerivedProperty : TProperty
        {
            public AccessorImpl(PropertyInfo<TDerivedTarget, TDerivedProperty> propertyInfo,
                Func<Accessor<TDerivedTarget, TDerivedProperty>, TDerivedProperty> constructor,
                Action<TDerivedProperty> initializer)
            {
                Debug.Assert(constructor != null);

                var name = propertyInfo.Name;
                var setter = GetSetter(name);
                if (setter == null)
                    throw new ArgumentException(Strings.Accessor_InvalidGetter, nameof(propertyInfo));
                Init(name, propertyInfo.Getter.Compile(), setter, constructor, initializer);
            }

            private static Action<TDerivedTarget, TDerivedProperty> GetSetter(string name)
            {
                try
                {
                    var paramTargetExp = Expression.Parameter(typeof(TDerivedTarget));
                    var paramValueExp = Expression.Parameter(typeof(TDerivedProperty));

                    var propExp = Expression.PropertyOrField(paramTargetExp, name);
                    var assignExp = Expression.Assign(propExp, paramValueExp);

                    return Expression.Lambda<Action<TDerivedTarget, TDerivedProperty>>(assignExp, paramTargetExp, paramValueExp).Compile();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public override Type OwnerType
            {
                get { return typeof(TDerivedTarget); }
            }
        }

        private sealed class AttachedAccessorImpl<TDerivedTarget, TDerivedProperty> : AccessorImplBase<TDerivedTarget, TDerivedProperty>
            where TDerivedTarget : class, TTarget
            where TDerivedProperty : class, TProperty
        {
            private ConditionalWeakTable<TDerivedTarget, TDerivedProperty> _storage = new ConditionalWeakTable<TDerivedTarget, TDerivedProperty>();

            TDerivedProperty GetStoredProperty(TDerivedTarget target)
            {
                return _storage.GetValue(target, x => null);
            }

            void SetStoredProperty(TDerivedTarget target, TDerivedProperty value)
            {
                _storage.Add(target, value);
            }

            public AttachedAccessorImpl(PropertyInfo<TDerivedTarget, TDerivedProperty> propertyInfo,
                Func<Accessor<TDerivedTarget, TDerivedProperty>, TDerivedProperty> constructor,
                Action<TDerivedProperty> initializer)
            {
                Init(propertyInfo.Name, GetStoredProperty, SetStoredProperty, constructor, initializer);
                _ownerType = propertyInfo.OwnerType;
            }

            Type _ownerType;
            public override Type OwnerType
            {
                get { return _ownerType; }
            }
        }

        private sealed class RegistrationCollection : KeyedCollection<Key, IAccessor<TTarget, TProperty>>
        {
            protected override Key GetKeyForItem(IAccessor<TTarget, TProperty> item)
            {
                return new Key(item);
            }
        }

        private Dictionary<Type, RegistrationCollection> _registrations = new Dictionary<Type, RegistrationCollection>();
        private Dictionary<Type, ReadOnlyCollection<IAccessor<TTarget, TProperty>>> _resultRegistrations = new Dictionary<Type, ReadOnlyCollection<IAccessor<TTarget, TProperty>>>();

        public Accessor<TDerivedTarget, TDerivedProperty> Register<TDerivedTarget, TDerivedProperty>(
            Expression<Func<TDerivedTarget, TDerivedProperty>> getter,
            Func<Accessor<TDerivedTarget, TDerivedProperty>, TDerivedProperty> constructor,
            Action<TDerivedProperty> initializer = null)
            where TDerivedTarget : class, TTarget
            where TDerivedProperty : class, TProperty
        {
            Debug.Assert(getter != null);
            var propertyInfo = PropertyInfo<TDerivedTarget, TDerivedProperty>.FromGetter(getter);
            if (propertyInfo == null)
                throw new ArgumentException(Strings.Accessor_InvalidGetter);

            var info = propertyInfo.Value;
            if (info.IsAttached)
            {
                var result = new AttachedAccessorImpl<TDerivedTarget, TDerivedProperty>(info, constructor, initializer);
                Register(result);
                return result;
            }
            else
            {
                var result = new AccessorImpl<TDerivedTarget, TDerivedProperty>(info, constructor, initializer);
                Register(result);
                return result;
            }
        }

        private void Register(IAccessor<TTarget, TProperty> item)
        {
            Type targetType = item.ParentType;
            lock (_resultRegistrations) // ensure thread safety
            {
                if (_resultRegistrations.ContainsKey(targetType))
                    throw new InvalidOperationException(Strings.Accessor_RegisterAfterUse(targetType.FullName));

                RegistrationCollection registrations;
                if (!_registrations.TryGetValue(targetType, out registrations))
                {
                    registrations = new RegistrationCollection();
                    _registrations.Add(targetType, registrations);
                }

                if (registrations.Contains(new Key(item)))
                    throw new InvalidOperationException(Strings.Accessor_RegisterDuplicate(item.OwnerType.FullName, item.Name));

                registrations.Add(item);
            }
        }

        public ReadOnlyCollection<IAccessor<TTarget, TProperty>> GetAll(Type targetType)
        {
            ReadOnlyCollection<IAccessor<TTarget, TProperty>> result;
            if (_resultRegistrations.TryGetValue(targetType, out result))
                return result;

            return SyncGetAll(targetType);
        }

        private ReadOnlyCollection<IAccessor<TTarget, TProperty>> SyncGetAll(Type targetType)
        {
            lock (_resultRegistrations) // ensure thread safety
            {
                return GetAccessors(targetType);
            }
        }

        private static readonly ReadOnlyCollection<IAccessor<TTarget, TProperty>> Empty =
            new ReadOnlyCollection<IAccessor<TTarget, TProperty>>(new IAccessor<TTarget, TProperty>[0]);
        private ReadOnlyCollection<IAccessor<TTarget, TProperty>> GetAccessors(Type targetType)
        {
            Debug.Assert(targetType != null);

            ReadOnlyCollection<IAccessor<TTarget, TProperty>> result;
            if (_resultRegistrations.TryGetValue(targetType, out result))
                return result;

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(targetType.TypeHandle);  // Ensure type initialized
            RegistrationCollection registrations;
            if (_registrations.TryGetValue(targetType, out registrations))
            {
                _registrations.Remove(targetType);
                result = new ReadOnlyCollection<IAccessor<TTarget, TProperty>>(registrations);
            }
            else
                result = Empty;

            var baseType = targetType.GetTypeInfo().BaseType;
            if (baseType != null)
            {
                if (baseType.GetTypeInfo().IsAbstract)
                    result = result.Concat(GetAccessors(baseType));
                else
                    result = GetAccessors(baseType).Concat(result);
            }
            _resultRegistrations.Add(targetType, result);
            return result;
        }
    }
}
