using UnityEngine;

namespace Cinemachine
{
    /// <summary>
    /// An add-on module for Cinemachine Virtual Camera that post-processes
    /// the final position of the virtual camera. Pushes the camera out of intersecting colliders.
    /// </summary>
    [DocumentationSorting(DocumentationSortingAttribute.Level.UserRef)]
    [ExecuteInEditMode]
    [AddComponentMenu("")] // Hide in menu
    [SaveDuringPlay]
    public class SimpleVcamCollider : CinemachineExtension
    {
        /// <summary>The Unity layer mask against which the collider will raycast.</summary>
        [Tooltip("The Unity layer mask against which the collider will raycast")]
        public LayerMask m_CollideAgainst = 1;

        /// <summary>Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag</summary>
        [TagField]
        [Tooltip("Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
        public string m_IgnoreTag = string.Empty;

        /// <summary>
        /// Camera will try to maintain this distance from any obstacle.
        /// </summary>
        [Tooltip("Camera will try to maintain this distance from any obstacle.")]
        public float m_CameraRadius = 0.1f;

        private void OnValidate()
        {
            m_CameraRadius = Mathf.Max(0, m_CameraRadius);
        }

        /// <summary>Cleanup</summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupCameraCollider();
        }

        /// <summary>Callcack to to the collision resolution and shot evaluation</summary>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            // Move the body before the Aim is calculated
            if (stage == CinemachineCore.Stage.Body)
            {
                Vector3 displacement = RespectCameraRadius(state.RawPosition);
                state.PositionCorrection += displacement;
            }
        }

        private Collider[] mColliderBuffer = new Collider[5];
        private SphereCollider mCameraCollider;
        private GameObject mCameraColliderGameObject;
        private Vector3 RespectCameraRadius(Vector3 cameraPos)
        {
            Vector3 result = Vector3.zero;
            int numObstacles = Physics.OverlapSphereNonAlloc(
                cameraPos, m_CameraRadius, mColliderBuffer,
                m_CollideAgainst, QueryTriggerInteraction.Ignore);
            if (numObstacles > 0)
            {
                if (mCameraColliderGameObject == null)
                {
                    mCameraColliderGameObject = new GameObject("SimpleCollider Collider");
                    mCameraColliderGameObject.hideFlags = HideFlags.HideAndDontSave;
                    mCameraColliderGameObject.transform.position = Vector3.zero;
                    mCameraColliderGameObject.SetActive(true);
                    mCameraCollider = mCameraColliderGameObject.AddComponent<SphereCollider>();
                    var rb = mCameraColliderGameObject.AddComponent<Rigidbody>();
                    rb.detectCollisions = false;
                    rb.isKinematic = true;
                }
                mCameraCollider.radius = m_CameraRadius;
                for (int i = 0; i < numObstacles; ++i)
                {
                    Collider c = mColliderBuffer[i];
                    if (m_IgnoreTag.Length > 0 && c.CompareTag(m_IgnoreTag))
                        continue;
                    Vector3 dir;
                    float distance;
                    if (Physics.ComputePenetration(
                        mCameraCollider, cameraPos, Quaternion.identity,
                        c, c.transform.position, c.transform.rotation,
                        out dir, out distance))
                    {
                        result += dir * distance;   // naive, but maybe enough
                    }
                }
            }
            return result;
        }

        private void CleanupCameraCollider()
        {
            if (mCameraColliderGameObject != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(mCameraColliderGameObject);
                else
                    DestroyImmediate(mCameraColliderGameObject);
#else
                UnityEngine.Object.Destroy(obj);
#endif
            }
            mCameraColliderGameObject = null;
            mCameraCollider = null;
        }
    }
}