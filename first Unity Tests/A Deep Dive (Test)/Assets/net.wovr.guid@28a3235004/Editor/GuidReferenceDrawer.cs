using UnityEditor;
using UnityEngine;

namespace wovr
{
    /// <summary>
    /// Using a property drawer to allow any class to have a field of type GuidRefernce and still get good UX
    /// If you are writing your own inspector for a class that uses a GuidReference, drawing it with
    /// EditorLayout.PropertyField(prop) or similar will get this to show up automatically
    /// </summary>
    [CustomPropertyDrawer(typeof(GuidReference))]
    public class GuidReferenceDrawer : PropertyDrawer
    {
        private SerializedProperty m_GuidProp;
        private SerializedProperty m_SceneProp;
        private SerializedProperty m_NameProp;

        // cache off GUI content to avoid creating garbage every frame in editor
        private readonly GUIContent m_SceneLabel = new GUIContent("Containing Scene", "The target object is expected in this scene asset.");
        private readonly GUIContent m_ClearButtonGUI = new GUIContent("Clear", "Remove Cross Scene Reference");

        /// <summary>
        /// Add an extra line to display source scene for targets.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Draw gui.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_GuidProp = property.FindPropertyRelative("m_SerializedGuid");
            m_NameProp = property.FindPropertyRelative("m_CachedName");
            m_SceneProp = property.FindPropertyRelative("m_CachedScene");

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;

            // Draw prefix label, returning the new rect we can draw in
            var guidCompPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            System.Guid currentGuid;
            GameObject currentGO = null;

            // working with array properties is a bit unwieldy
            // you have to get the property at each index manually
            var byteArray = new byte[16];
            var arraySize = m_GuidProp.arraySize;
            for (int i = 0; i < arraySize; ++i)
            {
                var byteProp = m_GuidProp.GetArrayElementAtIndex(i);
                byteArray[i] = (byte)byteProp.intValue;
            }

            currentGuid = new System.Guid(byteArray);
            currentGO = GuidManager.ResolveGuid(currentGuid);
            var currentGuidComponent = currentGO?.GetComponent<GuidComponent>();

            GuidComponent component = null;
            if (currentGuid != System.Guid.Empty && currentGuidComponent == null)
            {
                // if our reference is set, but the target isn't loaded, we display the target and the scene it is in, and provide a way to clear the reference
                var buttonWidth = 55.0f;
                guidCompPosition.xMax -= buttonWidth;

                var guiEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.LabelField(guidCompPosition, new GUIContent(m_NameProp.stringValue, "Target GameObject is not currently loaded."), EditorStyles.objectField);
                GUI.enabled = guiEnabled;

                var clearButtonRect = new Rect(guidCompPosition) { xMin = guidCompPosition.xMax };
                clearButtonRect.xMax += buttonWidth;

                if (GUI.Button(clearButtonRect, m_ClearButtonGUI, EditorStyles.miniButton))
                    ClearPreviousGuid();
            }
            // if our object is loaded, we can simply use an object field directly
            else
                component = EditorGUI.ObjectField(guidCompPosition, currentGuidComponent, typeof(GuidComponent), true) as GuidComponent;

            if (currentGuidComponent != null && component == null)
                ClearPreviousGuid();

            // if we have a valid reference, draw the scene name of the scene it lives in so users can find it
            if (component != null)
            {
                m_NameProp.stringValue = component.name;
                var scenePath = component.gameObject.scene.path;
                m_SceneProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

                // only update the GUID Prop if something changed. This fixes multi-edit on GUID References
                if (component != currentGuidComponent)
                {
                    byteArray = component.GetGuid().ToByteArray();
                    arraySize = m_GuidProp.arraySize;
                    for (int i = 0; i < arraySize; ++i)
                    {
                        var byteProp = m_GuidProp.GetArrayElementAtIndex(i);
                        byteProp.intValue = byteArray[i];
                    }
                }
            }

            EditorGUI.indentLevel++;
            position.y += EditorGUIUtility.singleLineHeight;
            var cachedGUIState = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.ObjectField(position, m_SceneLabel, m_SceneProp.objectReferenceValue, typeof(SceneAsset), false);
            GUI.enabled = cachedGUIState;
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Clear prvious guid.
        /// </summary>
        void ClearPreviousGuid()
        {
            m_NameProp.stringValue = string.Empty;
            m_SceneProp.objectReferenceValue = null;

            var arraySize = m_GuidProp.arraySize;
            for (int i = 0; i < arraySize; ++i)
            {
                var byteProp = m_GuidProp.GetArrayElementAtIndex(i);
                byteProp.intValue = 0;
            }
        }
    }
}