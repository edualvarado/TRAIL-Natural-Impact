using UnityEditor;
using UnityEngine;

namespace MxMEditor
{
    [CustomEditor(typeof(MxMAnimationIdleSet))]
    public class MxMIdleSetInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Idle Set", curHeight);

            if (GUILayout.Button("Delete Asset"))
            {
                if (EditorUtility.DisplayDialog("Delete Asseet",
                    "Are you sure? This cannot be reversed", "Yes", "Cancel"))
                {
                    DestroyImmediate(serializedObject.targetObject, true);
                }
            }
        }
    }//End of class: MxMIdleSetInspector
}//End of namespace: MxMEditor