using System.Collections.Generic;
using Game.Feedback.Animation.Data;
using UnityEngine;

namespace Game.Entities.Target.Feedback
{
    /// <summary>
    /// Builds shard meshes by generating radial or linear slices based on sprite proportions.
    /// </summary>
    internal static class SpriteShardMeshBuilder
    {
        private const float MaxSegmentAngle = 12f;
        private const float AspectThreshold = 1.3f;

        public static List<Mesh> Build(Sprite sprite, int shardCount, SpriteShatterAnimationConfig config)
        {
            var meshes = new List<Mesh>();
            if (sprite == null || sprite.texture == null || config == null)
                return meshes;

            Bounds bounds = sprite.bounds;
            bool useRadial = ShouldUseRadial(bounds);

            if (useRadial)
            {
                BuildRadial(sprite, shardCount, config.SliceAngleRange, meshes);
            }
            else
            {
                BuildLinear(sprite, shardCount, config.LinearSliceSizeRange, meshes);
            }

            return meshes;
        }

        private static bool ShouldUseRadial(Bounds bounds)
        {
            float width = Mathf.Max(bounds.size.x, 0.0001f);
            float height = Mathf.Max(bounds.size.y, 0.0001f);
            float aspect = width / height;
            return aspect >= 1f / AspectThreshold && aspect <= AspectThreshold;
        }

        private static void BuildRadial(Sprite sprite, int shardCount, Vector2 sliceAngleRange, List<Mesh> meshes)
        {
            int targetShardCount = Mathf.Clamp(shardCount, 1, 32);
            IReadOnlyList<SliceDefinition> slices = SliceDefinition.Create(targetShardCount, sliceAngleRange);

            Bounds bounds = sprite.bounds;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            Texture texture = sprite.texture;
            Rect textureRect = sprite.textureRect;

            for (int i = 0; i < slices.Count; i++)
            {
                Mesh mesh = CreateSliceMesh(bounds, textureRect, texture.width, texture.height, radius, slices[i]);
                if (mesh != null)
                    meshes.Add(mesh);
            }
        }

        private static void BuildLinear(Sprite sprite, int shardCount, Vector2 sliceSizeRange, List<Mesh> meshes)
        {
            Bounds bounds = sprite.bounds;
            Texture texture = sprite.texture;
            Rect textureRect = sprite.textureRect;
            bool horizontal = bounds.size.x >= bounds.size.y;
            float totalLength = horizontal ? bounds.size.x : bounds.size.y;

            int targetShardCount = Mathf.Clamp(shardCount, 1, 32);
            IReadOnlyList<LinearSliceDefinition> slices = LinearSliceDefinition.Create(targetShardCount, totalLength, sliceSizeRange);
            if (slices.Count == 0)
                return;

            for (int i = 0; i < slices.Count; i++)
            {
                Mesh mesh = CreateLinearSliceMesh(bounds, textureRect, texture.width, texture.height, slices[i], horizontal);
                if (mesh != null)
                    meshes.Add(mesh);
            }
        }

        private static Mesh CreateSliceMesh(
            Bounds bounds,
            Rect textureRect,
            float textureWidth,
            float textureHeight,
            float radius,
            SliceDefinition slice)
        {
            int segments = Mathf.Max(1, Mathf.CeilToInt(slice.Angle / MaxSegmentAngle));
            var vertices = new List<Vector3>(segments + 2);
            var uvs = new List<Vector2>(segments + 2);
            var triangles = new List<int>(segments * 3);

            Vector3 center = bounds.center;
            vertices.Add(center);
            uvs.Add(CalculateUV(center, bounds, textureRect, textureWidth, textureHeight));

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = slice.StartAngle + slice.Angle * t;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 point = new Vector3(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius, 0f) + center;

                vertices.Add(point);
                uvs.Add(CalculateUV(point, bounds, textureRect, textureWidth, textureHeight));
            }

