using UnityEngine;
using UnityEditor;
using EditorUtil;

namespace MxMEditor
{
    public class CompositeCategorySettingsWindow : EditorWindow
    {
        private static int m_categoryId;
        private static CompositeCategorySettingsWindow m_inst;

        private static MxMPreProcessData m_data;
        private static AnimationModule m_moduleData;

        private static SerializedObject m_soPreProcessData;
        private static SerializedProperty m_spCategoryList;

        private static SerializedProperty m_spCategory;
        private static SerializedProperty m_spCompositeCategory;
        private static SerializedProperty m_spCategoryName;
        private static SerializedProperty m_spExtrapolate;
        private static SerializedProperty m_spFlattenTrajectory;
        private static SerializedProperty m_spRuntimeSplicing;
        private static SerializedProperty m_spIgnoreEdges;
        private static SerializedProperty m_spRequireTags;
        private static SerializedProperty m_spFavourTags;

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private static CompositeCategorySettingsWindow Inst()
        {
            if (m_inst == null)
                ShowWindow();

            return m_inst;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/

        public static void ShowWindow()
        {
            EditorWindow editorWindow = EditorWindow.GetWindow
                <CompositeCategorySettingsWindow>(true, "Composite Category Settings");

#if UNITY_2019_3_OR_NEWER
            editorWindow.minSize = new Vector2(200f, 218f);
            editorWindow.maxSize = new Vector2(200f, 218f);
#else
            editorWindow.minSize = new Vector2(200f, 200f);
            editorWindow.maxSize = new Vector2(200f, 200f);
#endif
            editorWindow.Show();

            m_inst = (CompositeCategorySettingsWindow) editorWindow;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/

        public static void SetData(SerializedObject a_soPreProcessData, MxMPreProcessData a_preProcessData, int a_categoryId)
        {
            if (a_soPreProcessData == null || a_preProcessData == null)
                return;

            m_data = a_preProcessData;
            m_moduleData = null;
            m_soPreProcessData = a_soPreProcessData;
            m_categoryId = a_categoryId;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public static void SetData(SerializedObject a_soAnimationModule, AnimationModule a_animModule, int a_categoryId)
        {
            if (a_soAnimationModule == null || a_animModule == null)
                return;

            m_data = null;
            m_moduleData = a_animModule;
            m_soPreProcessData = a_soAnimationModule;
            m_categoryId = a_categoryId;
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        private void OnGUI()
        {
            if (m_soPreProcessData == null)
            {
                Close();
                return;
            }

            m_spCategoryList = m_soPreProcessData.FindProperty("m_compositeCategories");

            if (m_spCategoryList == null)
            {
                Close();
                return;
            }

            if (m_categoryId >= m_spCategoryList.arraySize)
            {
                Close();
                return;
            }

            m_spCategory = m_spCategoryList.GetArrayElementAtIndex(m_categoryId);
            m_spCategoryName = m_spCategory.FindPropertyRelative("CatagoryName");
            m_spIgnoreEdges = m_spCategory.FindPropertyRelative("IgnoreEdges_default");
            m_spExtrapolate = m_spCategory.FindPropertyRelative("Extrapolate_default");
            m_spFlattenTrajectory = m_spCategory.FindPropertyRelative("FlattenTrajectory_default");
            m_spRuntimeSplicing = m_spCategory.FindPropertyRelative("RuntimeSplicing_default");
            m_spRequireTags = m_spCategory.FindPropertyRelative("RequireTags_default");
            m_spFavourTags = m_spCategory.FindPropertyRelative("FavourTags_default");

            EditorGUILayout.LabelField(m_spCategoryName.stringValue + " Defaults", EditorStyles.boldLabel);
            GUILayout.Space(5f);

            EditorGUI.BeginChangeCheck();
            m_spIgnoreEdges.boolValue = EditorGUILayout.ToggleLeft("IgnoreEdges", m_spIgnoreEdges.boolValue);
            if(EditorGUI.EndChangeCheck())
            {
                if(m_spIgnoreEdges.boolValue)
                {
                    m_spExtrapolate.boolValue = false;
                    m_spRuntimeSplicing.boolValue = false;
                }
            }

            EditorGUI.BeginChangeCheck();
            m_spExtrapolate.boolValue = EditorGUILayout.ToggleLeft("Extrapolate", m_spExtrapolate.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_spExtrapolate.boolValue)
                {
                    m_spIgnoreEdges.boolValue = false;
                    m_spRuntimeSplicing.boolValue = false;
                }
            }

            EditorGUI.BeginChangeCheck();
            m_spRuntimeSplicing.boolValue = EditorGUILayout.ToggleLeft("Runtime Splicing", m_spRuntimeSplicing.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_spRuntimeSplicing.boolValue)
                {
                    m_spExtrapolate.boolValue = false;
                    m_spIgnoreEdges.boolValue = false;
                }
            }

            m_spFlattenTrajectory.boolValue = EditorGUILayout.ToggleLeft("Flatten Trajectory", m_spFlattenTrajectory.boolValue);

            //float labelWidthDefault = EditorGUIUtility.labelWidth;
            //EditorGUIUtility.labelWidth = 90f;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Require", GUILayout.Width(50f));
            if (m_moduleData != null)
            {
                if(m_moduleData.TagNames != null)
                    EditorFunctions.DrawTagFlagFieldWithCustomNames(m_moduleData.TagNames.ToArray(), m_spRequireTags, 75f);
            }
            else
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(m_data.TagNames.ToArray(), m_spRequireTags, 75f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Favour", GUILayout.Width(50f));
            if (m_moduleData != null)
            {
                if(m_moduleData.FavourTagNames != null)
                    EditorFunctions.DrawTagFlagFieldWithCustomNames(m_moduleData.FavourTagNames.ToArray(), m_spFavourTags, 75f);
            }
            else
            {
                EditorFunctions.DrawTagFlagFieldWithCustomNames(m_data.FavourTagNames.ToArray(), m_spFavourTags, 75f);
            }
            EditorGUILayout.EndHorizontal();

            //m_spRequireTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField("Require Tags", (ETags)m_spRequireTags.intValue);
            //m_spFavourTags.intValue = (int)(ETags)EditorGUILayout.EnumFlagsField("Favour Tags", (ETags)m_spFavourTags.intValue);
            //EditorGUIUtility.labelWidth = labelWidthDefault;

            GUILayout.Space(5f);

            if(GUILayout.Button("Apply Require Tags"))
            {
                if(EditorUtility.DisplayDialog("Apply Require Tags", "This will apply the default require tags to all existing composites " +
                    "in this category. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplySettings(0);
                }
            }

            if(GUILayout.Button("Apply Favour Tags"))
            {
                if (EditorUtility.DisplayDialog("Apply Favour Tags", "This will apply the default favour tags to all existing composites " +
                    "in this category. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplySettings(1);
                }
            }

            if(GUILayout.Button("Apply All"))
            {
                if (EditorUtility.DisplayDialog("Apply All Settings", "This will apply all default settings to all existing composites " +
                    "in this category. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplySettings(2);
                }
            }

            m_soPreProcessData.ApplyModifiedProperties();
        }

        public void ApplySettings(int a_settingsToApply)
        {
            SerializedProperty spCompositeList = m_spCategory.FindPropertyRelative("Composites");

            if (spCompositeList == null)
                return;

            for (int i = 0; i < spCompositeList.arraySize; ++i)
            {
                SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(i);

                if (spComposite == null)
                    continue;

                MxMAnimationClipComposite composite = spComposite.objectReferenceValue as MxMAnimationClipComposite;

                if (composite == null)
                    continue;

                SerializedObject soClipComposite = new SerializedObject(composite);

                switch (a_settingsToApply)
                {
                    case 0: //Require Tags
                        {
                            SerializedProperty spRequireTags = soClipComposite.FindProperty("GlobalTags");
                            spRequireTags.intValue = m_spRequireTags.intValue;
                        }
                        break;
                    case 1: //Favour Tags
                        {
                            SerializedProperty spFavourTags = soClipComposite.FindProperty("GlobalFavourTags");
                            spFavourTags.intValue = m_spFavourTags.intValue;
                        }
                        break;
                    case 2: //All Settings
                        {
                            SerializedProperty spIgnoreEdges = soClipComposite.FindProperty("IgnoreEdges");
                            SerializedProperty spExtrapolateTrajectory = soClipComposite.FindProperty("ExtrapolateTrajectory");
                            SerializedProperty spFlattenTrajectory = soClipComposite.FindProperty("FlattenTrajectory");
                            SerializedProperty spRuntimeSplicing = soClipComposite.FindProperty("RuntimeSplicing");
                            SerializedProperty spRequireTags = soClipComposite.FindProperty("GlobalTags");
                            SerializedProperty spFavourTags = soClipComposite.FindProperty("GlobalFavourTags");

                            spIgnoreEdges.boolValue = m_spIgnoreEdges.boolValue;
                            spFlattenTrajectory.boolValue = m_spFlattenTrajectory.boolValue;
                            spRuntimeSplicing.boolValue = m_spRuntimeSplicing.boolValue;
                            spExtrapolateTrajectory.boolValue = m_spExtrapolate.boolValue;
                            spFavourTags.intValue = m_spFavourTags.intValue;
                            spRequireTags.intValue = m_spRequireTags.intValue;
                        }
                        break;
                }

                soClipComposite.ApplyModifiedProperties();
            }
        }

        public void OnLostFocus()
        {
            Close();
        }
    }
}
