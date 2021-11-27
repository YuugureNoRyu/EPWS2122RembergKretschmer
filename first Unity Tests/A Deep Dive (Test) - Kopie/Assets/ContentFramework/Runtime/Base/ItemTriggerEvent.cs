using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using UnityEngine;

namespace wovr.ContentFramework
{
    [ActionCategory(ActionCategory.Physics)]
    public class ItemTriggerEvent : TriggerEvent
    {
        public TagManager TagManager;
        public bool NetworkSync = true;
        private int m_ColliderId = -1;

        public override void Awake()
        {
            base.Awake();
            if (TagManager != null)
                m_ColliderId = TagManager.TagList.IndexOf(collideTag.Value);
            else
                Debug.LogError("Please add a new TagManager with Create/WoVR/ContentFramework/TagManager");
        }

        public override void Reset()
        {
            base.Reset();
            m_ColliderId = -1;
        }

#if UNITY_EDITOR
        public override void OnGUI()
        {
            base.OnGUI();
            if (TagManager != null)
            {
                var list = TagManager.TagList;
                if (!list.Contains(collideTag.Value))
                    list.Add(collideTag.Value);
                m_ColliderId = list.IndexOf(collideTag.Value);
            }
            else
                TagManager = TagManager.Load();
        }
#endif

        protected override void TriggerEnter(Collider other)
        {
            if ((!NetworkSync || ContentNetworking.IsMaster()) && trigger == TriggerType.OnTriggerEnter)
            {
                var tagged = other.GetComponent<Item>();
                if (tagged != null && tagged.ID == m_ColliderId)
                {
                    StoreCollisionInfo(other);
                    if (NetworkSync)
                        SendCustomEvent();
                    Fsm.Event(sendEvent);
                }
            }
        }
        protected override void TriggerStay(Collider other)
        {
            if ((!NetworkSync || ContentNetworking.IsMaster()) && trigger == TriggerType.OnTriggerStay)
            {
                var tagged = other.GetComponent<Item>();
                if (tagged != null && tagged.ID == m_ColliderId)
                {
                    StoreCollisionInfo(other);
                    if (NetworkSync)
                        SendCustomEvent();
                    Fsm.Event(sendEvent);
                }
            }
        }

        protected override void TriggerExit(Collider other)
        {
            if ((!NetworkSync || ContentNetworking.IsMaster()) && trigger == TriggerType.OnTriggerExit)
            {
                var tagged = other.GetComponent<Item>();
                if (tagged != null && tagged.ID == m_ColliderId)
                {
                    StoreCollisionInfo(other);
                    if (NetworkSync)
                        SendCustomEvent();
                    Fsm.Event(sendEvent);
                }
            }
        }

        protected override void TriggerCustomEvent()
        {
            Fsm.Event(sendEvent);
            base.TriggerCustomEvent();
        }
    }
}
