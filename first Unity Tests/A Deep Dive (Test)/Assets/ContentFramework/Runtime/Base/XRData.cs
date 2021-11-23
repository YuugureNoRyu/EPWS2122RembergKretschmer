#if PHOTON_UNITY_NETWORKING 
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
#endif

using UnityEngine.XR.Interaction.Toolkit;
using OVRTouchSample;
using UnityEngine;
using System;

namespace wovr.ContentFramework
{
    [RequireComponent(typeof(OVRCameraRig))]
    [RequireComponent(typeof(MasterController))]
#if PHOTON_UNITY_NETWORKING
	public class XRData : MonoBehaviour, IOnEventCallback
#else
	public class XRData : MonoBehaviour
#endif
    {
        private static readonly byte PLAYER_SYNC_EVENT = 105;
        private static readonly byte OTHER_PLAYER_SYNC_EVENT = 106;

        [SerializeField] private GameObject m_Head = null;
        [SerializeField] private GameObject m_RightAnchor = null;
        [SerializeField] private GameObject m_LeftAnchor = null;

        [Space]
        [SerializeField] private Hand m_RightHand = null;
        [SerializeField] private Hand m_LeftHand = null;

        [Space]
        [SerializeField] private GameObject[] m_PlayerItems = null;

        private GameObject m_SpawnHead;
        private GameObject m_SpawnLeftHand;
        private GameObject m_SpawnRightHand;

#if PHOTON_UNITY_NETWORKING
        /// <summary>
        /// Init values.
        /// </summary>
        private void Start()
        {
            CreateSpawnObjects();
            InitLocalPlayer();
        }
    
        /// <summary>
        /// Init the local player for multi user.
        /// </summary>
        private void InitLocalPlayer()
        {
#if GUID_EXISTS
            m_Head.AddComponent<GuidComponent>();
            m_LeftAnchor.AddComponent<GuidComponent>();
            m_RightAnchor.AddComponent<GuidComponent>();
#endif
            var player = PhotonNetwork.LocalPlayer;
            var headID = ContentNetworking.AddRigidbody(m_Head, player.ActorNumber);
            var leftID = ContentNetworking.AddRigidbody(m_LeftAnchor, player.ActorNumber);
            var rightID = ContentNetworking.AddRigidbody(m_RightAnchor, player.ActorNumber);

            var headView = m_Head.GetComponent<PhotonView>();
            var leftView = m_LeftAnchor.GetComponent<PhotonView>();
            var rightView = m_RightAnchor.GetComponent<PhotonView>();

            leftView.ObservedComponents.Add(m_LeftHand);
            rightView.ObservedComponents.Add(m_RightHand);

            if (!ContentNetworking.IsMaster())
            {
                var colliders = m_Head.transform.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].enabled = false;
            }

            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            var sending = new SendOptions { Reliability = true };

            var param = new object[] { rightID, leftID, headID, player.ActorNumber, player.NickName };
            PhotonNetwork.RaiseEvent(PLAYER_SYNC_EVENT, param, options, sending);
        }

        /// <summary>
        /// Init the player xr data.
        /// </summary>
        /// <param name="rightID"></param>
        /// <param name="leftID"></param>
        /// <param name="headID"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        private void InitOtherPlayer(int rightID, int leftID, int headID, int owner, string name)
        {
            var otherHead = Instantiate(m_SpawnHead);
            var otherLeftHand = Instantiate(m_SpawnLeftHand);
            var otherRigthHand = Instantiate(m_SpawnRightHand);
#if TMP_EXISTS
            var textmesh = otherHead.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textmesh != null)
            {
                var namePlate = textmesh.gameObject.AddComponent<NamePlate>();
                namePlate.ShowName(name, m_Head.transform);
            }
#endif
            otherHead.name = "Head Player {" + otherHead + "}";
            otherLeftHand.name = "LeftHandAnchor Player {" + owner + "}";
            otherRigthHand.name = "RightHandAnchor Player {" + owner + "}";

            ContentNetworking.AddRigidbody(otherHead, owner, headID);
            ContentNetworking.AddRigidbody(otherLeftHand, owner, leftID);
            ContentNetworking.AddRigidbody(otherRigthHand, owner, rightID);

            var leftView = otherLeftHand.GetComponent<PhotonView>();
            var rightView = otherRigthHand.GetComponent<PhotonView>();

            var speakers = FindObjectsOfType<Photon.Voice.Unity.Speaker>();
            for (int i = 0; i < speakers.Length; i++)
            {
                var speaker = speakers[i];
                var actor = speaker.Actor;

                if (actor != null && actor.ActorNumber == owner)
                {
                    var helper = speaker.gameObject.AddComponent<FollowTarget>();
                    helper.Follow(otherHead.transform);
                }
            }

