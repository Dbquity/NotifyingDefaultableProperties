﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dbquity.Implementation {
    public static class PropertyOwnerImplementations {
        static bool AreEqual(object a, object b) {
            if (a is IEnumerable enumerableA && b is IEnumerable enumerableB) {
                IEnumerator enumeratorA = enumerableA.GetEnumerator();
                IEnumerator enumeratorB = enumerableB.GetEnumerator();
                while (enumeratorA.MoveNext())
                    if (!enumeratorB.MoveNext() || !AreEqual(enumeratorA.Current, enumeratorB.Current))
                        return false;
                return !enumeratorB.MoveNext();
            }
            return a?.Equals(b) ?? b is null;
        }
        public static bool Change<T>(
            this IPropertyOwner owner, T oldValue, T value, Action setter, [CallerMemberName]string propertyName = null,
            params string[] derivedProperties) {
            if (AreEqual(oldValue, value))
                return false;
            string isDefaultedPropertyName = PropertyOwnerExtensions.IsDefaultedPropertyName(propertyName);
            bool isDefaultedChange = owner.HasProperty(isDefaultedPropertyName) &&
                (owner.IsDefaulted(propertyName) ^ ((object)value is null));
            PropertyChangeNotifier.OnChanging(owner, propertyName);
            if (isDefaultedChange)
                PropertyChangeNotifier.OnChanging(owner, isDefaultedPropertyName);
            if (derivedProperties?.Any() ?? false)
                PropertyChangeNotifier.OnChanging(owner, derivedProperties);
            setter();
            PropertyChangeNotifier.OnChanged(owner, propertyName);
            if (isDefaultedChange)
                PropertyChangeNotifier.OnChanged(owner, isDefaultedPropertyName);
            if (derivedProperties?.Any() ?? false)
                PropertyChangeNotifier.OnChanged(owner, derivedProperties);
            return true;
        }
        internal class PropertyChangeNotifier : IDisposable {
            static Dictionary<IPropertyOwner, PropertyChangeNotifier> activeNotifiers =
                new Dictionary<IPropertyOwner, PropertyChangeNotifier>();
            readonly IPropertyOwner owner;
            public PropertyChangeNotifier(IPropertyOwner owner) {
                if (activeNotifiers.ContainsKey(owner))
                    throw new InvalidOperationException($"Cannot nest delayed notification for {owner}.");
                activeNotifiers.Add(this.owner = owner, this);
            }
            HashSet<string> changingProperties = new HashSet<string>();
            public static void OnChanging(IPropertyOwner owner, params string[] propertyNames) {
                if (activeNotifiers.TryGetValue(owner, out PropertyChangeNotifier pcn)) {
                    string[] newChangingProperties = propertyNames.Except(pcn.changingProperties).ToArray();
                    if (newChangingProperties.Length > 0) {
                        pcn.changingProperties.UnionWith(newChangingProperties);
                        owner.NotifyChanging(newChangingProperties);
                    }
                } else
                    owner.NotifyChanging(propertyNames);
            }
            HashSet<string> changedProperties = new HashSet<string>();
            public static void OnChanged(IPropertyOwner owner, params string[] propertyNames) {
                if (activeNotifiers.TryGetValue(owner, out PropertyChangeNotifier pcn)) {
                    if (pcn.changedProperties != null)
                        if (propertyNames is null || propertyNames.Length == 0)
                            pcn.changedProperties = null;
                        else
                            pcn.changedProperties.UnionWith(propertyNames);
                } else
                    owner.NotifyChanged(propertyNames);
            }
            void IDisposable.Dispose() {
                activeNotifiers.Remove(owner); // must remove owner first, because each notify may trigger a PropertyChangeNotifier
                if (changedProperties is null)
                    owner.NotifyChanged();
                else
                    owner.NotifyChanged(changedProperties.ToArray());
            }
        }
        public static void NotifyChanging(
            this IPropertyOwner owner, PropertyChangingEventHandler propertyChanging, params string[] propertyNames) {
            if (propertyChanging != null)
                foreach (string propertyName in propertyNames)
                    propertyChanging(owner, new PropertyChangingEventArgs(propertyName));
        }
        public static void NotifyChanged(
            this IPropertyOwner owner, PropertyChangedEventHandler propertyChanged, params string[] propertyNames) {
            if (propertyChanged != null)
                foreach (string propertyName in propertyNames)
                    propertyChanged(owner, new PropertyChangedEventArgs(propertyName));
        }
        public static Exception UnknownPropertyException(string propertyName) =>
            new ArgumentOutOfRangeException(nameof(propertyName), $"Unknown property: '{propertyName}'.");
        public static Exception CannotSetPropertyException(string propertyName) =>
            new ArgumentOutOfRangeException(nameof(propertyName), $"Cannot set: '{propertyName}'.");
        public static Exception TryReflectionGet(this IPropertyOwner owner, string propertyName, out object value) {
            PropertyInfo info = owner.GetType().GetProperty(propertyName);
            if (info is null) {
                value = null;
                return UnknownPropertyException(propertyName);
            }
            value = info.GetValue(owner);
            return null;
        }
        public static Exception TryReflectionSet(this IPropertyOwner owner, string propertyName, object value) {
            PropertyInfo info = owner.GetType().GetProperty(propertyName);
            if (info is null)
                return UnknownPropertyException(propertyName);
            if (!info.CanWrite)
                return CannotSetPropertyException(propertyName);
            try {
                info.SetValue(owner, value);
            } catch (Exception ex) {
                return ex.InnerException is null ? ex : ex.InnerException;
            }
            return null;
        }
        public static Exception TryDictionaryGet(this IPropertyOwner owner, IReadOnlyDictionary<string, object> bag,
            Func<object> defaultGetter, string propertyName, out object value) {
            if (!owner.HasProperty(propertyName)) {
                value = null;
                return UnknownPropertyException(propertyName);
            }
            if (propertyName.EndsWith(nameof(PropertyOwnerExtensions.IsDefaulted))) {
                string underlyingPropertyName =
                    propertyName.Substring(0, propertyName.Length - nameof(PropertyOwnerExtensions.IsDefaulted).Length);
                value = !bag.ContainsKey(underlyingPropertyName);
            } else if (!bag.TryGetValue(propertyName, out value))
                value = defaultGetter();
            return null;
        }
        public static Exception TryDictionarySet(this IPropertyOwner owner, IDictionary<string, object> bag,
            Func<object> defaultGetter, string propertyName, object value) {
            if (!owner.HasProperty(propertyName))
                return UnknownPropertyException(propertyName);
            if (propertyName.EndsWith(nameof(PropertyOwnerExtensions.IsDefaulted)))
                return CannotSetPropertyException(propertyName);
            if (!bag.TryGetValue(propertyName, out object oldValue))
                oldValue = defaultGetter();
            owner.Change(oldValue, value, () => {
                if (value is null)
                    bag.Remove(propertyName);
                else // TODO: check that value is of an assignable type - requires type info, which we don't have, yet
                    bag[propertyName] = value;
            }, propertyName);
            return null;
        }
    }
}