using UnityEditor;
using UnityEngine;

namespace MxMEditor
{
    [CustomEditor(typeof(MxMAnimationClipComposite))]
    public class MxMCompositeInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Animation Composite", curHeight);

            if(GUILayout.Button("Delete Asset"))
            {
                if(EditorUtility.DisplayDialog("Delete Asseet", 
                    "Are you sure? This cannot be reversed", "Yes", "Cancel"))
                {
                    DestroyImmediate(serializedObject.targetObject, true);
                }
            }
        }

    }//End of class: MxMCompositeInspector
}//End of namespace: MxMEditor