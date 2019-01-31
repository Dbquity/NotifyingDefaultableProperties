using System.ComponentModel;

namespace Dbquity {
    class PropertyViewModel<T> : INotifyPropertyChanging, INotifyPropertyChanged {
        IPropertyOwner owner;
        string propertyName;
        public PropertyViewModel(IPropertyOwner owner, string propertyName) {
            string propertyIsDefaultName = propertyName + nameof(IsDefaulted);
            this.owner = owner;
            this.propertyName = propertyName;
            owner.PropertyChanging += (s, e) => {
                if (string.IsNullOrWhiteSpace(e.PropertyName))
                    PropertyChanging?.Invoke(s, e);
                else if (e.PropertyName == propertyName)
                    PropertyChanging?.Invoke(s, new PropertyChangingEventArgs(nameof(Value)));
                else if (e.PropertyName == propertyIsDefaultName)
                    PropertyChanging?.Invoke(s, new PropertyChangingEventArgs(nameof(IsDefaulted)));
            };
            owner.PropertyChanged += (s, e) => {
                if (string.IsNullOrWhiteSpace(e.PropertyName))
                    PropertyChanged?.Invoke(s, e);
                else if (e.PropertyName == propertyName)
                    PropertyChanged?.Invoke(s, new PropertyChangedEventArgs(nameof(Value)));
                else if (e.PropertyName == propertyIsDefaultName)
                    PropertyChanged?.Invoke(s, new PropertyChangedEventArgs(nameof(IsDefaulted)));
            };
        }
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        public T Value { get => (T)owner[propertyName]; set => owner[propertyName] = value; }
        public bool IsDefaulted => owner.IsDefaulted(propertyName);
        public void SetToDefault() => owner.SetToDefault(propertyName);
    }
}