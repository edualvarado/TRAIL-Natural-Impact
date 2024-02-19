using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(MxMBlendSpace))]
    public class MxMBlendSpaceInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Blend Space", curHeight);

            if (GUILayout.Button("Delete Asset"))
            {
                if (EditorUtility.DisplayDialog("Delete Asseet",
                    "Are you sure? This cannot be reversed", "Yes", "Cancel"))
                {
                    DestroyImmediate(serializedObject.targetObject, true);
                }
            }
        }

    }//End of class: MxMBLendSpaceInspector
}//End of namespace MxMEditor