            for (int i = 1; i <= segments; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            var mesh = new Mesh
            {
                name = "SpriteShardRadial"
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0, true);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Mesh CreateLinearSliceMesh(
            Bounds bounds,
            Rect textureRect,
            float textureWidth,
            float textureHeight,
            LinearSliceDefinition slice,
            bool horizontal)
        {
            var vertices = new List<Vector3>(4);
            var uvs = new List<Vector2>(4);
            var triangles = new List<int>(6);

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            if (horizontal)
            {
                float startX = min.x + slice.Start;
                float endX = startX + slice.Length;
                float bottomY = min.y;
                float topY = max.y;

                vertices.Add(new Vector3(startX, bottomY, 0f));
                vertices.Add(new Vector3(endX, bottomY, 0f));
                vertices.Add(new Vector3(startX, topY, 0f));
                vertices.Add(new Vector3(endX, topY, 0f));
            }
            else
            {
                float startY = min.y + slice.Start;
                float endY = startY + slice.Length;
                float leftX = min.x;
                float rightX = max.x;

                vertices.Add(new Vector3(leftX, startY, 0f));
                vertices.Add(new Vector3(rightX, startY, 0f));
                vertices.Add(new Vector3(leftX, endY, 0f));
                vertices.Add(new Vector3(rightX, endY, 0f));
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                uvs.Add(CalculateUV(vertices[i], bounds, textureRect, textureWidth, textureHeight));
            }

            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(3);
            triangles.Add(1);

            var mesh = new Mesh
            {
                name = "SpriteShardLinear"
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0, true);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Vector2 CalculateUV(Vector3 position, Bounds bounds, Rect textureRect, float textureWidth, float textureHeight)
        {
            Vector3 min = bounds.min;
            Vector3 size = bounds.size;
            if (size.x <= 0f || size.y <= 0f)
                return Vector2.zero;

            float normalizedX = Mathf.InverseLerp(min.x, min.x + size.x, position.x);
            float normalizedY = Mathf.InverseLerp(min.y, min.y + size.y, position.y);

            float u = (textureRect.x + textureRect.width * normalizedX) / textureWidth;
            float v = (textureRect.y + textureRect.height * normalizedY) / textureHeight;
            return new Vector2(u, v);
        }

        private readonly struct SliceDefinition
        {
            public float StartAngle { get; }
            public float Angle { get; }

            public SliceDefinition(float startAngle, float angle)
            {
                StartAngle = startAngle;
                Angle = angle;
            }

            public static IReadOnlyList<SliceDefinition> Create(int count, Vector2 range)
            {
                var slices = new List<SliceDefinition>(count);
                float minAngle = Mathf.Max(1f, Mathf.Min(range.x, range.y));
                float maxAngle = Mathf.Max(minAngle, Mathf.Max(range.x, range.y));

                float startAngle = Random.Range(0f, 360f);
                float currentAngle = startAngle;
                float remainingAngle = 360f;

                for (int i = 0; i < count; i++)
                {
                    int remainingSlices = count - i;
                    float minAllowed = Mathf.Max(minAngle, remainingAngle - maxAngle * (remainingSlices - 1));
                    float maxAllowed = Mathf.Min(maxAngle, remainingAngle - minAngle * (remainingSlices - 1));
                    float sliceAngle = remainingSlices == 1
                        ? remainingAngle
                        : Random.Range(minAllowed, Mathf.Max(minAllowed, maxAllowed));

                    slices.Add(new SliceDefinition(currentAngle, sliceAngle));
                    currentAngle += sliceAngle;
                    remainingAngle -= sliceAngle;
                }

                return slices;
            }
        }

        private readonly struct LinearSliceDefinition
        {
            public float Start { get; }
            public float Length { get; }

            public LinearSliceDefinition(float start, float length)
            {
                Start = start;
                Length = length;
            }

            public static IReadOnlyList<LinearSliceDefinition> Create(int count, float totalLength, Vector2 normalizedRange)
            {
                var slices = new List<LinearSliceDefinition>(count);
                float minNorm = Mathf.Clamp01(Mathf.Min(normalizedRange.x, normalizedRange.y));
                float maxNorm = Mathf.Clamp01(Mathf.Max(normalizedRange.x, normalizedRange.y));
                float minLength = Mathf.Max(0.01f, totalLength * Mathf.Max(0.01f, minNorm));
                float maxLength = Mathf.Max(minLength, totalLength * Mathf.Max(0.02f, maxNorm));

                float remaining = totalLength;
                float offset = 0f;

                for (int i = 0; i < count; i++)
                {
                    int remainingSlices = count - i;
                    float minAllowed = Mathf.Max(minLength, remaining - maxLength * (remainingSlices - 1));
                    float maxAllowed = Mathf.Min(maxLength, remaining - minLength * (remainingSlices - 1));
                    float length = remainingSlices == 1
                        ? remaining
                        : Random.Range(minAllowed, Mathf.Max(minAllowed, maxAllowed));

                    slices.Add(new LinearSliceDefinition(offset, length));
                    offset += length;
                    remaining -= length;
                }

                return slices;
            }
        }
    }
}
