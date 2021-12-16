using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace wovr
{
    /// <summary>
    /// This call is the type used by any other code to hold a reference to an object by GUID
    /// If the target object is loaded, it will be returned, otherwise, NULL will be returned.
    /// This always works in Game Objects, so calling code will need to use GetComponent<>
    /// or other methods to track down the specific objects need by any given system.
    /// Ideally this would be a struct, but we need the ISerializationCallbackReciever
    /// </summary>
    [Serializable]
    public class GuidReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        // decorate with some extra info in Editor so we can inform a user of what that GUID means
#pragma warning disable IDE0044 // Readonly-Modifizierer hinzufügen
        [SerializeField] private string m_CachedName = null;
        [SerializeField] private SceneAsset m_CachedScene = null;
#pragma warning restore IDE0044 // Readonly-Modifizierer hinzufügen
#endif
        // store our GUID in a form that Unity can save
        [SerializeField] private byte[] m_SerializedGuid = null;

        // cache the referenced Game Object if we find one for performance
        private GameObject m_CachedReference;
        private bool m_IsCacheSet;
        private Guid m_Guid;

        // Set up events to let users register to cleanup their own cached references on destroy or to cache off values
        public event Action<GameObject> OnGuidAdded = delegate (GameObject go) { };
        public event Action OnGuidRemoved = delegate () { };

        // create concrete delegates to avoid boxing. 
        // When called 10,000 times, boxing would allocate ~1MB of GC Memory
        private Action<GameObject> addDelegate;
        private Action removeDelegate;

        /// <summary>
        /// Optimized accessor, and ideally the only code you ever call on this class
        /// </summary>
        public GameObject gameObject
        {
            get
            {
                if (m_IsCacheSet)
                    return m_CachedReference;

                m_CachedReference = GuidManager.ResolveGuid(m_Guid, addDelegate, removeDelegate);
                m_IsCacheSet = true;
                return m_CachedReference;
            }

            private set { }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GuidReference() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target"></param>
        public GuidReference(GuidComponent target)
        {
            m_Guid = target.GetGuid();
        }

        /// <summary>
        /// Add a new guid.
        /// </summary>
        /// <param name="go"></param>
        private void GuidAdded(GameObject go)
        {
            m_CachedReference = go;
            OnGuidAdded(go);
        }

        /// <summary>
        /// Remove the guid.
        /// </summary>
        private void GuidRemoved()
        {
            m_CachedReference = null;
            m_IsCacheSet = false;
            OnGuidRemoved();
        }

        /// <summary>
        /// Convert system guid to a format unity likes to work with.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_SerializedGuid = m_Guid.ToByteArray();
        }

        /// <summary>
        /// Convert from byte array to system guid and reset state.
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_CachedReference = null;
            m_IsCacheSet = false;
            if (m_SerializedGuid == null || m_SerializedGuid.Length != 16)
                m_SerializedGuid = new byte[16];

            m_Guid = new Guid(m_SerializedGuid);
            addDelegate = GuidAdded;
            removeDelegate = GuidRemoved;
        }
    }
}