using System;

namespace Dbquity {
    using Implementation;
    class Item : ItemBase {
        public override object this[string propertyName] {
            get {
                switch (propertyName) {
                    case nameof(Name): return Name;
                    case nameof(Label): return Label;
                    case nameof(LabelIsDefaulted): return LabelIsDefaulted;
                    case nameof(Cost): return Cost;
                    case nameof(CostIsDefaulted): return CostIsDefaulted;
                    case nameof(Price): return Price;
                }
                throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
            }
            set {
                switch (propertyName) {
                    case nameof(Name): Name = (string)value; return;
                    case nameof(Label): Label = (string)value; return;
                    case nameof(Cost): this.Change(cost, (decimal?)value, () => cost = (decimal?)value, propertyName); return;
                    case nameof(LabelIsDefaulted):
                    case nameof(CostIsDefaulted):
                        throw IPropertyOwnerImplementations.CannotSetPropertyException(propertyName);
                    case nameof(Price): Price = value is null ?
                            throw IPropertyOwnerExtensions.CannotBeDefaultedException(nameof(Price)) : (decimal)value;
                        return;
                }
                throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
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