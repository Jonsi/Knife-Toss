using R3;
using UnityEngine;

namespace Common.Events
{
    public abstract class ReactiveEventChannel<T> : ScriptableObject, IReactiveEventChannel<T>
    {
        private readonly Subject<T> _subject = new();

        public Observable<T> OnRaised => _subject;

        public void Raise(T data)
        {
            _subject.OnNext(data);
        }
    }
}
