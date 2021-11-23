using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;
using UnityEngine;

namespace wovr.ContentFramework
{
    [CustomEditor(typeof(Item))]
	public class ItemEditor : Editor
	{
        private enum ColliderType { None, BoxCollider, MeshCollider, SphereCollider }
        private ColliderType m_ColliderType;

        private Item m_Item;
        private TagManager m_Manager;
        private Rigidbody m_RigidBody;

        /// <summary>
        /// Load values.
        /// </summary>
        private void OnEnable()
        {
            m_Item = (Item)target;
            m_Manager = m_Item.TagManager;
            if (m_Manager == null)
            {
                m_Manager = TagManager.Load();
                m_Item.TagManager = m_Manager;
                EditorUtility.SetDirty(m_Item);
            }
        }

        /// <summary>
        /// Check the dependencies for this item.
        /// </summary>
        private void CheckDependencies()
        {
            var collider = m_Item.GetComponent<Collider>();
            m_ColliderType = ColliderType.None;

            if (collider == null)
                m_ColliderType = ColliderType.None;
            else if (collider is BoxCollider)
                m_ColliderType = ColliderType.BoxCollider;
            else if (collider is MeshCollider)
                m_ColliderType = ColliderType.MeshCollider;
            else if (collider is SphereCollider)
                m_ColliderType = ColliderType.SphereCollider;

            m_RigidBody = m_Item.GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Draw the gui.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_Item.ID = EditorGUILayout.Popup("Tag", m_Item.ID, m_Manager.TagList.ToArray());

            CheckDependencies();

            EditorGUI.BeginChangeCheck();
            m_ColliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider", m_ColliderType);
            if (EditorGUI.EndChangeCheck())
                AddCollider();

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Toggle("Rigid Body", m_RigidBody != null);
            if (EditorGUI.EndChangeCheck())
                AddRigidBody();

            if (m_RigidBody != null)
                DrawRigidBodyEditor();
            
            serializedObject.Update();
            var interactable = m_Item.GetComponent<XRBaseInteractable>();
            var network = serializedObject.FindProperty("NetworkSync");
            if (interactable == null)
                EditorGUILayout.PropertyField(network);
            else
            {
                network.boolValue = false;

                GUI.enabled = false;
                EditorGUILayout.PropertyField(network, new GUIContent("NetworkSync", "XR Interactables are already in sync!"));
                GUI.enabled = true;
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw the editor stuff for the rigid body.
        /// </summary>
        private void DrawRigidBodyEditor()
        {
            m_RigidBody.useGravity = EditorGUILayout.Toggle("Use Gravity", m_RigidBody.useGravity);
        }

        /// <summary>
        /// Add rigid body to this item.
        /// </summary>
        private void AddRigidBody()
        {
            if (m_RigidBody != null)
                DestroyImmediate(m_RigidBody);
            else
                m_Item.gameObject.AddComponent<Rigidbody>();
            EditorUtility.SetDirty(m_Item);
        }

        /// <summary>
        /// Add a new collider.
        /// </summary>
        private void AddCollider()
        {
            var collider = m_Item.GetComponent<Collider>();
            if (collider != null)
                DestroyImmediate(collider);

            switch (m_ColliderType)
            {
                case ColliderType.BoxCollider:
                    collider = m_Item.gameObject.AddComponent<BoxCollider>();
                    break;
                case ColliderType.MeshCollider:
                    collider = m_Item.gameObject.AddComponent<MeshCollider>();
                    ((MeshCollider)collider).convex = true;
                    break;
                case ColliderType.SphereCollider:
                    collider = m_Item.gameObject.AddComponent<SphereCollider>();
                    break;
            }

            if (collider != null)
                collider.isTrigger = true;
            EditorUtility.SetDirty(m_Item);
        }
    }
}
