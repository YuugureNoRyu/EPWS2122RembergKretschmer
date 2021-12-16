using UnityEditor;

namespace wovr
{
    [CustomEditor(typeof(GuidComponent))]
    public class GuidComponentDrawer : Editor
    {
        private GuidComponent guidComp;

        public override void OnInspectorGUI()
        {
            if (guidComp == null)
                guidComp = (GuidComponent)target;
            EditorGUILayout.LabelField("Guid:", guidComp.GetGuid().ToString());
        }
    }
}