using R3;
using UnityEngine;

namespace Common.Events
{
    [CreateAssetMenu(fileName = "VoidReactiveEventChannel", menuName = "Knife Toss/Events/Void Reactive Event Channel")]
    public class VoidReactiveEventChannel : ScriptableObject, IReactiveEventChannel<Unit>
    {
        private readonly Subject<Unit> _subject = new();

        public Observable<Unit> OnRaised => _subject;

        public void Raise()
        {
            _subject.OnNext(Unit.Default);
        }

        public void Raise(Unit data)
        {
            _subject.OnNext(data);
        }
    }
}
