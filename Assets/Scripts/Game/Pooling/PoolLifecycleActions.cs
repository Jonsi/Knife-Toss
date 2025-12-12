using System;

namespace Game.Pooling
{
    /// <summary>
    /// Encapsulates lifecycle actions used when working with Unity ObjectPool instances.
    /// </summary>
    /// <typeparam name="T">Type managed by the pool.</typeparam>
    public class PoolLifecycleActions<T> where T : class
    {
        /// <summary>
        /// Initializes lifecycle actions for pool operations.
        /// </summary>
        /// <param name="onGet">Action invoked when an instance is retrieved from the pool.</param>
        /// <param name="onRelease">Action invoked when an instance is returned to the pool.</param>
        public PoolLifecycleActions(Action<T> onGet, Action<T> onRelease)
        {
            OnGet = onGet;
            OnRelease = onRelease;
        }

        /// <summary>
        /// Action executed on pool retrieval.
        /// </summary>
        public Action<T> OnGet { get; }

        /// <summary>
        /// Action executed on pool release.
        /// </summary>
        public Action<T> OnRelease { get; }
    }
}
