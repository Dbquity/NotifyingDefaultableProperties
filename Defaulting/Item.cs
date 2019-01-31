using System;
using System.ComponentModel;

namespace Dbquity {
    class Item : IPropertyOwner {
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        void IImplementPropertyOwner.NotifyChanging(params string[] propertyNames) =>
            this.NotifyChanging(PropertyChanging, propertyNames);
        void IImplementPropertyOwner.NotifyChanged(params string[] propertyNames) =>
            this.NotifyChanged(PropertyChanged, propertyNames);
        public object this[string propertyName] {
            get {
                switch (propertyName) {
                    case nameof(Name): return Name;
                    case nameof(Label): return Label;
                    case nameof(LabelIsDefaulted): return LabelIsDefaulted;
                    case nameof(Cost): return Cost;
                    case nameof(CostIsDefaulted): return CostIsDefaulted;
                }
                throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
            }
            set {
                switch (propertyName) {
                    case nameof(Name): Name = (string)value; return;
                    case nameof(Label): Label = (string)value; return;
                    case nameof(Cost): this.Change(cost, (decimal?)value, () => cost = (decimal?)value, propertyName); return;
                    case nameof(LabelIsDefaulted):
                    case nameof(CostIsDefaulted): throw new ArgumentOutOfRangeException($"Cannot set: '{propertyName}'.");
                }
                throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
            }
        }
        public bool CanBeDefaulted(string propertyName) {
            switch (propertyName) {
                case nameof(Label):
                case nameof(Cost): return true;
                case nameof(Name):
                case nameof(LabelIsDefaulted):
                case nameof(CostIsDefaulted): return false;
            }
            throw IPropertyOwnerImplementations.UnknownPropertyException(propertyName);
        }
        public const string LabelDefault = "<label it>";
        public string Label { get => label ?? LabelDefault; set => this.Change(label, value, () => label = value); } string label;
        public bool LabelIsDefaulted => label is null;
        public const Decimal CostDefault = 43L;
        public Decimal Cost { get => cost ?? CostDefault; set => this.Change(cost, value, () => cost = value); } Decimal? cost;
        public bool CostIsDefaulted => !cost.HasValue;
        public string Name { get => name; set => this.Change(name, value, () => name = value); } string name;
    }
}