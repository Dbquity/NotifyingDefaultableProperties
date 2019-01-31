namespace Dbquity.Implementation {
    public interface IImplementPropertyOwner {
        void NotifyChanged(params string[] propertyNames);
        void NotifyChanging(params string[] propertyNames);
    }
}