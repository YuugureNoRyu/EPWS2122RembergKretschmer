using UnityEngine;

#if TMP_EXISTS
namespace wovr.ContentFramework
{
    [RequireComponent(typeof(TMPro.TextMeshPro))]
	public class NamePlate : MonoBehaviour
	{
        private Transform m_Target = null;
        private Vector3 m_RotationMask = new Vector3(0, 1, 0);
        private TMPro.TextMeshPro m_NamePlate = null;

        /// <summary>
        /// Show this nameplate
        /// </summary>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public void ShowName(string name, Transform target)
        {
            if (m_NamePlate == null)
                m_NamePlate = GetComponent<TMPro.TextMeshPro>();

            var split = name.Split('_');
            m_NamePlate.text = split[0];
            m_Target = target;
        }

        /// <summary>
        /// Update the rotation.
        /// </summary>
        private void LateUpdate()
        {
            if (m_Target != null)
            {
                var lookAtRotation = Quaternion.LookRotation(transform.position - m_Target.position).eulerAngles;
                transform.rotation = Quaternion.Euler(Vector3.Scale(lookAtRotation, m_RotationMask));
            }
        }
    }
}
#endif
