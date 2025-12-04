using System;
using UnityEngine;

namespace KnifeToss.Infrastructure.Services
{
    /// <summary>
    /// Caches frequently accessed Unity objects to avoid repeated expensive lookups.
    /// Replaces Camera.main calls and other FindObject* patterns.
    /// </summary>
    public sealed class RuntimeCacheService : IDisposable
    {
        private Camera _mainCamera;
        private Transform _mainCameraTransform;
        private AudioListener _audioListener;

        /// <summary>
        /// Gets the main camera, caching it on first access.
        /// </summary>
        public Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                {
                    _mainCamera = Camera.main;
                    if (_mainCamera != null)
                    {
                        _mainCameraTransform = _mainCamera.transform;
                    }
                }
                return _mainCamera;
            }
        }

        /// <summary>
        /// Gets the main camera's transform, caching it on first access.
        /// </summary>
        public Transform MainCameraTransform
        {
            get
            {
                if (_mainCameraTransform == null && MainCamera != null)
                {
                    _mainCameraTransform = MainCamera.transform;
                }
                return _mainCameraTransform;
            }
        }

        /// <summary>
        /// Gets the AudioListener, caching it on first access.
        /// </summary>
        public AudioListener AudioListener
        {
            get
            {
                if (_audioListener == null)
                {
                    _audioListener = UnityEngine.Object.FindFirstObjectByType<AudioListener>();
                }
                return _audioListener;
            }
        }

        /// <summary>
        /// Invalidates all cached references. Call this when scenes change or objects are destroyed.
        /// </summary>
        public void InvalidateCache()
        {
            _mainCamera = null;
            _mainCameraTransform = null;
            _audioListener = null;
        }

        /// <summary>
        /// Invalidates only the camera cache.
        /// </summary>
        public void InvalidateCameraCache()
        {
            _mainCamera = null;
            _mainCameraTransform = null;
        }

        public void Dispose()
        {
            InvalidateCache();
        }
    }
}

