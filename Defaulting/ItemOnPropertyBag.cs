using Dbquity.Implementation;
using System;
using System.Collections.Generic;

namespace Defaulting.TestItems {
    using static PropertyOwnerImplementations;
    class ItemOnPropertyBag : ItemBase {
        Dictionary<string, object> bag = new Dictionary<string, object>();
        public override object this[string propertyName] {
            get => this.TryDictionaryGet(bag, () => GetDefault(propertyName), propertyName, out object value) is Exception ex ?
                throw ex : value;
            set {
                if (this.TryDictionarySet(bag, () => GetDefault(propertyName), propertyName, value) is Exception ex)
                    throw ex;
            }
        }
        static object GetDefault(string propertyName) {
            switch (propertyName) {
                case nameof(Item.Label): return LabelDefault;
                case nameof(Item.Cost): return CostDefault;
                case nameof(Item.Price): return default(decimal); // TODO: provide type instead and let implementations get default
            }
            return null;
        }
    }
}