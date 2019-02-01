using System;

namespace Dbquity {
    using Implementation;
    public static class IPropertyOwnerExtensions {
        public static string IsDefaultedPropertyName(string propertyName) => propertyName + nameof(IsDefaulted);
        public static bool CanBeDefaulted(this IPropertyOwner owner, string propertyName) =>
            owner.HasProperty(propertyName) ? owner.HasProperty(IsDefaultedPropertyName(propertyName)) :
                throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
        public static object GetDefault(this IPropertyOwner owner, string propertyName) =>
            owner[propertyName + nameof(IsDefaulted)];
        public static void SetToDefault(this IPropertyOwner owner, string propertyName) {
            if (owner.CanBeDefaulted(propertyName))
                owner[propertyName] = null;
            else
                throw CannotBeDefaultedException(propertyName);
        }
        public static bool IsDefaulted(this IPropertyOwner owner, string propertyName) => owner.CanBeDefaulted(propertyName) ?
            (bool)owner[IsDefaultedPropertyName(propertyName)] : throw CannotBeDefaultedException(propertyName);
        public static Exception CannotBeDefaultedException(string propertyName) =>
            new ArgumentOutOfRangeException(nameof(propertyName), $"'{propertyName}' cannot be defaulted.");
        /// <summary>
        /// Intended use:
        /// <see cref="IPropertyOwner"/> o = ...;
        /// o.PropertyChanging += (s, e) => { ... };
        /// o.PropertyChanged += (s, e) => { ... };
        /// using (o.DelayedPropertyChangeNotification()) {
        ///     o["A"] = ...; // Changing, but not Changed, fires for A
        ///     o["B"] = ...; // Changing, but not Changed, fires for B
        ///     o["A"] = ...; // nothing fires
        ///     ...
        /// } // Changed fires once for A and once for B
        /// o["B"] = ...; // Changing and Changed both fire for B
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static IDisposable DelayedPropertyChangeNotification(this IPropertyOwner owner) =>
            new IPropertyOwnerImplementations.PropertyChangeNotifier(owner);
    }
}