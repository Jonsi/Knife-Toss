using System;
using R3;

namespace Common.Events
{
    public static class EventChannelExtensions
    {
        public static Observable<T> AsObservable<T>(this IEventChannel<T> channel)
        {
            return Observable.Create<T>(observer =>
            {
                Action<T> handler = data => observer.OnNext(data);
                channel.Subscribe(handler);
                return Disposable.Create(() => channel.Unsubscribe(handler));
            });
        }

        public static Observable<Unit> AsObservable(this IEventChannel channel)
        {
            return Observable.Create<Unit>(observer =>
            {
                Action handler = () => observer.OnNext(Unit.Default);
                channel.Subscribe(handler);
                return Disposable.Create(() => channel.Unsubscribe(handler));
            });
        }
    }
}
