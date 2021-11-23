#if PHOTON_UNITY_NETWORKING 
using Photon.Pun;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace wovr.ContentFramework
{
    [DefaultExecutionOrder(-99)]
    [AddComponentMenu("WoVR/Content Framework/Networking")]
    public static class ContentNetworking
	{
#if PHOTON_UNITY_NETWORKING
        private static int VIEW_INDEX = 0;
        private static PhotonView m_View;
#endif

        /// <summary>
        /// Check if the player is the master.
        /// </summary>
        /// <returns></returns>
        public static bool IsMaster()
        {
            var isMaster = true;
#if PHOTON_UNITY_NETWORKING
            if (PhotonNetwork.CurrentRoom == null)
                isMaster = true;
            else
                isMaster = PhotonNetwork.LocalPlayer.IsMasterClient;
#endif
            return isMaster;
        }

        /// <summary>
        /// Add a new photon view to this go.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="components"></param>
        public static void AddPhotonView(GameObject go, List<Component> components)
        {
#if PHOTON_UNITY_NETWORKING
            if (PhotonNetwork.CurrentRoom == null)
                return;
            var view = AddPhotonView(go);
            view.ObservedComponents = components;
#endif
        }

        /// <summary>
        /// Add rigidbody with a custom id.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="customID"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static int AddRigidbody(GameObject go, int owner = 0, int customID = 0)
        {
#if PHOTON_UNITY_NETWORKING
            if (PhotonNetwork.CurrentRoom == null)
                return 0;

            var view = AddPhotonView(go, owner, customID);
            var tf = go.AddComponent<PhotonRigidbodyView>();

            tf.m_SynchronizeVelocity = true;
            tf.m_TeleportEnabled = true;
            tf.m_SynchronizeAngularVelocity = true;

            view.ObservedComponents = new List<Component> { tf };
            var isMaster = PhotonNetwork.LocalPlayer.IsMasterClient;
            if (!isMaster)
            {
                var rigid = go.GetComponent<Rigidbody>();
                if (rigid != null)
                    rigid.useGravity = false;
            }
            return view.ViewID;
#else
            return 0;
#endif
        }

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Add a photon view to this gameObject.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="owner"></param>
        /// <param name="customID"></param>
        /// <returns></returns>
        private static PhotonView AddPhotonView(GameObject go, int owner = 0, int customID = 0)
        {
            var view = go.AddComponent<PhotonView>();

            if (owner == 0)
                owner = PhotonNetwork.MasterClient.ActorNumber;

            if (customID == 0)
            {
                customID = 1000 - VIEW_INDEX;
#if GUID_EXISTS
                var guid = go.GetComponent<GuidComponent>();
                if (guid != null)
                    customID = guid.GetGuid().GetHashCode();
#endif
            }

            view.ViewID = customID;
            PhotonNetwork.RegisterPhotonView(view);
            VIEW_INDEX++;

            view.OwnershipTransfer = OwnershipOption.Takeover;
            view.Synchronization = ViewSynchronization.Unreliable;
            view.TransferOwnership(owner);
            return view;
        }
#endif
    }
}
