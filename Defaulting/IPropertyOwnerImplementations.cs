using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dbquity {
    internal static class IPropertyOwnerImplementations {
        public static bool Change<T>(
            this IPropertyOwner owner, T oldValue, T value, Action setter, [CallerMemberName]string propertyName = null) {
            if (oldValue?.Equals(value) ?? value is null)
                return false;
            DoChange(owner, oldValue, value, setter, propertyName);
            return true;
        }
        private static void DoChange<T>(IPropertyOwner owner, T oldValue, T value, Action setter, string propertyName) {
            // TODO: propagate notification to derived properties on owner and consider linked owners
            string propertyIsDefaulted = propertyName + "IsDefaulted";
            PropertyInfo isDefaulted = owner.GetType().GetProperty(propertyIsDefaulted);
            bool isDefaultedChange = isDefaulted != null && isDefaulted.PropertyType == typeof(bool) &&
                ((bool)isDefaulted.GetValue(owner) ^ (value is null));
            if (isDefaultedChange)
                PropertyChangeNotifier.OnChanging(owner, propertyIsDefaulted, propertyName);
            else
                PropertyChangeNotifier.OnChanging(owner, propertyName);
            setter();
            if (isDefaultedChange)
                PropertyChangeNotifier.OnChanged(owner, propertyName, propertyIsDefaulted);
            else
                PropertyChangeNotifier.OnChanged(owner, propertyName);
        }
        public static bool Change<T>(
            this IPropertyOwner owner, T[] oldValue, T[] value, Action setter, [CallerMemberName]string propertyName = null) {
            if (oldValue?.SequenceEqual(value) ?? value is null)
                return false;
            DoChange(owner, oldValue, value, setter, propertyName);
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
    }
}