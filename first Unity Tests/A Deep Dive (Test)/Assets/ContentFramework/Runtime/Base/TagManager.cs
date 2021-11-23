using System.Collections.Generic;
using UnityEngine;

namespace wovr.ContentFramework
{
    [CreateAssetMenu(fileName = "TagManager", menuName = "WoVR/Content Framework/Tag Manager")]
	public class TagManager : ScriptableObject
	{
#if ATTRIBUTES_EXISTS
        [NaughtyAttributes.ReorderableList]
#endif
        public List<string> TagList = new List<string>();

#if UNITY_EDITOR
        private static TagManager INSTANCE;

        /// <summary>
        /// Import the unity tags.
        /// </summary>
#if ATTRIBUTES_EXISTS
        [NaughtyAttributes.Button]
#endif
        private void ImportUnityTags()
        {
            TagList = new List<string>();
            var tags = UnityEditorInternal.InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                TagList.Add(tags[i]);
            }
        }

        /// <summary>
        /// Load the internal tag manager.
        /// </summary>
        /// <returns></returns>
        public static TagManager Load()
        {
            if (INSTANCE == null)
            {
                var result = UnityEditor.AssetDatabase.FindAssets("TagManager");
                if (result.Length <= 1)
                {
                    Debug.LogError("Please add a new TagManager with Create/WoVR/ContentFramework/TagManager");
                    return null;
                }

                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(result[0]);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TagManager>(path);
                INSTANCE = asset;
            }
            return INSTANCE;
        }
#endif
    }
}
