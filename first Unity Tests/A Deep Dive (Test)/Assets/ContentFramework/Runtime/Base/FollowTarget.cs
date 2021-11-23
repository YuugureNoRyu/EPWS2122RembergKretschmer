using UnityEngine;

namespace wovr.ContentFramework
{
    public class FollowTarget : MonoBehaviour
    {
        private Transform m_Target;

        /// <summary>
        /// Follow the target every frame.
        /// </summary>
        /// <param name="target"></param>
        internal void Follow(Transform target)
        {
            m_Target = target;
        }

        /// <summary>
        /// Updates every frame.
        /// </summary>
        private void LateUpdate()
        {
            if (m_Target != null)
            {
                transform.position = m_Target.position;
                transform.rotation = m_Target.rotation;
            }
        }
    }
}
