using System.Collections.Generic;
using DG.Tweening;
using KnifeToss.Presentation.Gameplay.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KnifeToss.Presentation.Gameplay
{
    public class KnifeThrower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject knifePrefab;
        [SerializeField] private Transform launcherTransform;
        [SerializeField] private InputActionAsset inputActionAsset;

        [Header("Throwing Settings")]
        [SerializeField] private float throwSpeed = 10f;

        [Header("Magazine Settings")]
        [SerializeField] private int poolWarmupCount = 10;
        [SerializeField] private int visibleQueueCount = 5;
        [SerializeField] private float queueSpacing = 0.5f;
        [SerializeField] private float slideAnimationDuration = 0.3f;
        [SerializeField] private float queueScaleEnd = 0.5f;
        [SerializeField] private float queueOpacityEnd = 0.3f;

        private InputAction attackAction;
        private Knife currentKnife;
        private bool isKnifeInFlight;
        private int maxKnives;
        private int knivesThrown;
        private bool canThrow = true;
        private Transform knivesParent;

        private Queue<GameObject> knifePool;
        private List<GameObject> activeQueue;
        private int totalKnivesToCreate;
        private int knivesRemaining;
        private Vector3 prefabOriginalScale;
        private Color prefabOriginalColor;

        public event System.Action KnifeThrown;

        private void Awake()
        {
            if (inputActionAsset != null)
            {
                attackAction = inputActionAsset.FindAction("Player/Attack");
            }

            if (launcherTransform == null)
            {
                launcherTransform = transform;
            }

            knifePool = new Queue<GameObject>();
            activeQueue = new List<GameObject>();
        }

        private void OnEnable()
        {
            if (attackAction != null)
            {
                attackAction.Enable();
                attackAction.performed += OnThrowInput;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.performed -= OnThrowInput;
                attackAction.Disable();
            }
        }

        private void OnDestroy()
        {
            // Kill all tweens before destroying
            KillAllTweens();
            KillTweensOnChildren(transform);
            ClearPool();
        }

        /// <summary>
        /// Public method to kill all tweens - call this BEFORE ResetHierarchy to prevent DOTween errors
        /// </summary>
        public void KillAllKnifeTweens()
        {
            KillTweensOnAllKnivesInScene();
            KillAllTweens();
            KillTweensOnChildren(transform);
            if (knivesParent != null)
            {
                KillTweensOnChildren(knivesParent);
            }
        }

        private void OnThrowInput(InputAction.CallbackContext context)
        {
            if (!canThrow || isKnifeInFlight || knifePrefab == null || launcherTransform == null)
            {
                return;
            }

            if (maxKnives > 0 && knivesThrown >= maxKnives)
            {
                return;
            }

            if (knivesRemaining <= 0)
            {
                return;
            }

            ThrowKnife();
        }

        private void ThrowKnife()
        {
            if (activeQueue.Count == 0)
            {
                return;
            }

            // Set flag immediately to prevent double-throwing
            isKnifeInFlight = true;

            GameObject knifeObject = activeQueue[0];
            activeQueue.RemoveAt(0);

            // Remove from thrower hierarchy
            if (knivesParent != null)
            {
                knifeObject.transform.SetParent(knivesParent);
            }
            else
            {
                knifeObject.transform.SetParent(null);
            }

            // Position at launcher
            knifeObject.transform.position = launcherTransform.position;
            knifeObject.transform.rotation = launcherTransform.rotation;

            // Reset scale to prefab original scale
            knifeObject.transform.localScale = prefabOriginalScale;

            // Reset opacity to full
            SpriteRenderer spriteRenderer = knifeObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = prefabOriginalColor;
            }

            // Enable colliders for the thrown knife
            Collider2D[] colliders = knifeObject.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            // Enable and initialize
            knifeObject.SetActive(true);
            currentKnife = knifeObject.GetComponent<Knife>();

            if (currentKnife != null)
            {
                currentKnife.Initialize(this, throwSpeed);
                knivesThrown++;
                knivesRemaining--;
                KnifeThrown?.Invoke();
            }

            // Slide queue forward
            SlideQueueForward();
        }

        private void SlideQueueForward()
        {
            // Animate remaining knives sliding forward
            for (int i = 0; i < activeQueue.Count; i++)
            {
                GameObject knife = activeQueue[i];
                if (knife == null || knife.transform == null) continue;

                // Kill any existing tweens on this knife (complete immediately)
                DOTween.Kill(knife, true);
                DOTween.Kill(knife.transform, true);
                knife.transform.DOKill(true);
                
                SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    DOTween.Kill(spriteRenderer, true);
                    spriteRenderer.DOKill(true);
                }

                Vector3 targetPosition = GetQueuePosition(i);
                float scaleMultiplier = GetQueueScaleMultiplier(i);
                Vector3 targetScaleVector = prefabOriginalScale * scaleMultiplier;
                float opacityMultiplier = GetQueueOpacityMultiplier(i);
                Color targetColor = new Color(prefabOriginalColor.r, prefabOriginalColor.g, prefabOriginalColor.b, prefabOriginalColor.a * opacityMultiplier);

                knife.transform.DOMove(targetPosition, slideAnimationDuration).SetEase(Ease.OutQuad);
                knife.transform.DOScale(targetScaleVector, slideAnimationDuration).SetEase(Ease.OutQuad);

                if (spriteRenderer != null)
                {
                    spriteRenderer.DOColor(targetColor, slideAnimationDuration).SetEase(Ease.OutQuad);
                }
            }

            // Always maintain visibleQueueCount + 1 knives in queue (the extra one is invisible for smooth tweening)
            int knivesNeededInQueue = Mathf.Min(visibleQueueCount + 1, knivesRemaining);
            while (activeQueue.Count < knivesNeededInQueue && (knifePool.Count > 0 || CanCreateMoreKnives()))
            {
                if (knifePool.Count == 0)
                {
                    CreateAdditionalKnife();
                }
                AddKnifeToQueue();
            }

            UpdateQueueVisibility();
        }

        private void AddKnifeToQueue()
        {
            if (knifePool.Count == 0)
            {
                return;
            }

            GameObject knife = knifePool.Dequeue();
            if (knife == null || knife.transform == null)
            {
                return;
            }

            int queueIndex = activeQueue.Count;

            // Kill any existing tweens on this knife (complete immediately)
            DOTween.Kill(knife, true);
            DOTween.Kill(knife.transform, true);
            knife.transform.DOKill(true);
            
            SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                DOTween.Kill(spriteRenderer, true);
                spriteRenderer.DOKill(true);
            }

            // Position, scale, and opacity
            Vector3 targetPosition = GetQueuePosition(queueIndex);
            float scaleMultiplier = GetQueueScaleMultiplier(queueIndex);
            float opacityMultiplier = GetQueueOpacityMultiplier(queueIndex);
            Color targetColor = new Color(prefabOriginalColor.r, prefabOriginalColor.g, prefabOriginalColor.b, prefabOriginalColor.a * opacityMultiplier);

            knife.transform.position = targetPosition;
            knife.transform.localScale = prefabOriginalScale * scaleMultiplier;
            knife.transform.rotation = launcherTransform.rotation;
            knife.transform.SetParent(transform);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = targetColor;
            }

            // Disable colliders for knives in the queue
            Collider2D[] colliders = knife.GetComponents<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }

            knife.SetActive(true);

            activeQueue.Add(knife);
        }

        private bool CanCreateMoreKnives()
        {
            // Check if we can create more knives (total created < total needed)
            int totalCreated = knifePool.Count + activeQueue.Count;
            return totalCreated < totalKnivesToCreate;
        }

        private void CreateAdditionalKnife()
        {
            if (!CanCreateMoreKnives())
            {
                return;
            }

            GameObject knife = Instantiate(knifePrefab, transform);
            knife.SetActive(false);
            knifePool.Enqueue(knife);
        }

        private Vector3 GetQueuePosition(int queueIndex)
        {
            // Queue extends to the left (negative X) from launcher position
            // Index 0 is at launcher position (ready to throw)
            // Index 1+ are queued to the left (left-to-right order)
            Vector3 basePosition = launcherTransform.position;
            float offsetX = -queueIndex * queueSpacing;
            return basePosition + Vector3.right * offsetX;
        }

        private float GetQueueScaleMultiplier(int queueIndex)
        {
            // Returns a multiplier (0-1) for scaling knives in the queue
            // Use prefab's original scale as the base
            if (visibleQueueCount <= 1)
            {
                return 1f;
            }

            float t = Mathf.Clamp01((float)queueIndex / (visibleQueueCount - 1));
            return Mathf.Lerp(1f, queueScaleEnd, t);
        }

        private float GetQueueOpacityMultiplier(int queueIndex)
        {
            // Returns a multiplier (0-1) for opacity of knives in the queue
            // Similar to scale, opacity decreases as knives go further back
            // The knife at visibleQueueCount index should be completely invisible (0 opacity)
            if (queueIndex >= visibleQueueCount)
            {
                return 0f; // Completely invisible for the extra knife
            }

            if (visibleQueueCount <= 1)
            {
                return 1f;
            }

            float t = Mathf.Clamp01((float)queueIndex / (visibleQueueCount - 1));
            return Mathf.Lerp(1f, queueOpacityEnd, t);
        }

        private void UpdateQueueVisibility()
        {
            // Keep all knives active, but set opacity appropriately
            // The extra knife (at visibleQueueCount) will be invisible (opacity 0)
            for (int i = 0; i < activeQueue.Count; i++)
            {
                GameObject knife = activeQueue[i];
                if (knife == null || knife.transform == null) continue;

                // Keep all knives active for smooth tweening
                knife.SetActive(true);

                // Update opacity based on position
                float opacityMultiplier = GetQueueOpacityMultiplier(i);
                Color targetColor = new Color(prefabOriginalColor.r, prefabOriginalColor.g, prefabOriginalColor.b, prefabOriginalColor.a * opacityMultiplier);

                SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = targetColor;
                }
            }
        }

        private void CreateKnifePool(int totalKnives)
        {
            // Kill all tweens and clean up null entries first
            CleanupNullEntries();
            KillAllTweens();

            // Return all active queue knives to pool first
            while (activeQueue.Count > 0)
            {
                GameObject knife = activeQueue[0];
                activeQueue.RemoveAt(0);
                if (knife != null)
                {
                    ReturnKnifeToPool(knife);
                }
            }

            // Store prefab's original scale and color
            if (knifePrefab != null)
            {
                prefabOriginalScale = knifePrefab.transform.localScale;

                SpriteRenderer prefabRenderer = knifePrefab.GetComponent<SpriteRenderer>();
                if (prefabRenderer != null)
                {
                    prefabOriginalColor = prefabRenderer.color;
                }
                else
                {
                    prefabOriginalColor = Color.white;
                }
            }
            else
            {
                prefabOriginalScale = Vector3.one;
                prefabOriginalColor = Color.white;
            }

            totalKnivesToCreate = totalKnives;
            knivesRemaining = totalKnives;

            // Clean up null entries from pool
            CleanupNullEntries();

            // Only create new knives if we don't have enough in the pool
            int knivesNeeded = Mathf.Min(poolWarmupCount, totalKnives);
            int knivesToCreate = knivesNeeded - knifePool.Count;
            
            for (int i = 0; i < knivesToCreate; i++)
            {
                GameObject knife = Instantiate(knifePrefab, transform);
                knife.SetActive(false);
                
                // Disable colliders
                Collider2D[] colliders = knife.GetComponents<Collider2D>();
                foreach (Collider2D collider in colliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }
                }
                
                knifePool.Enqueue(knife);
            }

            // Initialize queue with visibleQueueCount + 1 knives (extra one is invisible for smooth tweening)
            int initialQueueSize = Mathf.Min(visibleQueueCount + 1, totalKnives);
            for (int i = 0; i < initialQueueSize; i++)
            {
                AddKnifeToQueue();
            }

            UpdateQueueVisibility();
        }

        private void CleanupNullEntries()
        {
            // Remove null entries from active queue
            for (int i = activeQueue.Count - 1; i >= 0; i--)
            {
                if (activeQueue[i] == null)
                {
                    activeQueue.RemoveAt(i);
                }
            }

            // Remove null entries from pool
            Queue<GameObject> cleanedPool = new Queue<GameObject>();
            while (knifePool.Count > 0)
            {
                GameObject knife = knifePool.Dequeue();
                if (knife != null)
                {
                    cleanedPool.Enqueue(knife);
                }
            }
            knifePool = cleanedPool;
        }

        private void KillAllTweens()
        {
            // Kill tweens on all queue knives
            foreach (GameObject knife in activeQueue)
            {
                if (knife != null)
                {
                    // Kill by GameObject first (more aggressive, complete immediately)
                    DOTween.Kill(knife, true);
                    
                    if (knife.transform != null)
                    {
                        DOTween.Kill(knife.transform, true);
                        knife.transform.DOKill(true);
                    }
                    
                    SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        DOTween.Kill(spriteRenderer, true);
                        spriteRenderer.DOKill(true);
                    }
                }
            }

            // Kill tweens on all pooled knives
            foreach (GameObject knife in knifePool)
            {
                if (knife != null)
                {
                    // Kill by GameObject first (more aggressive, complete immediately)
                    DOTween.Kill(knife, true);
                    
                    if (knife.transform != null)
                    {
                        DOTween.Kill(knife.transform, true);
                        knife.transform.DOKill(true);
                    }
                    
                    SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        DOTween.Kill(spriteRenderer, true);
                        spriteRenderer.DOKill(true);
                    }
                }
            }

            // Kill tweens on all knives that are children of this transform (in case they're still here)
            KillTweensOnChildren(transform);

            // Kill tweens on current knife if it exists
            if (currentKnife != null && currentKnife.gameObject != null)
            {
                DOTween.Kill(currentKnife.gameObject, true);
                
                if (currentKnife.transform != null)
                {
                    DOTween.Kill(currentKnife.transform, true);
                    currentKnife.transform.DOKill(true);
                }
                
                SpriteRenderer spriteRenderer = currentKnife.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    DOTween.Kill(spriteRenderer, true);
                    spriteRenderer.DOKill(true);
                }
            }
        }

        private void KillTweensOnAllKnivesInScene()
        {
            // Find all Knife components in the scene and kill their tweens
            // This catches knives that might be destroyed by hierarchy manager
            Knife[] allKnives = FindObjectsByType<Knife>(FindObjectsSortMode.None);
            foreach (Knife knife in allKnives)
            {
                if (knife != null && knife.gameObject != null)
                {
                    // Kill all tweens targeting this GameObject and all its components
                    DOTween.Kill(knife.gameObject, true); // Complete = true to finish immediately
                    
                    // Also kill tweens on transform
                    if (knife.transform != null)
                    {
                        DOTween.Kill(knife.transform, true);
                        knife.transform.DOKill(true);
                    }
                    
                    // Kill tweens on SpriteRenderer specifically
                    SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        DOTween.Kill(spriteRenderer, true);
                        spriteRenderer.DOKill(true);
                    }
                }
            }
        }

        private void KillTweensOnChildren(Transform parent)
        {
            if (parent == null) return;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child != null)
                {
                    // Kill tweens on the GameObject itself first
                    if (child.gameObject != null)
                    {
                        DOTween.Kill(child.gameObject, true);
                    }
                    
                    // Kill tweens on the transform
                    DOTween.Kill(child, true);
                    child.DOKill(true);
                    
                    SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        DOTween.Kill(spriteRenderer, true);
                        spriteRenderer.DOKill(true);
                    }
                }
            }
        }

        private void ReturnKnifeToPool(GameObject knife)
        {
            if (knife == null) return;

            // Kill any active tweens on this knife (complete immediately)
            DOTween.Kill(knife, true);
            
            if (knife.transform != null)
            {
                DOTween.Kill(knife.transform, true);
                knife.transform.DOKill(true);
            }
            
            SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                DOTween.Kill(spriteRenderer, true);
                spriteRenderer.DOKill(true);
            }

            // Reset knife state (only if not destroyed)
            if (knife != null)
            {
                knife.SetActive(false);
                
                if (knife.transform != null)
                {
                    knife.transform.SetParent(transform);
                    
                    // Reset scale and color
                    knife.transform.localScale = prefabOriginalScale;
                }
                
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = prefabOriginalColor;
                }

                // Disable colliders
                Collider2D[] colliders = knife.GetComponents<Collider2D>();
                foreach (Collider2D collider in colliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }
                }

                knifePool.Enqueue(knife);
            }
        }

        private void ClearPool()
        {
            // Return all active queue knives to pool
            while (activeQueue.Count > 0)
            {
                GameObject knife = activeQueue[0];
                activeQueue.RemoveAt(0);
                ReturnKnifeToPool(knife);
            }

            // Don't destroy pooled knives - keep them for reuse
            // The pool will be reused when CreateKnifePool is called again
            activeQueue.Clear();
        }

        public void OnKnifeResolved()
        {
            isKnifeInFlight = false;
            
            // Note: The thrown knife is now parented to knivesParent or target,
            // so we don't return it to the pool. It will be cleaned up by the hierarchy manager.
            currentKnife = null;
        }

        public void ResetState()
        {
            // CRITICAL: Kill ALL tweens immediately - this must happen FIRST
            // Use DOTween's ability to kill all tweens targeting specific objects
            // even if they're about to be destroyed
            
            // Kill tweens on ALL knives in the scene first
            KillTweensOnAllKnivesInScene();
            
            // Kill tweens on all tracked knives
            KillAllTweens();

            // Also kill tweens on knives that might be in the knivesParent hierarchy
            if (knivesParent != null)
            {
                KillTweensOnChildren(knivesParent);
            }
            
            // Kill tweens on all children of this transform (our pooled knives)
            KillTweensOnChildren(transform);

            // Kill any active tweens on queue knives (check for null first, complete immediately)
            for (int i = activeQueue.Count - 1; i >= 0; i--)
            {
                GameObject knife = activeQueue[i];
                if (knife == null)
                {
                    activeQueue.RemoveAt(i);
                    continue;
                }

                // Kill tweens even if knife is about to be destroyed
                DOTween.Kill(knife, true);
                
                if (knife.transform != null)
                {
                    DOTween.Kill(knife.transform, true);
                    knife.transform.DOKill(true);
                }
                
                SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    DOTween.Kill(spriteRenderer, true);
                    spriteRenderer.DOKill(true);
                }
            }

            // Kill tweens on all pooled knives too
            foreach (GameObject knife in knifePool)
            {
                if (knife != null)
                {
                    DOTween.Kill(knife, true);
                    
                    if (knife.transform != null)
                    {
                        DOTween.Kill(knife.transform, true);
                        knife.transform.DOKill(true);
                    }
                    
                    SpriteRenderer spriteRenderer = knife.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        DOTween.Kill(spriteRenderer, true);
                        spriteRenderer.DOKill(true);
                    }
                }
            }

            // Return all queue knives to pool (only if they still exist)
            while (activeQueue.Count > 0)
            {
                GameObject knife = activeQueue[0];
                activeQueue.RemoveAt(0);
                
                // Only return to pool if knife still exists and is still a child of this transform
                if (knife != null && knife.transform != null && knife.transform.parent == transform)
                {
                    ReturnKnifeToPool(knife);
                }
            }

            // Clear current knife reference (it may already be destroyed or parented elsewhere)
            currentKnife = null;

            isKnifeInFlight = false;
            knivesThrown = 0;
            canThrow = true;
        }

        public void SetMaxKnives(int max)
        {
            maxKnives = max;
            knivesThrown = 0;
        }

        public void SetTotalKnives(int total)
        {
            CreateKnifePool(total);
        }

        public void SetCanThrow(bool allow)
        {
            canThrow = allow;
        }

        public void SetKnivesParent(Transform parent)
        {
            knivesParent = parent;
        }
    }
}

