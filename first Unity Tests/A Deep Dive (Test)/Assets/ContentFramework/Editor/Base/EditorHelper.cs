using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;
using UnityEngine;
using System;

namespace wovr.ContentFramework
{
	public static class EditorHelper
	{
        [MenuItem("WoVR/Content Framework/Init Scene")]
		private static void InitScene()
        {
            var manager = AddInteractionManager();
            AddPlayer(manager);
        }

        [MenuItem("GameObject/Content Framework/Add Player", validate = false, priority = 0)]
        private static void AddPlayerContext()
        {
            AddPlayer();
        }

        [MenuItem("GameObject/Content Framework/Add Teleporter", validate = false, priority = 1)]
        private static void AddTeleporterContext()
        {
            AddObject("Teleporter");
        }

#if GUID_EXISTS
        [MenuItem("WoVR/Content Framework/Apply GUID")]
        private static void ApplyGUID()
        {
            var interactables = GameObject.FindObjectsOfType<XRBaseInteractable>();
            var items = GameObject.FindObjectsOfType<Item>();

            for (int i = 0; i < interactables.Length; i++)
                AddGUIDComponent(interactables[i].gameObject);

            for (int i = 0; i < items.Length; i++)
                AddGUIDComponent(items[i].gameObject);
        }

        /// <summary>
        /// Add a new guid if none exists.
        /// </summary>
        /// <param name="go"></param>
        private static void AddGUIDComponent(GameObject go)
        {
            var guid = go.GetComponent<GuidComponent>();
            if (guid == null)
                guid = go.gameObject.AddComponent<GuidComponent>();
        }
#endif

        /// <summary>
        /// Add a new interaction manager to the scene.
        /// </summary>
        private static XRInteractionManager AddInteractionManager()
        {
            var manager = new GameObject("XRInteractionManager");
            return manager.AddComponent<XRInteractionManager>();
        }

        /// <summary>
        /// Add the local player to the current scene.
        /// </summary>
        /// <param name="manager"></param>
        private static void AddPlayer(XRInteractionManager manager = null)
        {
            var go = AddObject("LocalPlayer");
            if (manager != null)
            {
                var interactor = go.transform.GetComponentsInChildren<XRBaseControllerInteractor>();
                for (int i = 0; i < interactor.Length; i++)
                    interactor[i].interactionManager = manager;
            }
        }

        /// <summary>
        /// Add a new gameObject to the scene.
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        private static GameObject AddObject(string prefabName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"Packages/net.wovr.content_framework/Runtime/Assets/{prefabName}.prefab");
            var go = GameObject.Instantiate(asset, Vector3.zero, Quaternion.identity);
            go.name = prefabName;
            return go;
        }
    }
}