            leftView.ObservedComponents.Add(otherLeftHand.transform.GetComponentInChildren<Hand>());
            rightView.ObservedComponents.Add(otherRigthHand.transform.GetComponentInChildren<Hand>());

            otherHead.SetActive(true);
            otherLeftHand.SetActive(true);
            otherRigthHand.SetActive(true);
        }
        
        /// <summary>
        /// Send the own hand state.
        /// </summary>
        /// <param name="owner"></param>
        private void SendStateBack(int owner)
        {
            var player = PhotonNetwork.LocalPlayer;
            var options = new RaiseEventOptions { TargetActors = new int[] { owner } };
            var sending = new SendOptions { Reliability = true };

            var headID = m_Head.GetComponent<PhotonView>().ViewID;
            var rightID = m_RightAnchor.GetComponent<PhotonView>().ViewID;
            var leftID = m_LeftAnchor.GetComponent<PhotonView>().ViewID;

            var param = new object[] { rightID, leftID, headID, player.ActorNumber, player.NickName };
            PhotonNetwork.RaiseEvent(OTHER_PLAYER_SYNC_EVENT, param, options, sending);
        }

        /// <summary>
        /// Create spawn handy to reference all player with.
        /// </summary>
        private void CreateSpawnObjects()
        {
            int[] sibling = null;
            Transform[] parents = null;

            if (m_PlayerItems != null)
            {
                sibling = new int[m_PlayerItems.Length];
                parents = new Transform[m_PlayerItems.Length];
                for (int i = 0; i < m_PlayerItems.Length; i++)
                {
                    parents[i] = m_PlayerItems[i].transform.parent;
                    sibling[i] = m_PlayerItems[i].transform.GetSiblingIndex();

                    m_PlayerItems[i].transform.SetParent(null);
                }
            }

            var template = " Template";
            m_SpawnHead = Instantiate(m_Head);
            m_SpawnHead.name = m_Head.name + template;
            m_SpawnLeftHand = Instantiate(m_LeftAnchor);
            m_SpawnLeftHand.name = m_SpawnLeftHand.name + template;
            m_SpawnRightHand = Instantiate(m_RightAnchor);
            m_SpawnRightHand.name = m_SpawnRightHand + template;

            if (m_PlayerItems != null)
            {
                for (int i = 0; i < m_PlayerItems.Length; i++)
                {
                    m_PlayerItems[i].transform.SetParent(parents[i]);
                    m_PlayerItems[i].transform.SetSiblingIndex(sibling[i]);
                }
            }

            var types = new Type[]
            {
                typeof(Item),
                typeof(Collider),
#if GUID_EXISTS
                typeof(GuidComponent),
#endif
                typeof(XRInteractorLineVisual),
                typeof(XRBaseControllerInteractor)
            };

            FindAndDestroyComponentsInChildren(m_SpawnHead.transform, types);
            FindAndDestroyComponentsInChildren(m_SpawnLeftHand.transform, types);
            FindAndDestroyComponentsInChildren(m_SpawnRightHand.transform, types);

            m_SpawnHead.tag = "Untagged";
            Destroy(m_SpawnHead.GetComponent<Camera>());
            Destroy(m_SpawnHead.GetComponent<AudioListener>());

            m_SpawnHead.SetActive(false);
            m_SpawnLeftHand.SetActive(false);
            m_SpawnRightHand.SetActive(false);
        }

        /// <summary>
        /// Recursive search for multipe components in children.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="types"></param>
        private void FindAndDestroyComponentsInChildren(Transform parent, Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var comp = parent.GetComponent(types[i]);
                if (comp != null)
                    Destroy(comp);
            }

            for (int i = 0; i < parent.childCount; i++)
                FindAndDestroyComponentsInChildren(parent.GetChild(i), types);
        }

        /// <summary>
        /// PUN Callback.
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnEvent(EventData photonEvent)
        {
            var code = photonEvent.Code;
            if (code == PLAYER_SYNC_EVENT || code == OTHER_PLAYER_SYNC_EVENT)
            {
                var data = (object[])photonEvent.CustomData;
                var rightID = (int)data[0];
                var leftID = (int)data[1];
                var headID = (int)data[2];
                var owner = (int)data[3];

                var name = "";
                if (data.Length > 4)
                    name = (string)data[4];

                InitOtherPlayer(rightID, leftID, headID, owner, name);
                if (code == PLAYER_SYNC_EVENT)
                    SendStateBack(owner);
            }
        }

        /// <summary>
        /// Add listener.
        /// </summary>
        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        /// <summary>
        /// Remove listener.
        /// </summary>
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
#endif
    }
}
