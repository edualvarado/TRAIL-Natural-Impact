using UnityEngine;
using UnityEditor;
using EditorUtil;
using MxM;

namespace MxMEditor
{
    [CustomEditor(typeof(MxMInputProfile))]
    public class MxMInputProfileInspector : Editor
    {
        private SerializedProperty m_spViableInputs;

        public void OnEnable()
        {
            m_spViableInputs = serializedObject.FindProperty("m_viableInputs");
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = EditorFunctions.DrawTitle("MxM Input Profile", curHeight);

#if UNITY_2019_4_OR_NEWER
            Texture deleteIcon = EditorGUIUtility.IconContent("P4_DeletedLocal").image;
#else
            Texture deleteIcon = EditorGUIUtility.IconContent("d_P4_DeletedLocal").image;
#endif
            GUIStyle invisiButton = new GUIStyle(GUI.skin.label);

            int deleteIndex = -1;

            for (int i = 0; i < m_spViableInputs.arraySize; ++i)
            {
                SerializedProperty spViableInput = m_spViableInputs.GetArrayElementAtIndex(i);
                SerializedProperty spMin = spViableInput.FindPropertyRelative("minInput");
                SerializedProperty spMax = spViableInput.FindPropertyRelative("maxInput");
                SerializedProperty spRemapInput = spViableInput.FindPropertyRelative("viableInput");
                SerializedProperty spPosBias = spViableInput.FindPropertyRelative("posBias");
                SerializedProperty spDirBias = spViableInput.FindPropertyRelative("dirBias");

                Rect areaRect = new Rect(0f, curHeight, EditorGUIUtility.currentViewWidth, 18f * 8f + 5f);
                

                GUILayout.BeginArea(areaRect);
                GUI.Box(new Rect(0f, 0f, areaRect.width, areaRect.height), "");

                GUILayout.Label("Remap Set " + i.ToString(), EditorStyles.boldLabel);

                Rect btnRect = new Rect(areaRect.width - 20f, 2f, 18f, 18f);
                GUI.DrawTexture(btnRect, deleteIcon);
                if (GUI.Button(btnRect, "", invisiButton))
                {
                    if (EditorUtility.DisplayDialog("Delete Remap Set?",
                        "Are you sure you want to delete the track?", "Yes", "Cancel"))
                    {
                        deleteIndex = i;
                    }
                }

                GUILayout.Label("Input Range: ", GUILayout.Width(90f));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                GUILayout.Label("Min", GUILayout.Width(110f));

                spMin.floatValue = EditorGUILayout.FloatField(spMin.floatValue, GUILayout.Width(39f));

                float min = spMin.floatValue;
                float max = spMax.floatValue;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 2f);
                if(EditorGUI.EndChangeCheck())
                {
                    spMin.floatValue = min;
                    spMax.floatValue = max;
                }

                spMax.floatValue = EditorGUILayout.FloatField(spMax.floatValue, GUILayout.Width(39f));

                GUILayout.Space(10f);
                EditorGUILayout.EndHorizontal();


                GUILayout.Label("Viable Input: ", GUILayout.Width(80f));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                GUILayout.Label("Magnitude", GUILayout.Width(110f));
                spRemapInput.floatValue = EditorGUILayout.Slider(spRemapInput.floatValue, 0f, 2f);
                GUILayout.Space(10f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                GUILayout.Label("PosBias Multiplier", GUILayout.Width(110f));
                spPosBias.floatValue = EditorGUILayout.Slider(spPosBias.floatValue, 0f, 2f);
                GUILayout.Space(10f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30f);
                GUILayout.Label("DirBias Multiplier", GUILayout.Width(110f));
                spDirBias.floatValue = EditorGUILayout.Slider(spDirBias.floatValue, 0f, 2f);
                GUILayout.Space(10f);
                EditorGUILayout.EndHorizontal();

                GUILayout.EndArea();
                curHeight += areaRect.height + 10f;
                GUILayout.Space(areaRect.height + 10f);
            }

            if(deleteIndex > -1 && deleteIndex < m_spViableInputs.arraySize)
            {
                m_spViableInputs.DeleteArrayElementAtIndex(deleteIndex);
                Repaint();
            }

            curHeight += 50f;

            if (GUILayout.Button("New Viable Input"))
            {
                m_spViableInputs.InsertArrayElementAtIndex(m_spViableInputs.arraySize);
                SerializedProperty spNewViableInput = m_spViableInputs.GetArrayElementAtIndex(m_spViableInputs.arraySize - 1);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}