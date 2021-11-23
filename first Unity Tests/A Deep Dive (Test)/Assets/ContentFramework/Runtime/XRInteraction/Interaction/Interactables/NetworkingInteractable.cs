using System.Collections.Generic;
using UnityEngine;

namespace wovr.ContentFramework
{
#if GUID_EXISTS
    [RequireComponent(typeof(GuidComponent))]
#endif
#if PHOTON_UNITY_NETWORKING
    public class NetworkingInteractable : MonoBehaviour, Photon.Pun.IPunObservable
#else
        public class NetworkingInteractable : MonoBehaviour
#endif
    {
        [SerializeField] private bool m_IsSyncing = true;

        /// <summary>
        /// Add the component to serialize.
        /// </summary>
        protected virtual void Start()
        {
            if (!m_IsSyncing)
                return;

            ContentNetworking.AddPhotonView(gameObject, new List<Component> { this });
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody != null && !ContentNetworking.IsMaster())
                rigidbody.useGravity = false;
        }

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Serialize the stream.
        /// </summary>
        /// <param name="stream"></param>
        protected virtual void SerializeView(Photon.Pun.PhotonStream stream) { }

        /// <summary>
        /// Serialize the hand animations.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(Photon.Pun.PhotonStream stream, Photon.Pun.PhotonMessageInfo info)
        {
            if (m_IsSyncing)
                SerializeView(stream);
        }
#endif
    }
}
