using System;
using System.Collections.Generic;
using Game.Entities.Target.Feedback;
using Game.Feedback.Animation.Data;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Feedback.Animation
{
    /// <summary>
    /// Drives the runtime behavior of spawned sprite shards.
    /// </summary>
    public class SpriteShatterSequence : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly List<ShardInstance> _ActiveShards = new();

        private SpriteShatterAnimationConfig _Config;
        private Material _ShardMaterial;
        private bool _IsCompleted;

        /// <summary>
        /// Event raised when the shatter sequence has completed.
        /// </summary>
        public event Action<SpriteShatterSequence> Completed;

        /// <summary>
        /// Creates and initializes a shatter sequence for the provided sprite renderer.
        /// </summary>
        /// <returns>The created sequence instance or null if initialization failed.</returns>
        public static SpriteShatterSequence Create(SpriteRenderer sourceRenderer, SpriteShatterAnimationConfig config, Material shardMaterial)
        {
            if (sourceRenderer == null || config == null || shardMaterial == null)
                return null;

            Sprite sprite = sourceRenderer.sprite;
            if (sprite == null)
                return null;

            var host = new GameObject("SpriteShatterSequence");
            Transform sourceTransform = sourceRenderer.transform;
            host.transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
            host.transform.localScale = sourceTransform.lossyScale;

            SpriteShatterSequence sequence = host.AddComponent<SpriteShatterSequence>();
            if (!sequence.Initialize(sprite, sourceRenderer, config, shardMaterial))
            {
                UnityEngine.Object.Destroy(host);
                return null;
            }

            return sequence;
        }

        private bool Initialize(Sprite sprite, SpriteRenderer sourceRenderer, SpriteShatterAnimationConfig config, Material shardMaterial)
        {
            _Config = config;
            _ShardMaterial = shardMaterial;
            Texture spriteTexture = sprite.texture;
            if (spriteTexture == null)
                return false;

            int shardCount = GetShardCount();
            IReadOnlyList<Mesh> meshes = SpriteShardMeshBuilder.Build(sprite, shardCount, _Config);
            if (meshes.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < meshes.Count; i++)
            {
                SpawnShard(meshes[i], sourceRenderer, spriteTexture);
            }

            return true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = _ActiveShards.Count - 1; i >= 0; i--)
            {
                ShardInstance shard = _ActiveShards[i];
                shard.Age += deltaTime;

                if (shard.Age >= _Config.Lifetime)
                {
                    shard.Dispose();
                    _ActiveShards.RemoveAt(i);
                    continue;
                }

                UpdateShardMovement(shard, deltaTime);
                UpdateShardColor(shard);
            }

            if (_ActiveShards.Count == 0)
                CompleteSequence(true);
        }

        private void UpdateShardMovement(ShardInstance shard, float deltaTime)
        {
            Vector3 velocity = shard.Velocity;
            velocity.y += _Config.Gravity * deltaTime;
            if (_Config.Drag > 0f)
            {
                float dragFactor = Mathf.Clamp01(_Config.Drag * deltaTime);
                velocity = Vector3.Lerp(velocity, Vector3.zero, dragFactor);
            }

            shard.Velocity = velocity;
            shard.Transform.position += velocity * deltaTime;
        }

        private void UpdateShardColor(ShardInstance shard)
        {
            if (shard.Age < _Config.FadeDelay)
                return;

            float normalized = Mathf.Clamp01((shard.Age - _Config.FadeDelay) / _Config.FadeDuration);
            Color blended = Color.Lerp(shard.StartColor, shard.EndColor, normalized);
            shard.PropertyBlock.SetColor(ColorId, blended);
            shard.Renderer.SetPropertyBlock(shard.PropertyBlock);
        }

        private void SpawnShard(Mesh mesh, SpriteRenderer sourceRenderer, Texture spriteTexture)
        {
            var shardObject = new GameObject("SpriteShardPiece");
            shardObject.transform.SetParent(transform, false);
            shardObject.transform.localPosition = new Vector3(RandomJitter(_Config.SpawnJitter.x), RandomJitter(_Config.SpawnJitter.y), 0f);
            shardObject.transform.localRotation = Quaternion.identity;
            shardObject.transform.localScale = Vector3.one;

            MeshFilter filter = shardObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = shardObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _ShardMaterial;
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            renderer.sortingOrder = sourceRenderer.sortingOrder;

            var shard = new ShardInstance(shardObject.transform, mesh, renderer)
            {
                Velocity = CalculateInitialVelocity(),
                StartColor = sourceRenderer.color,
                EndColor = _Config.TargetColor
            };
            shard.PropertyBlock.SetColor(ColorId, shard.StartColor);
            shard.PropertyBlock.SetTexture("_MainTex", spriteTexture);
            renderer.SetPropertyBlock(shard.PropertyBlock);

            _ActiveShards.Add(shard);
        }

        private static float RandomJitter(float amplitude)
        {
            float value = Mathf.Abs(amplitude);
            return Random.Range(-value, value);
        }

        private void OnDestroy()
        {
            CompleteSequence(false);
            for (int i = 0; i < _ActiveShards.Count; i++)
            {
                _ActiveShards[i].Dispose();
            }
            _ActiveShards.Clear();
        }

        private void CompleteSequence(bool requestDestroy)
        {
            if (_IsCompleted)
                return;

            _IsCompleted = true;
            Completed?.Invoke(this);
            Completed = null;

            if (requestDestroy)
                Destroy(gameObject);
        }

        private int GetShardCount()
        {
            int min = Mathf.Max(1, _Config.ShardCountRange.x);
            int max = Mathf.Max(min, _Config.ShardCountRange.y);
            return Random.Range(min, max + 1);
        }

        private Vector3 CalculateInitialVelocity()
        {
            float horizontal = RandomRange(_Config.HorizontalForceRange);
            float vertical = RandomRange(_Config.VerticalForceRange);

            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                vertical = 0.1f;
            }

            return new Vector3(horizontal, vertical, 0f);
        }

        private static float RandomRange(Vector2 range)
        {
            float min = Mathf.Min(range.x, range.y);
            float max = Mathf.Max(range.x, range.y);
            return Random.Range(min, max);
        }

        private class ShardInstance
        {
            public readonly Transform Transform;
            public readonly Mesh Mesh;
            public readonly MeshRenderer Renderer;
            public readonly MaterialPropertyBlock PropertyBlock;

            public Vector3 Velocity;
            public float Age;
            public Color StartColor;
            public Color EndColor;
            private bool _IsDisposed;

            public ShardInstance(Transform transform, Mesh mesh, MeshRenderer renderer)
            {
                Transform = transform;
                Renderer = renderer;
                Mesh = mesh;
                PropertyBlock = new MaterialPropertyBlock();
            }

            public void Dispose()
            {
                if (_IsDisposed)
                    return;

                if (Transform != null)
                    UnityObject.Destroy(Transform.gameObject);
                if (Mesh != null)
                    UnityObject.Destroy(Mesh);

                _IsDisposed = true;
            }
        }
    }
}
