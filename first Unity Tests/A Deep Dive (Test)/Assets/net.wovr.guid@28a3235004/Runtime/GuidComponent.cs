using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

namespace wovr
{
    /// <summary>
    /// This component gives a GameObject a stable, non-replicatable Globally Unique IDentifier.
    /// It can be used to reference a specific instance of an object no matter where it is.
    /// This can also be used for other systems, such as Save/Load game
    /// </summary>
    [ExecuteInEditMode, DisallowMultipleComponent]
    public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver
    {
        // Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
        [SerializeField] private byte[] serializedGuid;
        // System guid we use for comparison and generation
        private System.Guid m_Guid = System.Guid.Empty;

        /// <summary>
        /// Check if a guid is assigend.
        /// </summary>
        /// <returns></returns>
        public bool IsGuidAssigned()
        {
            return m_Guid != System.Guid.Empty;
        }

        /// <summary>
        /// When de-serializing or creating this component, we want to either restore our serialized GUID
        /// or create a new one.
        /// </summary>
        private void CreateGuid()
        {
            // if our serialized data is invalid, then we are a new object and need a new GUID
            if (serializedGuid == null || serializedGuid.Length != 16)
            {
#if UNITY_EDITOR
                // if in editor, make sure we aren't a prefab of some kind
                if (IsAssetOnDisk())
                    return;
                Undo.RecordObject(this, "Added GUID");
#endif
                m_Guid = System.Guid.NewGuid();
                serializedGuid = m_Guid.ToByteArray();

#if UNITY_EDITOR
                // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
                // force a save of the modified prefab instance properties
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            }
            // otherwise, we should set our system guid to our serialized guid
            else if (m_Guid == System.Guid.Empty)
                m_Guid = new System.Guid(serializedGuid);

            // register with the GUID Manager so that other components can access this
            if (m_Guid != System.Guid.Empty)
            {
                if (!GuidManager.Add(this))
                {
                    // if registration fails, we probably have a duplicate or invalid GUID, get us a new one.
                    serializedGuid = null;
                    m_Guid = System.Guid.Empty;
                    CreateGuid();
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Check if we are in a prefa editor mode.
        /// </summary>
        /// <returns></returns>
        private bool IsEditingInPrefabMode()
        {
            // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
            if (EditorUtility.IsPersistent(this))
                return true;
            else
            {
                // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
                var mainStage = StageUtility.GetMainStageHandle();
                var currentStage = StageUtility.GetStageHandle(gameObject);
                if (currentStage != mainStage)
                {
                    var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                    if (prefabStage != null)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if asset is a prefa.
        /// </summary>
        /// <returns></returns>
        private bool IsAssetOnDisk()
        {
            return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
        }
#endif

        /// <summary>
        /// We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
        /// </summary>
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // This lets us detect if we are a prefab instance or a prefab asset.
            // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                m_Guid = System.Guid.Empty;
            }
            else
#endif
            {
                if (m_Guid != System.Guid.Empty)
                    serializedGuid = m_Guid.ToByteArray();
            }
        }

        /// <summary>
        /// On load, we can go head a restore our system guid for later use
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (serializedGuid != null && serializedGuid.Length == 16)
                m_Guid = new System.Guid(serializedGuid);
        }

        /// <summary>
        /// Create a guid on awake.
        /// </summary>
        private void Awake()
        {
            CreateGuid();
        }

        /// <summary>
        /// similar to on Serialize, but gets called on Copying a Component or Applying a Prefab
        /// at a time that lets us detect what we are.
        /// </summary>
        void OnValidate()
        {
#if UNITY_EDITOR
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                m_Guid = System.Guid.Empty;
            }
            else
#endif
            {
                CreateGuid();
            }
        }

        /// <summary>
        /// Never return an invalid GUID
        /// </summary>
        /// <returns></returns>
        public System.Guid GetGuid()
        {
            if (m_Guid == System.Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
                m_Guid = new System.Guid(serializedGuid);
            return m_Guid;
        }

        /// <summary>
        /// let the manager know we are gone, so other objects no longer find this
        /// </summary>
        public void OnDestroy()
        {
            GuidManager.Remove(m_Guid);
        }
    }
}