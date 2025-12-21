using R3;

namespace Common.Events
{
    public interface IReactiveEventChannel<T>
    {
        Observable<T> OnRaised { get; }
        void Raise(T data);
    }
}
