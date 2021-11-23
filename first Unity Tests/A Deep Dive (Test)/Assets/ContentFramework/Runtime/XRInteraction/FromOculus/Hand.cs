/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace OVRTouchSample
{
    // Animated hand visuals for a user of a Touch controller.
#if PHOTON_UNITY_NETWORKING
    public class Hand : MonoBehaviour, Photon.Pun.IPunObservable
#else
    public class Hand : MonoBehaviour
#endif
    {
        public const string ANIM_LAYER_NAME_POINT = "Point Layer";
        public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
        public const string ANIM_PARAM_NAME_FLEX = "Flex";
        public const string ANIM_PARAM_NAME_POSE = "Pose";
        public const float THRESH_COLLISION_FLEX = 0.9f;

        public const float INPUT_RATE_CHANGE = 20.0f;

        public const float COLLIDER_SCALE_MIN = 0.01f;
        public const float COLLIDER_SCALE_MAX = 1.0f;
        public const float COLLIDER_SCALE_PER_SECOND = 1.0f;

        public const float TRIGGER_DEBOUNCE_TIME = 0.05f;
        public const float THUMB_DEBOUNCE_TIME = 0.15f;

        [SerializeField]
        private OVRInput.Controller m_controller = OVRInput.Controller.None;
        [SerializeField]
        private Animator m_animator = null;
        [SerializeField]
        private HandPose m_defaultGrabPose = null;

        private Collider[] m_colliders = null;
        private bool m_collisionEnabled = true;

        List<Renderer> m_showAfterInputFocusAcquired;

        private int m_animLayerIndexThumb = -1;
        private int m_animLayerIndexPoint = -1;
        private int m_animParamIndexFlex = -1;
        private int m_animParamIndexPose = -1;
        private bool m_isPointing = false;
        private bool m_isGivingThumbsUp = false;
        private float m_pointBlend = 0.0f;
        private float m_thumbsUpBlend = 0.0f;

        private bool m_restoreOnInputAcquired = false;

        // Values to sync over networking.
        private float m_IsFlex;
        private float m_Point;
        private float m_IsPinch;
#if PHOTON_UNITY_NETWORKING
        private Photon.Pun.PhotonView m_View;
#endif

        private void Start()
        {
            m_showAfterInputFocusAcquired = new List<Renderer>();

            // Collision starts disabled. We'll enable it for certain cases such as making a fist.
            m_colliders = this.GetComponentsInChildren<Collider>().Where(childCollider => !childCollider.isTrigger).ToArray();
            CollisionEnable(false);

            // Get animator layer indices by name, for later use switching between hand visuals
            m_animLayerIndexPoint = m_animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
            m_animLayerIndexThumb = m_animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
            m_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
            m_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);

#if PHOTON_UNITY_NETWORKING
            m_View = transform.parent.GetComponent<Photon.Pun.PhotonView>();
            if (m_View != null && !m_View.IsMine)
                return;
#endif
            OVRManager.InputFocusAcquired += OnInputFocusAcquired;
            OVRManager.InputFocusLost += OnInputFocusLost;
#if UNITY_EDITOR
            OVRPlugin.SendEvent("custom_hand", (SceneManager.GetActiveScene().name == "CustomHands").ToString(), "sample_framework");
#endif
        }

        private void OnDestroy()
        {
            OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
            OVRManager.InputFocusLost -= OnInputFocusLost;
        }

        private void Update()
        {
            UpdateCapTouchStates();

            m_pointBlend = InputValueRateChange(m_isPointing, m_pointBlend);
            m_thumbsUpBlend = InputValueRateChange(m_isGivingThumbsUp, m_thumbsUpBlend);

            float flex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);

            bool collisionEnabled = flex >= THRESH_COLLISION_FLEX;
            CollisionEnable(collisionEnabled);
            
            UpdateAnimValues();
            UpdateAnimStates();
        }

        /// <summary>
        /// Update animation values.
        /// </summary>
        private void UpdateAnimValues()
        {
#if PHOTON_UNITY_NETWORKING
            if (m_View != null && !m_View.IsMine)
                return;
#endif
            m_IsFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
            m_IsPinch = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);

            var handPose = GetHandPose();
            var isPointing = handPose.AllowPointing;
            m_Point = isPointing ? m_pointBlend : 0.0f;
            var isThumbsUp = handPose.AllowThumbsUp;
            m_ThumbsUp = isThumbsUp ? m_thumbsUpBlend : 0.0f;
        }

        /// <summary>
        /// Update animation states with values.
        /// </summary>
        private void UpdateAnimStates()
        {
            var handPose = GetHandPose();
            var handPoseId = handPose.PoseId;

            m_animator.SetInteger(m_animParamIndexPose, (int)handPoseId);
            m_animator.SetFloat(m_animParamIndexFlex, m_IsFlex);
            m_animator.SetLayerWeight(m_animLayerIndexPoint, m_Point);
            m_animator.SetLayerWeight(m_animLayerIndexThumb, m_ThumbsUp);
            m_animator.SetFloat("Pinch", m_IsPinch);
        }

        private HandPose GetHandPose()
        {
            var grabPose = m_defaultGrabPose;
            return grabPose;
        }

        // Just checking the state of the index and thumb cap touch sensors, but with a little bit of
        // debouncing.
        private void UpdateCapTouchStates()
        {
            m_isPointing = !OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, m_controller);
            m_isGivingThumbsUp = !OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, m_controller);
        }

        private void LateUpdate()
        {
            // Hand's collision grows over a short amount of time on enable, rather than snapping to on, to help somewhat with interpenetration issues.
            if (m_collisionEnabled && m_collisionScaleCurrent + Mathf.Epsilon < COLLIDER_SCALE_MAX)
            {
                m_collisionScaleCurrent = Mathf.Min(COLLIDER_SCALE_MAX, m_collisionScaleCurrent + Time.deltaTime * COLLIDER_SCALE_PER_SECOND);
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    collider.transform.localScale = new Vector3(m_collisionScaleCurrent, m_collisionScaleCurrent, m_collisionScaleCurrent);
                }
            }
        }

        // Simple Dash support. Just hide the hands.
        private void OnInputFocusLost()
        {
            if (gameObject.activeInHierarchy)
            {
                m_showAfterInputFocusAcquired.Clear();
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    if (renderers[i].enabled)
                    {
                        renderers[i].enabled = false;
                        m_showAfterInputFocusAcquired.Add(renderers[i]);
                    }
                }

                CollisionEnable(false);

                m_restoreOnInputAcquired = true;
            }
        }

        private void OnInputFocusAcquired()
        {
            if (m_restoreOnInputAcquired)
            {
                for (int i = 0; i < m_showAfterInputFocusAcquired.Count; ++i)
                {
                    if (m_showAfterInputFocusAcquired[i])
                    {
                        m_showAfterInputFocusAcquired[i].enabled = true;
                    }
                }
                m_showAfterInputFocusAcquired.Clear();

                // Update function will update this flag appropriately. Do not set it to a potentially incorrect value here.
                //CollisionEnable(true);

                m_restoreOnInputAcquired = false;
            }
        }

        private float InputValueRateChange(bool isDown, float value)
        {
            float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
            float sign = isDown ? 1.0f : -1.0f;
            return Mathf.Clamp01(value + rateDelta * sign);
        }

        private float m_collisionScaleCurrent = 0.0f;
        private float m_ThumbsUp;

        private void CollisionEnable(bool enabled)
        {
            if (m_collisionEnabled == enabled)
            {
                return;
            }
            m_collisionEnabled = enabled;

            if (enabled)
            {
                m_collisionScaleCurrent = COLLIDER_SCALE_MIN;
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    collider.transform.localScale = new Vector3(COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN);
                    collider.enabled = true;
                }
            }
            else
            {
                m_collisionScaleCurrent = COLLIDER_SCALE_MAX;
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    collider.enabled = false;
                    collider.transform.localScale = new Vector3(COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN);
                }
            }
        }

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Serialize the hand animations.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(Photon.Pun.PhotonStream stream, Photon.Pun.PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(m_IsFlex);
                stream.SendNext(m_Point);
                stream.SendNext(m_ThumbsUp);
                stream.SendNext(m_IsPinch);
            }
            else
            {
                m_IsFlex = (float)stream.ReceiveNext();
                m_Point = (float)stream.ReceiveNext();
                m_ThumbsUp = (float)stream.ReceiveNext();
                m_IsPinch = (float)stream.ReceiveNext();
            }
        }
#endif
    }
}
