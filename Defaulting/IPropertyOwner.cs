using System.ComponentModel;

namespace Dbquity {
    public interface IPropertyOwner : INotifyPropertyChanging, INotifyPropertyChanged, IImplementPropertyOwner {
        object this[string propertyName] { get; set;}
        bool CanBeDefaulted(string propertyName);
    }
    public interface IImplementPropertyOwner {
        void NotifyChanged(params string[] propertyNames);
        void NotifyChanging(params string[] propertyNames);
    }
}