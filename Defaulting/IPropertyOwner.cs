using System.ComponentModel;

namespace Dbquity {
    public interface IPropertyOwner : INotifyPropertyChanging, INotifyPropertyChanged, Implementation.IImplementPropertyOwner {
        object this[string propertyName] { get; set;}
        bool HasProperty(string propertyName);
    }
}