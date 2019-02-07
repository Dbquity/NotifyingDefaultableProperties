using Dbquity.Implementation;
using System;

namespace Defaulting.TestItems {
    class Item : ItemBase {
        public override object this[string propertyName] {
            get => this.TryReflectionGet(propertyName, out object value) is Exception ex ? throw ex : value;
            set {
                if (value is null && propertyName == nameof(Cost))
                    this.Change(cost, null, () => cost = null, nameof(Cost));
                else if (this.TryReflectionSet(propertyName, value) is Exception ex)
                    throw ex;
            }
        }
        public string Label { get => label ?? LabelDefault; set => this.Change(label, value, () => label = value); } string label;
        public bool LabelIsDefaulted => label is null;
        public Decimal Cost { get => cost ?? CostDefault; set => this.Change(cost, value, () => cost = value); } Decimal? cost;
        public bool CostIsDefaulted => !cost.HasValue;
        public string Name { get => name; set => this.Change(name, value, () => name = value); } string name;
        public Decimal Price { get => price; set => this.Change(price, value, () => price = value); } decimal price;
    }
}