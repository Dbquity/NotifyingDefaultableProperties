using System.Collections.Generic;

namespace Dbquity {
    using Implementation;
    class ItemOnPropertyBag : ItemBase {
        Dictionary<string, object> bag = new Dictionary<string, object>();
        public override object this[string propertyName] {
            get {
                if (!HasProperty(propertyName))
                    throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
                if (propertyName.EndsWith(nameof(IPropertyOwnerExtensions.IsDefaulted)))
                    return !bag.ContainsKey(propertyName.Substring(0, propertyName.Length - nameof(IPropertyOwnerExtensions.IsDefaulted).Length));
                return bag.TryGetValue(propertyName, out object value) ? value : GetDefaultValue(propertyName);
            }
            set {
                if (!HasProperty(propertyName))
                    throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
                if (propertyName.EndsWith(nameof(IPropertyOwnerExtensions.IsDefaulted)))
                    throw IPropertyOwnerImplementations.CannotSetPropertyException(propertyName);
                if (value is null && propertyName == nameof(Item.Price))
                    throw IPropertyOwnerExtensions.CannotBeDefaultedException(nameof(Item.Price));
                // TODO: assert that value is of an assignable type - requires type info about properties, which we don't have, yet
                if (!bag.TryGetValue(propertyName, out object oldValue))
                    oldValue = GetDefaultValue(propertyName);
                this.Change(oldValue, value, () => {
                    if (value is null) 
                        bag.Remove(propertyName);
                    else
                        bag[propertyName] = value;
                }, propertyName);
            }
        }
        static object GetDefaultValue(string propertyName) {
            switch (propertyName) {
                case nameof(Item.Label): return LabelDefault;
                case nameof(Item.Cost): return CostDefault;
                case nameof(Item.Price): return default(decimal);
            }
            return null;
        }
    }
}