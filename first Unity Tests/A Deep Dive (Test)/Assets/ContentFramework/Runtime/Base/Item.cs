using UnityEngine;

namespace wovr.ContentFramework
{
#if GUID_EXISTS
    [RequireComponent(typeof(GuidComponent))]
#endif
    [AddComponentMenu("WoVR/Content Framework/Item")]
    public class Item : MonoBehaviour
	{
        private static int ItemCounter = 0;
        public TagManager TagManager;
        public int ID
        {
            get
            {
                return TagManager != null ? TagManager.TagList.IndexOf(m_Tag) : 0;
            }
            set
            {
                m_ID = value;
                if (TagManager != null)
                    m_Tag = TagManager.TagList[m_ID];
            }
        }

        [HideInInspector] public bool NetworkSync = false;
        [HideInInspector] [SerializeField]
        private string m_Tag;

        [SerializeField] [HideInInspector]
        private int m_ID = 0;
        private int m_ItemIndex;

        /// <summary>
        /// Init the index.
        /// </summary>
        private void Awake()
        {
            m_ItemIndex = ItemCounter;
            ItemCounter++;

            m_ID = TagManager.TagList.IndexOf(m_Tag);

            if (NetworkSync)
                ContentNetworking.AddRigidbody(gameObject);

            if (!ContentNetworking.IsMaster())
            {
                Destroy(this);
                var collider = GetComponent<Collider>();
                if (collider != null)
                    Destroy(collider);
            }
        }
    }
}
