using UnityEngine;
using UnityEditor;

namespace MxMEditor
{
    [CustomEditor(typeof(MotionTimingPresets))]
    public class MotionTimingPresetInspector : Editor
    {
        private SerializedProperty m_spDefenitions;

        private void OnEnable()
        {
            m_spDefenitions = serializedObject.FindProperty("m_defenitions");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorUtil.EditorFunctions.DrawTitle("MxM Motion Presets", curHeight);

            if (GUILayout.Button("Delete Asset"))
            {
                if (EditorUtility.DisplayDialog("Delete Asseet",
                    "Are you sure? This cannot be reversed", "Yes", "Cancel"))
                {
                    DestroyImmediate(serializedObject.targetObject, true);
                    return;
                }
            }

            curHeight += 23f;
            GUILayout.Space(5f);

            EditorUtil.EditorFunctions.DrawFoldout("General", curHeight, Screen.width, true);

            DrawDefaultInspector();
        }

    }//End of class: MotionTImingPresetInspector
}//End of namespace: MxMEditor