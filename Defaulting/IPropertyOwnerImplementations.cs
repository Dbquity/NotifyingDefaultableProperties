using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dbquity.Implementation {
    public static class IPropertyOwnerImplementations {
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
            this IPropertyOwner owner, T oldValue, T value, Action setter, [CallerMemberName]string propertyName = null) {            
            if (AreEqual(oldValue, value))
                return false;
            // TODO: propagate notification to derived properties on owner and consider linked owners
            bool isDefaultedChange = owner.IsDefaulted(propertyName) ^ ((object)value is null);
            string propertyIsDefaulted = null;
            if (isDefaultedChange) {
                propertyIsDefaulted = IPropertyOwnerExtensions.IsDefaultedPropertyName(propertyName);
                PropertyChangeNotifier.OnChanging(owner, propertyIsDefaulted, propertyName);
            } else
                PropertyChangeNotifier.OnChanging(owner, propertyName);
            setter();
            if (isDefaultedChange)
                PropertyChangeNotifier.OnChanged(owner, propertyName, propertyIsDefaulted);
            else
                PropertyChangeNotifier.OnChanged(owner, propertyName);
            return true;
        }
        public class PropertyChangeNotifier : IDisposable {
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
    }
}