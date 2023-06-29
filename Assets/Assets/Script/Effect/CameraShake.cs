// Copyright (c) 2011-2017 Thinksquirrel Inc.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Thinksquirrel.CShake
{
    /// <summary>
    /// Shakes the camera and UI.
    /// </summary>
    /// <remarks>
    /// All events use the System.Action method signature.
    /// </remarks>
    public class CameraShake : CameraShakeBase
    {
        [System.Serializable]
        public struct ShakeParam
        {
            public ShakeType shakeType;
            public int numberOfShakes;
            public Vector3 shakeAmount;
            public Vector3 rotationAmount;
            public float distance;
            public float speed;
            public float decay;
            public float uiShakeModifier;
            public bool multiplyByTimeScale;
        }
            /// <summary>
        /// The type of shake to perform on a camera.
        /// </summary>
        public enum ShakeType
        {
            /// <summary>
            /// Shakes the camera matrix.
            /// </summary>
            CameraMatrix,
            /// <summary>
            /// Shakes the camera's local position.
            /// </summary>
            LocalPosition,
        }
        
        [SerializeField] private List<Camera> m_Cameras = new List<Camera>();
        private ShakeType m_ShakeType = ShakeType.CameraMatrix;
        private int m_NumberOfShakes = 2;
        private Vector3 m_ShakeAmount = Vector3.one;
        private Vector3 m_RotationAmount = Vector3.one;
        private float m_Distance = 00.10f;
        private float m_Speed = 50.00f;
        private float m_Decay = 00.20f;
        private float m_UiShakeModifier = 01.00f;
        private bool m_MultiplyByTimeScale = true;

        public ShakeParam shootParam;
        public ShakeParam hitParam;



        // Analysis disable ConvertToAutoProperty
        /// <summary>
        /// The cameras to shake.
        /// </summary>
        public List<Camera> cameras
        {
            get { return m_Cameras; }
            set { m_Cameras = value; }
        }
        /// <summary>
        /// The type of shake to perform (camera matrix or local position).
        /// </summary>
        public ShakeType shakeType
        {
            get { return m_ShakeType; }
            set { m_ShakeType = value; }
        }
        /// <summary>
        /// The maximum number of shakes to perform.
        /// </summary>
        public int numberOfShakes
        {
            get { return m_NumberOfShakes; }
            set { m_NumberOfShakes = value; }
        }
        /// <summary>
        /// The amount to shake in each direction.
        /// </summary>
        public Vector3 shakeAmount
        {
            get { return m_ShakeAmount; }
            set { m_ShakeAmount = value; }
        }
        /// <summary>
        /// The amount to rotate in each direction.
        /// </summary>
        public Vector3 rotationAmount
        {
            get { return m_RotationAmount; }
            set { m_RotationAmount = value; }
        }
        /// <summary>
        /// The initial distance for the first shake.
        /// </summary>
        public float distance
        {
            get { return m_Distance; }
            set { m_Distance = value; }
        }
        /// <summary>
        /// The speed multiplier for the shake.
        /// </summary>
        public float speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }
        /// <summary>
        /// The decay speed (between 0 and 1). Higher values will stop shaking sooner.
        /// </summary>
        public float decay
        {
            get { return m_Decay; }
            set { m_Decay = value; }
        }
        [System.Obsolete("Use CameraShake.uiShakeModifier instead")]
        public float guiShakeModifier
        {
            get { return uiShakeModifier; }
            set { uiShakeModifier = value; }
        }
        /// <summary>
        /// The modifier applied to speed in order to shake the UI.
        /// </summary>
        public float uiShakeModifier
        {
            get { return m_UiShakeModifier; }
            set { m_UiShakeModifier = value; }
        }
        /// <summary>
        /// If true, multiplies the final shake speed by the time scale.
        /// </summary>
        public bool multiplyByTimeScale
        {
            get { return m_MultiplyByTimeScale; }
            set { m_MultiplyByTimeScale = value; }
        }
        /// <summary>
        /// Gets the current shake area for the UI (in screen space).
        /// </summary>
        public Rect uiShakeRect
        {
            get { return m_ShakeRect; }
            set { m_ShakeRect = value; }
        }
        // Analysis restore ConvertToAutoProperty

        

        // Shake rect (for UI)
        // Analysis disable FieldCanBeMadeReadOnly.Local
        private Rect m_ShakeRect;
        // Analysis restore FieldCanBeMadeReadOnly.Local

        // States
        private bool m_Shaking;
        private bool m_Cancelling;
        private readonly List<Vector3> m_MatrixOffsetCache = new List<Vector3>(10);
        private readonly List<Quaternion> m_MatrixRotationCache = new List<Quaternion>(10);
        private readonly List<Vector3> m_OffsetCache = new List<Vector3>(10);
        private readonly List<Quaternion> m_RotationCache = new List<Quaternion>(10);
        private readonly List<bool> m_IgnoreMatrixCache = new List<bool>(10);
        private readonly List<bool> m_IgnorePositionCache = new List<bool>(10);

        internal class ShakeState
        {
            internal readonly ShakeType _shakeType;
            internal readonly Vector3 _startPosition;
            internal readonly Quaternion _startRotation;
            internal readonly Vector2 _uiStartPosition;
            internal Vector3 _shakePosition;
            internal Quaternion _shakeRotation;
            internal Vector2 _uiShakePosition;

            internal ShakeState(ShakeType shakeType, Vector3 position, Quaternion rotation, Vector2 uiPosition)
            {
                _shakeType = shakeType;
                _startPosition = position;
                _startRotation = rotation;
                _uiStartPosition = uiPosition;
                _shakePosition = position;
                _shakeRotation = rotation;
                _uiShakePosition = uiPosition;
            }
        }

        private readonly Dictionary<Camera, List<ShakeState>> m_States = new Dictionary<Camera, List<ShakeState>>();
        private readonly Dictionary<Camera, int> m_ShakeCount = new Dictionary<Camera, int>();

        // Minimum shake values
        private const float minShakeValue = 0.001f;
        private const float minRotationValue = 0.001f;

        #region Components
        private static readonly List<CameraShake> m_Components = new List<CameraShake>();

        [System.Obsolete("Use CameraShake.GetComponents() to get all Camera Shake components")]
        public static CameraShake instance
        {
            get { return m_Components.Count > 0 ? m_Components[0] : null; }
        }

        private void OnEnable()
        {
            if (cameras.Count < 1)
            {
                if (GetComponent<Camera>()) cameras.Add(GetComponent<Camera>());
            }

            if (cameras.Count < 1)
            {
                if (Camera.main) cameras.Add(Camera.main);
            }

            if (cameras.Count < 1)
            {
                LogError("No cameras assigned in the inspector!", "Camera Shake", "CameraShake", this);
            }

            m_Components.Add(this);
        }

        private void OnDisable()
        {
            m_Components.Remove(this);
        }

        /// <summary>
        /// Get all enabled Camera Shake components in the scene.
        /// </summary>
        /// <returns>An array of all enabled Camera Shake components.</returns>
        public static CameraShake[] GetComponents()
        {
            return m_Components.ToArray();
        }
        #endregion

        #region Static properties
        [System.Obsolete("Use IsShaking method on individual Camera Shake components")]
        public static bool isShaking
        {
            get
            {
                for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
                {
                    var component = m_Components[i];
                    if (component.IsShaking()) return true;
                }

                return false;
            }
        }

        [System.Obsolete("Use IsCancelling method on individual Camera Shake components")]
        public static bool isCancelling
        {
            get
            {
                var inst = m_Components.Count > 0 ? m_Components[0] : null;
                return inst != null && inst.IsCancelling();
            }
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Shake all cameras associated with all enabled Camera Shake
        /// components in the scene, using the default settings.
        /// </summary>
        public static void ShakeAll()
        {
            for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
            {
                var component = m_Components[i];
                component.Shake();
            }
        }

        /// <summary>
        /// Shake all cameras associated with all enabled Camera Shake
        /// components in the scene, temporarily overriding the default settings.
        /// </summary>
        /// <param name='shakeType'>The type of shake to perform (camera matrix
        /// or local position).</param>
        /// <param name='numberOfShakes'>The maximum number of shakes to
        /// perform.</param>
        /// <param name='shakeAmount'>The amount to shake in each
        /// direction.</param>
        /// <param name='rotationAmount'>The amount to rotate in each
        /// axis.</param>
        /// <param name='distance'>The initial distance for the first
        /// shake.</param>
        /// <param name='speed'>The speed multiplier for the shake.</param>
        /// <param name='decay'>The decay speed (between 0 and 1). Higher values
        /// will stop shaking sooner.</param>
        /// <param name='uiShakeModifier'>The modifier applied to speed in order
        /// to shake the UI.</param>
        /// <param name='multiplyByTimeScale'>If true, multiplies the shake
        /// speed by the time scale.</param>
        public static void ShakeAll(
            ShakeType shakeType,
            int numberOfShakes,
            Vector3 shakeAmount,
            Vector3 rotationAmount,
            float distance,
            float speed,
            float decay,
            float uiShakeModifier,
            bool multiplyByTimeScale)
        {
            for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
            {
                var component = m_Components[i];
                component.Shake(shakeType,
                                numberOfShakes,
                                shakeAmount,
                                rotationAmount,
                                distance,
                                speed,
                                decay,
                                uiShakeModifier,
                                multiplyByTimeScale);
            }
        }

        /// <summary>
        /// Shake all cameras associated with all enabled Camera Shake
        /// components in the scene, then call the specified callback once
        /// completed.
        /// </summary>
        /// <param name='callback'>An action to invoke after the cameras have
        /// finished shaking.</param>
        public static void ShakeAll(System.Action callback)
        {
            for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
            {
                var component = m_Components[i];
                component.Shake(callback);
            }
        }

        /// <summary>
        /// Shake all cameras associated with all enabled Camera Shake
        /// components in the scene (using temporarily overriden settings), then
        /// call the specified callback once completed.
        /// </summary>
        /// <param name='shakeType'>The type of shake to perform (camera matrix
        /// or local position).</param>
        /// <param name='numberOfShakes'>The maximum number of shakes to
        /// perform.</param>
        /// <param name='shakeAmount'>The amount to shake in each
        /// direction.</param>
        /// <param name='rotationAmount'>The amount to rotate in each
        /// axis.</param>
        /// <param name='distance'>The initial distance for the first
        /// shake.</param>
        /// <param name='speed'>The speed multiplier for the shake.</param>
        /// <param name='decay'>The decay speed (between 0 and 1). Higher values
        /// will stop shaking sooner.</param>
        /// <param name='uiShakeModifier'>The modifier applied to speed in order
        /// to shake the UI.</param>
        /// <param name='multiplyByTimeScale'>If true, multiplies the shake
        /// speed by the time scale.</param>
        /// <param name='callback'>An action to invoke after the cameras have
        /// finished shaking.</param>
        public static void ShakeAll(
            ShakeType shakeType,
            int numberOfShakes,
            Vector3 shakeAmount,
            Vector3 rotationAmount,
            float distance,
            float speed,
            float decay,
            float uiShakeModifier,
            bool multiplyByTimeScale,
            System.Action callback)
        {
            for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
            {
                var component = m_Components[i];
                component.Shake(shakeType,
                                numberOfShakes,
                                shakeAmount,
                                rotationAmount,
                                distance,
                                speed,
                                decay,
                                uiShakeModifier,
                                multiplyByTimeScale,
                                callback);
            }
        }

        /// <summary>
        /// Cancel all camera shakes in progress in the current scene, and reset
        /// all cameras.
        /// </summary>
        public static void CancelAllShakes()
        {
            for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
            {
                var component = m_Components[i];
                component.CancelShake();
            }
        }

        /// <summary>
        /// Cancel all camera shakes in progress in the current scene, and reset
        /// all cameras over the specified time (in seconds).
        /// </summary>
        /// <param name='time'>The amount of time to take to reset the cameras,
        /// in seconds. During this time, the camera is smoothly moved back to
        /// its original orientation.</param>
        public static void CancelAllShakes(float time)
        {
            for (int i = 0, m_ComponentsCount = m_Components.Count; i < m_ComponentsCount; i++)
            {
                var component = m_Components[i];
                component.CancelShake(time);
            }
        }
        #endregion

        #region Events
        // Analysis disable InconsistentNaming
        // Analysis disable DelegateSubtraction
        [System.Obsolete("Use CameraShake.onStartShaking instead")]
        public event System.Action cameraShakeStarted
        {
            add { onStartShaking += value; }
            remove { onStartShaking -= value; }
        }
        [System.Obsolete("Use CameraShake.onEndShaking")]
        public event System.Action allCameraShakesCompleted
        {
            add { onEndShaking += value; }
            remove { onEndShaking -= value; }
        }
        // Analysis restore DelegateSubtraction
        // Analysis restore InconsistentNaming

        /// <summary>
        /// Occurs when a camera starts shaking.
        /// </summary>
        public event System.Action onStartShaking;
        /// <summary>
        /// Occurs when a camera has completely stopped shaking and has been reset to its original position.
        /// </summary>
        public event System.Action onEndShaking;
        /// <summary>
        /// Occurs before every individual camera shake.
        /// </summary>
        public event System.Action onPreShake;
        /// <summary>
        /// Occurs after every individual camera shake.
        /// </summary>
        public event System.Action onPostShake;
        /// <summary>
        /// Occurs before every individual camera movement, with raw positional and rotational offset data.
        /// </summary>
        public event System.Action<Vector3, Quaternion> onShakeOffset;
        #endregion

        #region Public methods
        /// <summary>
        /// Returns true if the camera is currently shaking.
        /// </summary>
        /// <returns>True if the camera is currently shaking; otherwise
        /// false.</returns>
        public bool IsShaking()
        {
            return m_Shaking;
        }

        /// <summary>
        /// Returns true if the camera is currently cancelling a shake action.
        /// </summary>
        /// <returns>True if the camera is currently cancelling a shake;
        /// otherwise false.</returns>
        public bool IsCancelling()
        {
            return m_Cancelling;
        }

        /// <summary>
        /// Shake all cameras associated with this component, using the default
        /// settings.
        /// </summary>
        public void Shake()
        {
            var seed = Random.insideUnitSphere;

            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];
                StartCoroutine(DoShake_Internal(cam,
                                                seed,
                                                shakeType,
                                                numberOfShakes,
                                                shakeAmount,
                                                rotationAmount,
                                                distance,
                                                speed,
                                                decay,
                                                uiShakeModifier,
                                                multiplyByTimeScale,
                                                null));
            }
        }

        public void Shake(ShakeParam param)
        {
            var seed = Random.insideUnitSphere;
            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];
                StartCoroutine(DoShake_Internal(cam,
                                                seed,
                                                param.shakeType,
                                                param.numberOfShakes,
                                                param.shakeAmount,
                                                param.rotationAmount,
                                                param.distance,
                                                param.speed,
                                                param.decay,
                                                param.uiShakeModifier,
                                                param.multiplyByTimeScale,
                                                null));
            }
        }

        public void HitShake()
        {
            Shake(hitParam);
        }

        public void ShootShake()
        {
            Shake(shootParam);
        }
        /// <summary>
        /// Shake all cameras associated with this component, temporarily
        /// overriding the default settings.
        /// </summary>
        /// <param name='shakeType'>The type of shake to perform (camera matrix
        /// or local position).</param>
        /// <param name='numberOfShakes'>The maximum number of shakes to
        /// perform.</param>
        /// <param name='shakeAmount'>The amount to shake in each
        /// direction.</param>
        /// <param name='rotationAmount'>The amount to rotate in each
        /// axis.</param>
        /// <param name='distance'>The initial distance for the first
        /// shake.</param>
        /// <param name='speed'>The speed multiplier for the shake.</param>
        /// <param name='decay'>The decay speed (between 0 and 1). Higher values
        /// will stop shaking sooner.</param>
        /// <param name='uiShakeModifier'>The modifier applied to speed in order
        /// to shake the UI.</param>
        /// <param name='multiplyByTimeScale'>If true, multiplies the shake
        /// speed by the time scale.</param>
        public void Shake(
            ShakeType shakeType,
            int numberOfShakes,
            Vector3 shakeAmount,
            Vector3 rotationAmount,
            float distance,
            float speed,
            float decay,
            float uiShakeModifier,
            bool multiplyByTimeScale)
        {
            var seed = Random.insideUnitSphere;

            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];
                StartCoroutine(DoShake_Internal(cam,
                                                seed,
                                                shakeType,
                                                numberOfShakes,
                                                shakeAmount,
                                                rotationAmount,
                                                distance,
                                                speed,
                                                decay,
                                                uiShakeModifier,
                                                multiplyByTimeScale,
                                                null));
            }
        }

        /// <summary>
        /// Shake all cameras associated with this component (using the default
        /// settings), then call the specified callback once completed.
        /// </summary>
        /// <param name='callback'>An action to invoke after the cameras have
        /// finished shaking.</param>
        public void Shake(System.Action callback)
        {
            var seed = Random.insideUnitSphere;

            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];
                StartCoroutine(DoShake_Internal(cam,
                                                seed,
                                                shakeType,
                                                numberOfShakes,
                                                shakeAmount,
                                                rotationAmount,
                                                distance,
                                                speed,
                                                decay,
                                                uiShakeModifier,
                                                multiplyByTimeScale,
                                                callback));
            }
        }

        /// <summary>
        /// Shake all cameras associated with this component (using temporarily
        /// overriden settings), then call the specified callback once completed.
        /// </summary>
        /// <param name='shakeType'>The type of shake to perform (camera matrix
        /// or local position).</param>
        /// <param name='numberOfShakes'>The maximum number of shakes to
        /// perform.</param>
        /// <param name='shakeAmount'>The amount to shake in each
        /// direction.</param>
        /// <param name='rotationAmount'>The amount to rotate in each
        /// axis.</param>
        /// <param name='distance'>The initial distance for the first
        /// shake.</param>
        /// <param name='speed'>The speed multiplier for the shake.</param>
        /// <param name='decay'>The decay speed (between 0 and 1). Higher values
        /// will stop shaking sooner.</param>
        /// <param name='uiShakeModifier'>The modifier applied to speed in order
        /// to shake the UI.</param>
        /// <param name='multiplyByTimeScale'>If true, multiplies the shake
        /// speed by the time scale.</param>
        /// <param name='callback'>An action to invoke after the cameras have
        /// finished shaking.</param>
        public void Shake(
            ShakeType shakeType,
            int numberOfShakes,
            Vector3 shakeAmount,
            Vector3 rotationAmount,
            float distance,
            float speed,
            float decay,
            float uiShakeModifier,
            bool multiplyByTimeScale,
            System.Action callback)
        {
            var seed = Random.insideUnitSphere;

            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];
                StartCoroutine(DoShake_Internal(cam,
                                                seed,
                                                shakeType,
                                                numberOfShakes,
                                                shakeAmount,
                                                rotationAmount,
                                                distance,
                                                speed,
                                                decay,
                                                uiShakeModifier,
                                                multiplyByTimeScale,
                                                callback));
            }
        }

        /// <summary>
        /// Cancels and resets a camera shake in progress.
        /// </summary>
        public void CancelShake()
        {
            if (m_Shaking && !m_Cancelling)
            {
                m_Shaking = false;
                StopAllCoroutines();
                for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
                {
                    var cam = cameras[i];
                    if (m_ShakeCount.ContainsKey(cam))
                    {
                        m_ShakeCount[cam] = 0;
                    }
                    ResetState(cam);
                }
            }
        }

        /// <summary>
        /// Cancels a camera shake in progress, and resets associated cameras
        /// over the specified time, in seconds.
        /// </summary>
        /// <param name='time'>The amount of time to take to reset the camera,
        /// in seconds. During this time, the camera is smoothly moved back to
        /// its original orientation.</param>
        public void CancelShake(float time)
        {
            if (m_Shaking && !m_Cancelling)
            {
                StopAllCoroutines();
                StartCoroutine(DoResetState(cameras, m_ShakeCount, time));
            }
        }

        /// <summary>
        /// Use this at the beginning of an OnGUI call in order to shake OnGUI
        /// with Camera Shake.
        /// </summary>
        public void BeginShakeGUI()
        {
            CheckShakeRect();
            GUI.BeginGroup(m_ShakeRect);
        }

        /// <summary>
        /// Use this at the end of an OnGUI call in order to shake OnGUI with
        /// Camera Shake.
        /// </summary>
        public void EndShakeGUI()
        {
            GUI.EndGroup();
        }

        /// <summary>
        /// Use this at the beginning of an OnGUI call in order to shake OnGUI
        /// with Camera Shake (uses GUILayout).
        /// </summary>
        public void BeginShakeGUILayout()
        {
            CheckShakeRect();
            GUILayout.BeginArea(m_ShakeRect);
        }

        /// <summary>
        /// Use this at the end of an OnGUI call in order to shake OnGUI with
        /// Camera Shake (uses GUILayout).
        /// </summary>
        public void EndShakeGUILayout()
        {
            GUILayout.EndArea();
        }
        #endregion

        #region Private methods
        private void OnDrawGizmosSelected()
        {
            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];

                if (!cam) continue;

                if (IsShaking())
                {
                    var offset = cam.worldToCameraMatrix.GetColumn(3);
                    offset.z *= -1;
                    offset = cam.transform.position + cam.transform.TransformPoint(offset);
                    var rot = QuaternionFromMatrix(cam.worldToCameraMatrix.inverse *
                                                   Matrix4x4.TRS(
                                                       Vector3.zero,
                                                       Quaternion.identity,
                                                       new Vector3(1, 1, -1)));
                    var matrix = Matrix4x4.TRS(offset, rot, cam.transform.lossyScale);
                    Gizmos.matrix = matrix;
                }
                else
                {
                    var matrix = Matrix4x4.TRS(cam.transform.position,
                                               cam.transform.rotation,
                                               cam.transform.lossyScale);
                    Gizmos.matrix = matrix;
                }
                Gizmos.DrawWireCube(Vector3.zero, shakeAmount);
                Gizmos.color = Color.cyan;
                if (cam.orthographic)
                {
                    var pos = new Vector3(0, 0, (cam.nearClipPlane + cam.farClipPlane) / 2f);
                    var size = new Vector3(cam.orthographicSize / cam.aspect,
                                           cam.orthographicSize * 2f,
                                           cam.farClipPlane - cam.nearClipPlane);
                    Gizmos.DrawWireCube(pos, size);
                }
                else
                {
                    Gizmos.DrawFrustum(Vector3.zero,
                                       cam.fieldOfView,
                                       cam.farClipPlane,
                                       cam.nearClipPlane,
                                       (.7f / cam.aspect));
                }
            }
        }

        private IEnumerator DoShake_Internal(
            Camera cam,
            Vector3 seed,
            ShakeType shakeType,
            int numberOfShakes,
            Vector3 shakeAmount,
            Vector3 rotationAmount,
            float distance,
            float speed,
            float decay,
            float uiShakeModifier,
            bool multiplyByTimeScale,
            System.Action callback)
        {
            // Wait for async cancel operations to complete
            if (m_Cancelling) yield return null;

            // Set random values
            var mod1 = seed.x > .5f ? 1 : -1;
            var mod2 = seed.y > .5f ? 1 : -1;
            var mod3 = seed.z > .5f ? 1 : -1;

            // First shake
            if (!m_Shaking)
            {
                m_Shaking = true;

                if (onStartShaking != null) onStartShaking();
            }

            if (m_ShakeCount.ContainsKey(cam)) m_ShakeCount[cam]++;
            else m_ShakeCount.Add(cam, 1);

            if (onPreShake != null) onPreShake();

            // Pixel width is always based on the first camera
            var pixelWidth = GetPixelWidth(cameras[0].transform, cameras[0]);

            // Set other values
            var cachedTransform = cam.transform;
            Vector3 camOffset;
            Quaternion camRot;

            var currentShakes = numberOfShakes;
            var shakeDistance = distance;
            float rotationStrength = 1;

            var startTime = Time.time;
            var scale = multiplyByTimeScale ? Time.timeScale : 1;
            var pixelScale = pixelWidth * uiShakeModifier * scale;
            Vector3 start1 = Vector2.zero;
            var startR = Quaternion.identity;
            var start2 = Vector2.zero;

            var state = new ShakeState(shakeType,
                                       cachedTransform.position,
                                       cachedTransform.rotation,
                                       new Vector2(m_ShakeRect.x, m_ShakeRect.y));
            List<ShakeState> stateList;

            if (m_States.TryGetValue(cam, out stateList))
            {
                stateList.Add(state);
            }
            else
            {
                stateList = new List<ShakeState>();
                stateList.Add(state);
                m_States.Add(cam, stateList);
            }

            // Main loop
            while (currentShakes > 0)
            {
                // Early break when rotation is less than the minimum value.
                if (Mathf.Abs(rotationAmount.sqrMagnitude) > Vector3.kEpsilon &&
                    rotationStrength <= minRotationValue) break;

                // Early break when shake amount is less than the minimum value.
                if (Mathf.Abs(shakeAmount.sqrMagnitude) > Vector3.kEpsilon &&
                    Mathf.Abs(distance) > Vector3.kEpsilon &&
                    shakeDistance <= minShakeValue) break;

                var timer = (Time.time - startTime) * speed;

                state._shakePosition = start1 +
                                       new Vector3(
                                           mod1 * Mathf.Sin(timer) * (shakeAmount.x * shakeDistance * scale),
                                           mod2 * Mathf.Cos(timer) * (shakeAmount.y * shakeDistance * scale),
                                           mod3 * Mathf.Sin(timer) * (shakeAmount.z * shakeDistance * scale));

                state._shakeRotation = startR *
                                       Quaternion.Euler(
                                           mod1 * Mathf.Cos(timer) * (rotationAmount.x * rotationStrength * scale),
                                           mod2 * Mathf.Sin(timer) * (rotationAmount.y * rotationStrength * scale),
                                           mod3 * Mathf.Cos(timer) * (rotationAmount.z * rotationStrength * scale));

                state._uiShakePosition = new Vector2(
                    start2.x - (mod1 * Mathf.Sin(timer) * (shakeAmount.x * shakeDistance * pixelScale)),
                    start2.y - (mod2 * Mathf.Cos(timer) * (shakeAmount.y * shakeDistance * pixelScale)));

                camOffset = GetGeometricAvg(stateList, true);
                camRot = GetAvgRotation(stateList);
                NormalizeQuaternion(ref camRot);

                if (onShakeOffset != null) onShakeOffset(camOffset, camRot);

                switch (state._shakeType)
                {
                    case ShakeType.CameraMatrix:
                        var m = Matrix4x4.TRS(camOffset, camRot, new Vector3(1, 1, -1));
                        cam.worldToCameraMatrix = m * cachedTransform.worldToLocalMatrix;
                        break;
                    case ShakeType.LocalPosition:
                        cachedTransform.localPosition = camOffset;
                        cachedTransform.localRotation = camRot;
                        break;
                }

                var avg = GetGeometricAvg(stateList, false);

                m_ShakeRect.x = avg.x;
                m_ShakeRect.y = avg.y;

                if (timer > Mathf.PI * 2)
                {
                    startTime = Time.time;
                    shakeDistance *= (1 - Mathf.Clamp01(decay));
                    rotationStrength *= (1 - Mathf.Clamp01(decay));
                    currentShakes--;
                }
                yield return null;
            }

            // End conditions
            m_ShakeCount[cam]--;

            if (onPostShake != null) onPostShake();

            // Last shake
            if (m_ShakeCount[cam] == 0)
            {
                m_Shaking = false;
                ResetState(cam);

                if (onEndShaking != null)
                {
                    onEndShaking();
                }
            }
            else
            {
                stateList.Remove(state);
            }

            if (callback != null) callback();
        }

        private void CheckShakeRect()
        {
            if (Mathf.Abs(Screen.width - m_ShakeRect.width) > Vector3.kEpsilon ||
                Mathf.Abs(Screen.height - m_ShakeRect.height) > Vector3.kEpsilon)
            {
                m_ShakeRect.width = Screen.width;
                m_ShakeRect.height = Screen.height;
            }
        }

        private void ResetState(Camera cam)
        {
            cam.ResetWorldToCameraMatrix();

            m_ShakeRect.x = 0;
            m_ShakeRect.y = 0;

            m_States[cam].Clear();
        }

        private IEnumerator DoResetState(IList<Camera> cameras, IDictionary<Camera, int> shakeCount, float time)
        {
            m_MatrixOffsetCache.Clear();
            m_MatrixRotationCache.Clear();
            m_OffsetCache.Clear();
            m_RotationCache.Clear();
            m_IgnoreMatrixCache.Clear();
            m_IgnorePositionCache.Clear();

            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];
                var cachedTransform = cam.transform;
                m_MatrixOffsetCache.Add(
                    (Vector3) ((cam.worldToCameraMatrix * cachedTransform.worldToLocalMatrix.inverse).GetColumn(3)));
                m_MatrixRotationCache.Add(
                    QuaternionFromMatrix(
                        (cam.worldToCameraMatrix * cachedTransform.worldToLocalMatrix.inverse).inverse *
                        Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1))));
                m_OffsetCache.Add(cachedTransform.localPosition);
                m_RotationCache.Add(cachedTransform.localRotation);

                if (shakeCount.ContainsKey(cam))
                {
                    shakeCount[cam] = 0;
                }

                var ignoreMatrix = true;
                var ignorePosition = true;
                var states = m_States[cam];

                for (int j = 0, statesCount = states.Count; j < statesCount; j++)
                {
                    var state = states[j];

                    if (state._shakeType == ShakeType.CameraMatrix)
                    {
                        ignoreMatrix = false;
                    }
                    else
                    {
                        ignorePosition = false;
                    }

                    if (!(ignoreMatrix || ignorePosition)) break;
                }

                m_IgnoreMatrixCache.Add(ignoreMatrix);
                m_IgnorePositionCache.Add(ignorePosition);

                m_States[cam].Clear();
            }

            float t = 0;
            float x = m_ShakeRect.x, y = m_ShakeRect.y;
            m_Cancelling = true;
            while (t < time)
            {
                var i = 0;
                for (int j = 0, camerasCount = cameras.Count; j < camerasCount; j++)
                {
                    var cam = cameras[j];
                    var cachedTransform = cam.transform;
                    m_ShakeRect.x = Mathf.Lerp(x, 0, t / time);
                    m_ShakeRect.y = Mathf.Lerp(y, 0, t / time);

                    if (!m_IgnoreMatrixCache[j])
                    {
                        var matrixPos = Vector3.Lerp(m_MatrixOffsetCache[i], Vector3.zero, t / time);
                        var matrixRot = Quaternion.Slerp(m_MatrixRotationCache[i], cachedTransform.rotation, t / time);
                        var m = Matrix4x4.TRS(matrixPos, matrixRot, new Vector3(1, 1, -1));
                        cam.worldToCameraMatrix = m * cachedTransform.worldToLocalMatrix;
                    }
                    if (!m_IgnorePositionCache[j])
                    {
                        var pos = Vector3.Lerp(m_OffsetCache[i], Vector3.zero, t / time);
                        var rot = Quaternion.Slerp(m_RotationCache[i], Quaternion.identity, t / time);
                        cachedTransform.localPosition = pos;
                        cachedTransform.localRotation = rot;
                    }
                    i++;
                }
                t += Time.deltaTime;
                yield return null;
            }

            for (int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                var cam = cameras[i];

                if (!m_IgnoreMatrixCache[i]) cam.ResetWorldToCameraMatrix();

                if (!m_IgnorePositionCache[i])
                {
                    cam.transform.localPosition = Vector3.zero;
                    cam.transform.localRotation = Quaternion.identity;
                }

                m_ShakeRect.x = 0;
                m_ShakeRect.y = 0;
            }
            m_Shaking = false;
            m_Cancelling = false;
        }
        #endregion

        #region Math helpers
        private static Vector3 GetGeometricAvg(IList<ShakeState> states, bool position)
        {
            float x = 0, y = 0, z = 0, l = states.Count;

            for (int i = 0, statesCount = states.Count; i < statesCount; i++)
            {
                var state = states[i];
                if (position)
                {
                    x -= state._shakePosition.x;
                    y -= state._shakePosition.y;
                    z -= state._shakePosition.z;
                }
                else
                {
                    x += state._uiShakePosition.x;
                    y += state._uiShakePosition.y;
                }
            }

            return new Vector3(x / l, y / l, z / l);
        }

        private static Quaternion GetAvgRotation(IList<ShakeState> states)
        {
            var avg = new Quaternion(0, 0, 0, 0);

            for (int i = 0, statesCount = states.Count; i < statesCount; i++)
            {
                var state = states[i];
                if (Quaternion.Dot(state._shakeRotation, avg) > 0)
                {
                    avg.x += state._shakeRotation.x;
                    avg.y += state._shakeRotation.y;
                    avg.z += state._shakeRotation.z;
                    avg.w += state._shakeRotation.w;
                }
                else
                {
                    avg.x += -state._shakeRotation.x;
                    avg.y += -state._shakeRotation.y;
                    avg.z += -state._shakeRotation.z;
                    avg.w += -state._shakeRotation.w;
                }
            }

            var mag = Mathf.Sqrt(avg.x * avg.x + avg.y * avg.y + avg.z * avg.z + avg.w * avg.w);

            if (mag > 0.0001f)
            {
                avg.x /= mag;
                avg.y /= mag;
                avg.z /= mag;
                avg.w /= mag;
            }
            else
            {
                avg = states[0]._shakeRotation;
            }

            return avg;
        }
        private static float GetPixelWidth(Transform cachedTransform, Camera cachedCamera)
        {
            var position = cachedTransform.position;
            var screenPos = cachedCamera.WorldToScreenPoint(position - cachedTransform.forward * .01f);
            var offset = screenPos;

            if (screenPos.x > 0) offset -= Vector3.right;
            else offset += Vector3.right;

            if (screenPos.y > 0) offset -= Vector3.up;
            else offset += Vector3.up;

            offset = cachedCamera.ScreenToWorldPoint(offset);

            return 1f /
                   (cachedTransform.InverseTransformPoint(position) - cachedTransform.InverseTransformPoint(offset))
                   .magnitude;
        }

        private static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }

        private static void NormalizeQuaternion(ref Quaternion q)
        {
            float sum = 0;

            for (var i = 0; i < 4; ++i) sum += q[i] * q[i];

            var magnitudeInverse = 1 / Mathf.Sqrt(sum);

            for (var i = 0; i < 4; ++i) q[i] *= magnitudeInverse;
        }
        #endregion
    }
}
