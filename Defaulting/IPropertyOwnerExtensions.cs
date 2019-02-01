using System;

namespace Dbquity {
    using Implementation;
    public static class IPropertyOwnerExtensions {
        public static string IsDefaultedPropertyName(string propertyName) => propertyName + nameof(IsDefaulted);
        public static void SetToDefault(this IPropertyOwner owner, string propertyName) => owner[propertyName] = null;
        public static bool IsDefaulted(this IPropertyOwner owner, string propertyName) => (bool)owner[IsDefaultedPropertyName(propertyName)];
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