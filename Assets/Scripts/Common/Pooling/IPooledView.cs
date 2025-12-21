namespace Common.Pooling
{
    public interface IPooledView
    {
        void OnSpawned();
        void OnDespawned();
    }
}
