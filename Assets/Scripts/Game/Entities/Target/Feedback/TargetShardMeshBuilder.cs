using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities.Target.Feedback
{
    /// <summary>
    /// Builds shard meshes by generating radial slices.
    /// </summary>
    internal static class TargetShardMeshBuilder
    {
        private const float MAX_SEGMENT_ANGLE = 12f;

        public static List<Mesh> Build(Sprite sprite, int shardCount, Vector2 sliceAngleRange)
        {
            var meshes = new List<Mesh>();
            if (sprite == null || sprite.texture == null)
                return meshes;

            int targetShardCount = Mathf.Clamp(shardCount, 1, 32);
            IReadOnlyList<SliceDefinition> slices = SliceDefinition.Create(targetShardCount, sliceAngleRange);

            Bounds bounds = sprite.bounds;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            Texture texture = sprite.texture;
            Rect textureRect = sprite.textureRect;

            for (int i = 0; i < slices.Count; i++)
            {
                Mesh mesh = CreateSliceMesh(sprite, bounds, textureRect, texture.width, texture.height, radius, slices[i]);
                if (mesh != null)
                    meshes.Add(mesh);
            }

            return meshes;
        }

        private static Mesh CreateSliceMesh(
            Sprite sprite,
            Bounds bounds,
            Rect textureRect,
            float textureWidth,
            float textureHeight,
            float radius,
            SliceDefinition slice)
        {
            int segments = Mathf.Max(1, Mathf.CeilToInt(slice.Angle / MAX_SEGMENT_ANGLE));
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
                name = "TargetShard"
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
    }
}
