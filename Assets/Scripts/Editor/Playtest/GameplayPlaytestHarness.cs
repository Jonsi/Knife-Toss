#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Entities.Knife.Views;
using Game.Entities.Target.Views;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace KnifeToss.EditorTools.Playtest
{
    /// <summary>
    /// Editor-only control surface for MCP and manual playtesting.
    /// Inputs are injected through Unity's Input System so the normal UI/input/gameplay
    /// pipeline handles them. The harness never invokes game callbacks directly.
    /// </summary>
    [InitializeOnLoad]
    public static class GameplayPlaytestHarness
    {
        private const double PointerHoldSeconds = 0.075d;
        private const double MinimumSchedulingLeadSeconds = 0.2d;
        private const double CollisionWatchTimeoutSeconds = 20d;
        private const float SafeAngularClearanceDegrees = 32f;
        private const float CollisionTriggerToleranceDegrees = 2.5f;
        private const BindingFlags InstanceFields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly FieldInfo TargetLastRotationDeltaField =
            typeof(TargetView).GetField("_LastRotationDelta", InstanceFields);

        private static readonly FieldInfo KnifeMainColliderField =
            typeof(KnifeView).GetField("_Collider", InstanceFields);

        private static readonly FieldInfo KnifeHitColliderField =
            typeof(KnifeView).GetField("_HitCollider", InstanceFields);

        private static readonly FieldInfo LoadedThrowSpeedField =
            typeof(KnifeThrowerView).GetField("_LoadedThrowSpeed", InstanceFields);

        private static ScheduledPointerInput _ScheduledInput;
        private static CollisionWatch _CollisionWatch;

        static GameplayPlaytestHarness()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Cancel;
            AssemblyReloadEvents.beforeAssemblyReload += Cancel;
        }

        public static bool HasScheduledInput => _ScheduledInput != null || _CollisionWatch != null;

        /// <summary>
        /// Schedules a real pointer click at the center of an active Unity UI button.
        /// Optional visibleText disambiguates repeated prefab/button names.
        /// </summary>
        public static string ClickButton(string buttonName, string visibleText = null)
        {
            if (!EnsureCanSchedule(out string error))
                return Failure(error);

            Button button = UnityEngine.Object
                .FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(candidate => candidate.gameObject.activeInHierarchy && candidate.interactable)
                .Where(candidate => string.Equals(candidate.name, buttonName, StringComparison.Ordinal))
                .FirstOrDefault(candidate =>
                    string.IsNullOrWhiteSpace(visibleText) ||
                    string.Equals(
                        candidate.GetComponentInChildren<TMP_Text>(true)?.text?.Trim(),
                        visibleText.Trim(),
                        StringComparison.Ordinal));

            if (button == null)
                return Failure($"No active interactable button matched name='{buttonName}', text='{visibleText}'.");

            Canvas canvas = button.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera != null ? canvas.worldCamera : Camera.main
                : null;
            RectTransform rectTransform = (RectTransform)button.transform;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                rectTransform.TransformPoint(rectTransform.rect.center));

            return SchedulePointer(
                screenPosition,
                MinimumSchedulingLeadSeconds,
                $"UI click {GetPath(button.transform)} text='{visibleText ?? string.Empty}'");
        }

        /// <summary>
        /// Schedules an ordinary gameplay tap at the center of the Game view.
        /// </summary>
        public static string TapGameplay(double delaySeconds = MinimumSchedulingLeadSeconds)
        {
            if (!EnsureCanSchedule(out string error))
                return Failure(error);

            Vector2 position = new(Screen.width * 0.5f, Screen.height * 0.5f);
            return SchedulePointer(position, Math.Max(delaySeconds, MinimumSchedulingLeadSeconds), "Gameplay tap");
        }

        /// <summary>
        /// Measures the target, loaded knife, throw speed, and every stuck knife, then
        /// schedules a real tap so the incoming knife reaches a stuck knife at the bottom
        /// of the target's orbit.
        /// </summary>
        public static string ScheduleIntentionalCollision()
        {
            if (!EnsureCanSchedule(out string error))
                return Failure(error);
            if (!TryGetGameplayContext(out GameplayContext context, out error))
                return Failure(error);
            if (context.StuckKnives.Count == 0)
                return Failure("The active target has no stuck knives to collide with.");
            if (Mathf.Abs(context.AngularVelocityDegreesPerSecond) < 0.01f)
                return Failure("The active target is not rotating; collision timing cannot be predicted.");

            _CollisionWatch = new CollisionWatch
            {
                ExpiresAt = EditorApplication.timeSinceStartup + CollisionWatchTimeoutSeconds
            };
            string result = Success(
                $"Armed live collision watch for {context.StuckKnives.Count} stuck knife(s); " +
                "a real gameplay press will be sent when predicted impact enters the collision lane.");
            Debug.Log($"[PlaytestHarness] {result}");
            return result;
        }

        /// <summary>
        /// Searches the upcoming target rotation for the earliest throw time whose predicted
        /// impact lane is clear of every stuck knife, then sends a real gameplay tap.
        /// </summary>
        public static string ScheduleSafeHit()
        {
            if (!EnsureCanSchedule(out string error))
                return Failure(error);
            if (!TryGetGameplayContext(out GameplayContext context, out error))
                return Failure(error);

            float targetFlightSeconds = CalculateTargetSurfaceFlightTime(context);
            float delaySeconds = (float)MinimumSchedulingLeadSeconds;
            float predictedClearance = 180f;

            if (context.StuckKnives.Count > 0)
            {
                float omega = context.AngularVelocityDegreesPerSecond;
                float horizon = Mathf.Abs(omega) > 0.01f ? 360f / Mathf.Abs(omega) : 1f;
                float bestDelay = delaySeconds;
                float bestClearance = -1f;
                bool foundSafeDelay = false;

                for (float candidate = delaySeconds; candidate <= delaySeconds + horizon; candidate += 0.01f)
                {
                    float impactTime = candidate + targetFlightSeconds;
                    float minimumClearance = 180f;
                    foreach (KnifeView stuckKnife in context.StuckKnives)
                    {
                        float currentAngle = GetWorldAngle(context.Target.transform.position, stuckKnife.transform.position);
                        float predictedAngle = currentAngle + omega * impactTime;
                        float clearance = Mathf.Abs(Mathf.DeltaAngle(predictedAngle, -90f));
                        minimumClearance = Mathf.Min(minimumClearance, clearance);
                    }

                    if (minimumClearance > bestClearance)
                    {
                        bestClearance = minimumClearance;
                        bestDelay = candidate;
                    }

                    if (minimumClearance >= SafeAngularClearanceDegrees)
                    {
                        delaySeconds = candidate;
                        predictedClearance = minimumClearance;
                        foundSafeDelay = true;
                        break;
                    }
                }

                if (!foundSafeDelay)
                {
                    delaySeconds = bestDelay;
                    predictedClearance = bestClearance;
                }
            }

            Vector2 position = new(Screen.width * 0.5f, Screen.height * 0.5f);
            string reason =
                $"Safe hit omega={context.AngularVelocityDegreesPerSecond:F2}deg/s, " +
                $"flight={targetFlightSeconds:F3}s, predictedClearance={predictedClearance:F2}deg";
            return SchedulePointer(position, delaySeconds, reason);
        }

        public static string SnapshotJson()
        {
            PlaytestSnapshot snapshot = new()
            {
                Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                Frame = Time.frameCount,
                GameTime = Time.time,
                TimeScale = Time.timeScale,
                ScreenSize = new Vector2(Screen.width, Screen.height),
                ScheduledInput = _ScheduledInput?.Reason,
                ActiveButtons = UnityEngine.Object
                    .FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Where(button => button.gameObject.activeInHierarchy)
                    .Select(button => new ButtonSnapshot
                    {
                        Name = button.name,
                        Text = button.GetComponentInChildren<TMP_Text>(true)?.text?.Trim(),
                        Path = GetPath(button.transform),
                        Interactable = button.interactable,
                        ScreenPosition = RectTransformUtility.WorldToScreenPoint(null, button.transform.position)
                    })
                    .ToArray()
            };

            TargetView target = UnityEngine.Object.FindFirstObjectByType<TargetView>();
            if (target != null)
            {
                snapshot.Target = new TargetSnapshot
                {
                    Position = target.transform.position,
                    Rotation = target.transform.eulerAngles.z,
                    AngularVelocity = GetAngularVelocity(target),
                    HitsReceived = target.HitsReceived,
                    KnivesToBreak = target.KnivesToBreak,
                    StuckKnives = FindStuckKnives(target)
                        .Select(knife => new KnifeSnapshot
                        {
                            InstanceId = knife.GetInstanceID(),
                            Position = knife.transform.position,
                            Angle = GetWorldAngle(target.transform.position, knife.transform.position),
                            InFlight = knife.IsInFlight,
                            Parent = knife.transform.parent != null ? knife.transform.parent.name : null
                        })
                        .ToArray()
                };
            }

            return JsonUtility.ToJson(snapshot, true);
        }

        public static void Cancel()
        {
            if (_ScheduledInput != null && _ScheduledInput.PressSent)
                QueueMouseState(_ScheduledInput.Position, false);
            _ScheduledInput = null;
            _CollisionWatch = null;
        }

        [MenuItem("Tools/Knife Toss/Playtest/Schedule Safe Hit")]
        private static void MenuScheduleSafeHit() => Debug.Log(ScheduleSafeHit());

        [MenuItem("Tools/Knife Toss/Playtest/Schedule Intentional Collision")]
        private static void MenuScheduleIntentionalCollision() => Debug.Log(ScheduleIntentionalCollision());

        [MenuItem("Tools/Knife Toss/Playtest/Cancel Scheduled Input")]
        private static void MenuCancel() => Cancel();

        [MenuItem("Tools/Knife Toss/Playtest/Log Snapshot")]
        private static void MenuLogSnapshot() => Debug.Log(SnapshotJson());

        private static void Tick()
        {
            if (_ScheduledInput == null && _CollisionWatch == null)
                return;
            if (!EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                Cancel();
                return;
            }

            if (_CollisionWatch != null && _ScheduledInput == null)
                UpdateCollisionWatch();

            double now = EditorApplication.timeSinceStartup;
            if (_ScheduledInput == null)
                return;
            if (!_ScheduledInput.PressSent && now >= _ScheduledInput.PressAt)
            {
                QueueMouseState(_ScheduledInput.Position, true);
                _ScheduledInput.PressSent = true;
                Debug.Log($"[PlaytestHarness] PRESS {_ScheduledInput.Reason} at {_ScheduledInput.Position}.");
                // A press and release queued in the same Editor update can collapse into
                // one Input System frame when the Editor is throttled. Always preserve at
                // least one rendered/input frame of held state.
                return;
            }

            if (_ScheduledInput.PressSent && now >= _ScheduledInput.ReleaseAt)
            {
                QueueMouseState(_ScheduledInput.Position, false);
                Debug.Log($"[PlaytestHarness] RELEASE {_ScheduledInput.Reason} at {_ScheduledInput.Position}.");
                _ScheduledInput = null;
            }
        }

        private static void UpdateCollisionWatch()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now >= _CollisionWatch.ExpiresAt)
            {
                Debug.LogWarning("[PlaytestHarness] Live collision watch timed out without finding a stable crossing.");
                _CollisionWatch = null;
                return;
            }

            if (!TryGetGameplayContext(out GameplayContext context, out _))
                return;
            if (context.StuckKnives.Count == 0 || Mathf.Abs(context.AngularVelocityDegreesPerSecond) < 0.01f)
                return;

            float bestError = float.MaxValue;
            KnifeView bestKnife = null;
            float bestAngle = 0f;
            float bestFlightSeconds = 0f;
            float bestPredictedAngle = 0f;

            foreach (KnifeView stuckKnife in context.StuckKnives)
            {
                float currentAngle = GetWorldAngle(context.Target.transform.position, stuckKnife.transform.position);
                float signedRotationToBottom = SignedRotationToAngle(
                    currentAngle,
                    -90f,
                    context.AngularVelocityDegreesPerSecond);
                float flightSeconds = CalculateKnifeCollisionFlightTime(
                    context,
                    stuckKnife,
                    signedRotationToBottom);
                float inputPipelineSeconds = Mathf.Max(Time.unscaledDeltaTime, 0.001f);
                float predictedAngle = currentAngle +
                                       context.AngularVelocityDegreesPerSecond *
                                       (flightSeconds + inputPipelineSeconds);
                float error = Mathf.Abs(Mathf.DeltaAngle(predictedAngle, -90f));

                if (error < bestError)
                {
                    bestError = error;
                    bestKnife = stuckKnife;
                    bestAngle = currentAngle;
                    bestFlightSeconds = flightSeconds;
                    bestPredictedAngle = predictedAngle;
                }
            }

            if (bestKnife == null || bestError > CollisionTriggerToleranceDegrees)
                return;

            Vector2 position = new(Screen.width * 0.5f, Screen.height * 0.5f);
            string reason =
                $"Live intentional collision knife={bestKnife.GetInstanceID()}, " +
                $"angle={bestAngle:F2}, omega={context.AngularVelocityDegreesPerSecond:F2}deg/s, " +
                $"flight={bestFlightSeconds:F3}s, predictedAngle={bestPredictedAngle:F2}, " +
                $"error={bestError:F2}deg";
            QueueMouseState(position, true);
            _ScheduledInput = new ScheduledPointerInput
            {
                Position = position,
                PressAt = now,
                ReleaseAt = now + PointerHoldSeconds,
                PressSent = true,
                Reason = reason
            };
            _CollisionWatch = null;
            Debug.Log($"[PlaytestHarness] PRESS {reason} at {position}.");
        }

        private static string SchedulePointer(Vector2 position, double delaySeconds, string reason)
        {
            double pressAt = EditorApplication.timeSinceStartup + Math.Max(delaySeconds, MinimumSchedulingLeadSeconds);
            _ScheduledInput = new ScheduledPointerInput
            {
                Position = position,
                PressAt = pressAt,
                ReleaseAt = pressAt + PointerHoldSeconds,
                Reason = reason
            };

            string result = Success(
                $"Scheduled '{reason}' in {delaySeconds:F3}s at screen position {position}.");
            Debug.Log($"[PlaytestHarness] {result}");
            return result;
        }

        private static void QueueMouseState(Vector2 position, bool pressed)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                mouse = InputSystem.AddDevice<Mouse>();

            MouseState state = new MouseState { position = position }
                .WithButton(MouseButton.Left, pressed);
            InputSystem.QueueStateEvent(mouse, state);
        }

        private static bool TryGetGameplayContext(out GameplayContext context, out string error)
        {
            context = null;
            error = null;

            TargetView target = UnityEngine.Object.FindFirstObjectByType<TargetView>();
            KnifeThrowerView thrower = UnityEngine.Object.FindFirstObjectByType<KnifeThrowerView>();
            KnifeView loadedKnife = UnityEngine.Object
                .FindObjectsByType<KnifeView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(knife =>
                    knife.gameObject.activeInHierarchy &&
                    !knife.IsInFlight &&
                    knife.transform.parent != null &&
                    knife.transform.parent.name == "KnifeThrower");
            float speed = thrower != null && LoadedThrowSpeedField?.GetValue(thrower) is float measuredSpeed
                ? measuredSpeed
                : 0f;

            if (target == null)
                error = "No active TargetView was found.";
            else if (thrower == null || loadedKnife == null)
                error = "No active loaded knife/thrower was found.";
            else if (speed <= 0f)
                error = "The loaded knife throw speed could not be measured.";

            if (error != null)
                return false;

            context = new GameplayContext
            {
                Target = target,
                LoadedKnife = loadedKnife,
                ThrowSpeed = speed,
                AngularVelocityDegreesPerSecond = GetAngularVelocity(target),
                StuckKnives = FindStuckKnives(target)
            };
            return true;
        }

        private static float GetAngularVelocity(TargetView target)
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0.00001f || TargetLastRotationDeltaField?.GetValue(target) is not float delta)
                return 0f;
            return delta / deltaTime;
        }

        private static List<KnifeView> FindStuckKnives(TargetView target)
        {
            return UnityEngine.Object
                .FindObjectsByType<KnifeView>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(knife =>
                    knife.gameObject.activeInHierarchy &&
                    !knife.IsInFlight &&
                    knife.transform.IsChildOf(target.transform))
                .ToList();
        }

        private static float CalculateTargetSurfaceFlightTime(GameplayContext context)
        {
            Collider2D targetCollider = context.Target.GetComponent<Collider2D>();
            float targetBottom = targetCollider != null
                ? targetCollider.bounds.min.y
                : context.Target.transform.position.y;
            float incomingTop = context.LoadedKnife.transform.position.y +
                                GetIncomingColliderTopOffset(context.LoadedKnife);
            return Mathf.Max(0f, (targetBottom - incomingTop) / context.ThrowSpeed);
        }

        private static float CalculateKnifeCollisionFlightTime(
            GameplayContext context,
            KnifeView stuckKnife,
            float signedRotationToBottom)
        {
            Collider2D hitCollider = KnifeHitColliderField?.GetValue(stuckKnife) as Collider2D;
            float collisionBottom = PredictRotatedColliderBottom(
                hitCollider,
                context.Target.transform.position,
                signedRotationToBottom,
                stuckKnife.transform.position);
            float incomingTop = context.LoadedKnife.transform.position.y +
                                GetIncomingColliderTopOffset(context.LoadedKnife);
            return Mathf.Max(0f, (collisionBottom - incomingTop) / context.ThrowSpeed);
        }

        private static float GetIncomingColliderTopOffset(KnifeView knife)
        {
            Collider2D collider = KnifeMainColliderField?.GetValue(knife) as Collider2D;
            if (collider is BoxCollider2D box)
            {
                Vector2 half = box.size * 0.5f;
                Vector3[] corners =
                {
                    box.transform.TransformPoint(box.offset + new Vector2(-half.x, -half.y)),
                    box.transform.TransformPoint(box.offset + new Vector2(-half.x, half.y)),
                    box.transform.TransformPoint(box.offset + new Vector2(half.x, -half.y)),
                    box.transform.TransformPoint(box.offset + new Vector2(half.x, half.y))
                };
                return corners.Max(point => point.y) - knife.transform.position.y;
            }

            return 0f;
        }

        private static float PredictRotatedColliderBottom(
            Collider2D collider,
            Vector3 pivot,
            float rotationDegrees,
            Vector3 fallbackPosition)
        {
            if (collider is not BoxCollider2D box)
                return RotateAroundPivot(fallbackPosition, pivot, rotationDegrees).y;

            Vector2 half = box.size * 0.5f;
            Vector3[] corners =
            {
                box.transform.TransformPoint(box.offset + new Vector2(-half.x, -half.y)),
                box.transform.TransformPoint(box.offset + new Vector2(-half.x, half.y)),
                box.transform.TransformPoint(box.offset + new Vector2(half.x, -half.y)),
                box.transform.TransformPoint(box.offset + new Vector2(half.x, half.y))
            };
            return corners.Min(point => RotateAroundPivot(point, pivot, rotationDegrees).y);
        }

        private static Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, float degrees)
        {
            return pivot + Quaternion.Euler(0f, 0f, degrees) * (point - pivot);
        }

        private static float GetWorldAngle(Vector3 pivot, Vector3 point)
        {
            Vector2 offset = point - pivot;
            return Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        }

        private static float SignedRotationToAngle(float currentAngle, float desiredAngle, float angularVelocity)
        {
            if (angularVelocity >= 0f)
                return Mathf.Repeat(desiredAngle - currentAngle, 360f);
            return -Mathf.Repeat(currentAngle - desiredAngle, 360f);
        }

        private static bool EnsureCanSchedule(out string error)
        {
            if (!EditorApplication.isPlaying)
                error = "Unity must be in Play Mode.";
            else if (EditorApplication.isPaused)
                error = "Unity is paused.";
            else if (_ScheduledInput != null)
                error = $"An input is already scheduled: {_ScheduledInput.Reason}.";
            else if (_CollisionWatch != null)
                error = "A live collision watch is already armed.";
            else
                error = null;
            return error == null;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
                Cancel();
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static string Success(string message) =>
            JsonUtility.ToJson(new OperationResult { Success = true, Message = message });

        private static string Failure(string message) =>
            JsonUtility.ToJson(new OperationResult { Success = false, Message = message });

        private sealed class ScheduledPointerInput
        {
            public Vector2 Position;
            public double PressAt;
            public double ReleaseAt;
            public bool PressSent;
            public string Reason;
        }

        private sealed class GameplayContext
        {
            public TargetView Target;
            public KnifeView LoadedKnife;
            public float ThrowSpeed;
            public float AngularVelocityDegreesPerSecond;
            public List<KnifeView> StuckKnives;
        }

        private sealed class CollisionWatch
        {
            public double ExpiresAt;
        }

        [Serializable]
        private sealed class OperationResult
        {
            public bool Success;
            public string Message;
        }

        [Serializable]
        private sealed class PlaytestSnapshot
        {
            public string Scene;
            public int Frame;
            public float GameTime;
            public float TimeScale;
            public Vector2 ScreenSize;
            public string ScheduledInput;
            public TargetSnapshot Target;
            public ButtonSnapshot[] ActiveButtons;
        }

        [Serializable]
        private sealed class TargetSnapshot
        {
            public Vector3 Position;
            public float Rotation;
            public float AngularVelocity;
            public int HitsReceived;
            public int KnivesToBreak;
            public KnifeSnapshot[] StuckKnives;
        }

        [Serializable]
        private sealed class KnifeSnapshot
        {
            public int InstanceId;
            public Vector3 Position;
            public float Angle;
            public bool InFlight;
            public string Parent;
        }

        [Serializable]
        private sealed class ButtonSnapshot
        {
            public string Name;
            public string Text;
            public string Path;
            public bool Interactable;
            public Vector2 ScreenPosition;
        }
    }
}
#endif
