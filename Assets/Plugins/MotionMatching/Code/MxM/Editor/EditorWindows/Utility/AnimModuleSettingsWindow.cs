using UnityEngine;
using UnityEditor;
using EditorUtil;
using MxM;

namespace MxMEditor
{
    public class AnimModuleSettingsWindow : EditorWindow
    {
        //Static instance
        private static AnimModuleSettingsWindow m_inst;

        //Target animation module
        private static AnimationModule m_moduleData;

        //Serialized handles
        private static SerializedObject m_soAnimModule;

        private static SerializedProperty m_spAnimModuleDefaults;

        private static SerializedProperty m_spIgnoreEdges;
        private static SerializedProperty m_spExtrapolate;
        private static SerializedProperty m_spFlattenTrajectory;
        private static SerializedProperty m_spRuntimeSplicing;

        private static SerializedProperty m_spBlendSpaceType;
        private static SerializedProperty m_spScatterSpacing;
        private static SerializedProperty m_spNormalizeBlendSpace;
        private static SerializedProperty m_spBlendSpaceMagnitude;
        private static SerializedProperty m_spBlendSpaceSmoothing;

        private static SerializedProperty m_spIdleMinLoops;
        private static SerializedProperty m_spIdleMaxLoops;

        private static SerializedProperty m_spRequireTags;
        private static SerializedProperty m_spFavourTags;

        private static AnimModuleSettingsWindow Inst()
        {
            if (m_inst == null)
                ShowWindow();

            return m_inst;
        }

        public static void ShowWindow()
        {
            EditorWindow editorWindow = EditorWindow.GetWindow
                <AnimModuleSettingsWindow>(true, "Anim Module Default Settings");

#if UNITY_2019_3_OR_NEWER
            editorWindow.minSize = new Vector2(200f, 515f);
            editorWindow.maxSize = new Vector2(200f, 515f);
#else
            editorWindow.minSize = new Vector2(200f, 470f);
            editorWindow.maxSize = new Vector2(200f, 470f);
#endif
            editorWindow.Show();

            m_inst = (AnimModuleSettingsWindow) editorWindow;
        }

        public static void SetData(SerializedObject a_soAnimModule, AnimationModule a_animModule)
        {
            if (a_soAnimModule == null || a_animModule == null)
                return;

            m_moduleData = a_animModule;
            m_soAnimModule = a_soAnimModule;
        }

        private void OnGUI()
        {
            if(m_soAnimModule == null)
            {
                Close();
                return;
            }

            m_spAnimModuleDefaults = m_soAnimModule.FindProperty("m_animModuleDefaults");

            m_spIgnoreEdges = m_spAnimModuleDefaults.FindPropertyRelative("IgnoreEdges");
            m_spExtrapolate = m_spAnimModuleDefaults.FindPropertyRelative("Extrapolate");
            m_spFlattenTrajectory = m_spAnimModuleDefaults.FindPropertyRelative("FlattenTrajectory");
            m_spRuntimeSplicing = m_spAnimModuleDefaults.FindPropertyRelative("RuntimeSplicing");

            m_spBlendSpaceType = m_spAnimModuleDefaults.FindPropertyRelative("BlendSpaceType");
            m_spScatterSpacing = m_spAnimModuleDefaults.FindPropertyRelative("ScatterSpacing");
            m_spNormalizeBlendSpace = m_spAnimModuleDefaults.FindPropertyRelative("NormalizeBlendSpace");
            m_spBlendSpaceMagnitude = m_spAnimModuleDefaults.FindPropertyRelative("BlendSpaceMagnitude");
            m_spBlendSpaceSmoothing = m_spAnimModuleDefaults.FindPropertyRelative("BlendSpaceSmoothing");

            m_spIdleMinLoops = m_spAnimModuleDefaults.FindPropertyRelative("MinLoops");
            m_spIdleMaxLoops = m_spAnimModuleDefaults.FindPropertyRelative("MaxLoops");

            m_spRequireTags = m_spAnimModuleDefaults.FindPropertyRelative("RequireTags");
            m_spFavourTags = m_spAnimModuleDefaults.FindPropertyRelative("FavourTags");

            GUIStyle applyBtnStyle = new GUIStyle(GUI.skin.button);
            applyBtnStyle.fontSize = 9;

            if (EditorGUIUtility.isProSkin)
            {
                applyBtnStyle.normal.textColor = Color.cyan;
            }
            else
            {
                applyBtnStyle.normal.textColor = Color.blue;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Composites", EditorStyles.boldLabel, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", applyBtnStyle,  GUILayout.Height(16f)))
            {
                if (EditorUtility.DisplayDialog("Apply Composite Settings", "This will apply all default composite settings to all existing composites " +
                    "in this anim module. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplyCompositeSettings();
                    
                    if (MxMAnimConfigWindow.IsOpen())
                        MxMAnimConfigWindow.Inst().Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);

            EditorGUI.BeginChangeCheck();
            m_spIgnoreEdges.boolValue = EditorGUILayout.ToggleLeft("IgnoreEdges", m_spIgnoreEdges.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_spIgnoreEdges.boolValue)
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

            GUILayout.Space(15f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Idle Sets", EditorStyles.boldLabel, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", applyBtnStyle, GUILayout.Height(16f)))
            {
                if (EditorUtility.DisplayDialog("Apply Idle Set Settings", "This will apply all default idle set settings to all existing idle sets " +
                    "in this anim module. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplyIdleSetSettings();

                    if (MxMAnimConfigWindow.IsOpen())
                        MxMAnimConfigWindow.Inst().Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min Loops", GUILayout.Width(80f));
            m_spIdleMinLoops.intValue = EditorGUILayout.IntField(m_spIdleMinLoops.intValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Loops", GUILayout.Width(80f));
            m_spIdleMaxLoops.intValue = EditorGUILayout.IntField(m_spIdleMaxLoops.intValue);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Blend Space", EditorStyles.boldLabel, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", applyBtnStyle, GUILayout.Height(16f)))
            {
                if (EditorUtility.DisplayDialog("Apply Blendspace Settings", "This will apply all default Blendspace settings to all existing blend spaces " +
                    "in this anim module. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplyBlendSpaceSettings();

                    if (MxMAnimConfigWindow.IsOpen())
                        MxMAnimConfigWindow.Inst().Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            m_spNormalizeBlendSpace.boolValue = EditorGUILayout.Toggle("Normalize", m_spNormalizeBlendSpace.boolValue);

            EditorGUILayout.PropertyField(m_spBlendSpaceType, new GUIContent("Type"));
            EditorGUI.BeginChangeCheck();
            Vector2 spacing = EditorGUILayout.Vector2Field("Scatter Spacing", m_spScatterSpacing.vector2Value);
            if(EditorGUI.EndChangeCheck())
            {
                if (spacing.x < 0.01f)
                    spacing.x = 0.01f;

                if (spacing.y < 0.01f)
                    spacing.y = 0.01f;

                m_spScatterSpacing.vector2Value = spacing;
            }

            EditorGUI.BeginChangeCheck();
            Vector2 BSMagnitude = EditorGUILayout.Vector2Field("Magnitude", m_spBlendSpaceMagnitude.vector2Value);
            if (EditorGUI.EndChangeCheck())
            {
                if (BSMagnitude.x < 0.01f)
                    BSMagnitude.x = 0.01f;

                if (BSMagnitude.y < 0.01f)
                    BSMagnitude.y = 0.01f;

                m_spBlendSpaceMagnitude.vector2Value = BSMagnitude;
            }

            EditorGUI.BeginChangeCheck();
            Vector2 BSSmoothing = EditorGUILayout.Vector2Field("Smoothing", m_spBlendSpaceSmoothing.vector2Value);
            if (EditorGUI.EndChangeCheck())
            {
                if (BSSmoothing.x < 0.01f)
                    BSSmoothing.x = 0.01f;

                if (BSSmoothing.y < 0.01f)
                    BSSmoothing.y = 0.01f;

                m_spBlendSpaceSmoothing.vector2Value = BSSmoothing;
            }

            GUILayout.Space(15f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", applyBtnStyle, GUILayout.Height(16f)))
            {
                if (EditorUtility.DisplayDialog("Apply All Tags", "This will apply all default settings to all existing animations " +
                    "in this animation module. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplyTags(0);

                    if (MxMAnimConfigWindow.IsOpen())
                        MxMAnimConfigWindow.Inst().Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Require", GUILayout.Width(50f));
            EditorFunctions.DrawTagFlagFieldWithCustomNames(m_moduleData.TagNames.ToArray(), m_spRequireTags, 75f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Favour", GUILayout.Width(50f));
            EditorFunctions.DrawTagFlagFieldWithCustomNames(m_moduleData.FavourTagNames.ToArray(), m_spFavourTags, 75f);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5f);

            if (GUILayout.Button("Apply Require Tags", applyBtnStyle, GUILayout.Height(16f)))
            {
                if (EditorUtility.DisplayDialog("Apply Require Tags", "This will apply the default require tags to all existing animation " +
                    "in this animation module. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplyTags(1);

                    if (MxMAnimConfigWindow.IsOpen())
                    {
                        MxMAnimConfigWindow.Inst().Repaint();
                    }
                }
            }

            if (GUILayout.Button("Apply Favour Tags", applyBtnStyle, GUILayout.Height(16f)))
            {
                if (EditorUtility.DisplayDialog("Apply Favour Tags", "This will apply the default favour tags to all existing animations " +
                    "in this animation module. Are you sure this is what you want to do?", "Yes", "Cancel"))
                {
                    ApplyTags(2);

                    if (MxMAnimConfigWindow.IsOpen())
                        MxMAnimConfigWindow.Inst().Repaint();
                }
            }

            m_soAnimModule.ApplyModifiedProperties();
        }

        public void OnLostFocus()
        {
            Close();
        }

        public void ApplyCompositeSettings()
        {
            SerializedProperty spCompositeCategories = m_soAnimModule.FindProperty("m_compositeCategories");

            for(int i = 0; i < spCompositeCategories.arraySize; ++i)
            {
                SerializedProperty spCategory = spCompositeCategories.GetArrayElementAtIndex(i);

                if (spCategory == null)
                    continue;

                //First change the defaults for the category
                SerializedProperty spIgnoreEdges_default = spCategory.FindPropertyRelative("IgnoreEdges_default");
                SerializedProperty spExtrapolate_default = spCategory.FindPropertyRelative("Extrapolate_default");
                SerializedProperty spFlattenTrajectory_default = spCategory.FindPropertyRelative("FlattenTrajectory_default");
                SerializedProperty spRuntimeSplicing_default = spCategory.FindPropertyRelative("RuntimeSplicing_default");

                spIgnoreEdges_default.boolValue = m_spIgnoreEdges.boolValue;
                spExtrapolate_default.boolValue = m_spExtrapolate.boolValue;
                spFlattenTrajectory_default.boolValue = m_spFlattenTrajectory.boolValue;
                spRuntimeSplicing_default.boolValue = m_spRuntimeSplicing.boolValue;

                //Now change each individual composite within each category to have those settings
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                if (spCompositeList == null)
                    continue;

                for(int k = 0; k < spCompositeList.arraySize; ++k)
                {
                    SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(k);

                    if (spComposite == null)
                        continue;

                    MxMAnimationClipComposite composite = spComposite.objectReferenceValue as MxMAnimationClipComposite;

                    if (composite == null)
                        continue;

                    SerializedObject soClipComposite = new SerializedObject(composite);

                    SerializedProperty spIgnoreEdges = soClipComposite.FindProperty("IgnoreEdges");
                    SerializedProperty spExtrapolateTrajectory = soClipComposite.FindProperty("ExtrapolateTrajectory");
                    SerializedProperty spFlattenTrajectory = soClipComposite.FindProperty("FlattenTrajectory");
                    SerializedProperty spRuntimeSplicing = soClipComposite.FindProperty("RuntimeSplicing");

                    spIgnoreEdges.boolValue = m_spIgnoreEdges.boolValue;
                    spFlattenTrajectory.boolValue = m_spFlattenTrajectory.boolValue;
                    spRuntimeSplicing.boolValue = m_spRuntimeSplicing.boolValue;
                    spExtrapolateTrajectory.boolValue = m_spExtrapolate.boolValue;

                    soClipComposite.ApplyModifiedProperties();
                }
            }
        }

        public void ApplyIdleSetSettings()
        {
            SerializedProperty spIdleSets = m_soAnimModule.FindProperty("m_animIdleSets");

            if (spIdleSets == null)
                return;

            for(int i = 0; i < spIdleSets.arraySize; ++i)
            {
                SerializedProperty spIdleSet = spIdleSets.GetArrayElementAtIndex(i);

                if (spIdleSet == null || spIdleSet.objectReferenceValue == null)
                    continue;

                MxMAnimationIdleSet idleSet = spIdleSet.objectReferenceValue as MxMAnimationIdleSet;

                if (idleSet == null)
                    continue;

                SerializedObject soIdleSet = new SerializedObject(idleSet);

                SerializedProperty spMinLoops = soIdleSet.FindProperty("MinLoops");
                SerializedProperty spMaxLoops = soIdleSet.FindProperty("MaxLoops");

                spMinLoops.intValue = m_spIdleMinLoops.intValue;
                spMaxLoops.intValue = m_spIdleMaxLoops.intValue;

                soIdleSet.ApplyModifiedProperties();
            }
        }

        public void ApplyBlendSpaceSettings()
        {
            SerializedProperty spBlendSpaces = m_soAnimModule.FindProperty("m_blendSpaces");

            if (spBlendSpaces == null)
                return;

            for(int i = 0; i < spBlendSpaces.arraySize; ++i)
            {
                SerializedProperty spBlendSpace = spBlendSpaces.GetArrayElementAtIndex(i);

                if (spBlendSpace == null || spBlendSpace.objectReferenceValue == null)
                    continue;

                MxMBlendSpace blendSpace = spBlendSpace.objectReferenceValue as MxMBlendSpace;

                if (blendSpace == null)
                    return;

                SerializedObject soBlendSpace = new SerializedObject(blendSpace);

                SerializedProperty spBlendSpaceType = soBlendSpace.FindProperty("m_scatterSpace");
                SerializedProperty spScatterSpacing = soBlendSpace.FindProperty("m_scatterSpacing");
                SerializedProperty spNormalizeBlendSpace = soBlendSpace.FindProperty("m_normalizeTime");
                SerializedProperty spBlendSpaceMagnitude = soBlendSpace.FindProperty("m_magnitude");
                SerializedProperty spBlendSpaceSmoothing = soBlendSpace.FindProperty("m_smoothing");

                spBlendSpaceType.intValue = m_spBlendSpaceType.intValue;
                spScatterSpacing.vector2Value = m_spScatterSpacing.vector2Value;
                spNormalizeBlendSpace.boolValue = m_spNormalizeBlendSpace.boolValue;
                spBlendSpaceMagnitude.vector2Value = m_spBlendSpaceMagnitude.vector2Value;
                spBlendSpaceSmoothing.vector2Value = m_spBlendSpaceSmoothing.vector2Value;

                soBlendSpace.ApplyModifiedProperties();
            }
        }

        public void ApplyTags(int a_option)
        {
            //First Idle Sets
            SerializedProperty spIdleSets = m_soAnimModule.FindProperty("m_animIdleSets");

            if (spIdleSets == null)
                return;

            for (int i = 0; i < spIdleSets.arraySize; ++i)
            {
                SerializedProperty spIdleSet = spIdleSets.GetArrayElementAtIndex(i);

                if (spIdleSet == null || spIdleSet.objectReferenceValue == null)
                    continue;

                MxMAnimationIdleSet idleSet = spIdleSet.objectReferenceValue as MxMAnimationIdleSet;

                if (idleSet == null)
                    continue;

                SerializedObject soIdleSet = new SerializedObject(idleSet);

                SerializedProperty spRequireTags = soIdleSet.FindProperty("Tags");
                SerializedProperty spFavourTags = soIdleSet.FindProperty("FavourTags");

                switch (a_option)
                {
                    case 0:
                        {
                            spRequireTags.intValue = m_spRequireTags.intValue;
                            spFavourTags.intValue = m_spFavourTags.intValue;
                        }
                        break;
                    case 1: { spRequireTags.intValue = m_spRequireTags.intValue; } break;
                    case 2: { spFavourTags.intValue = m_spFavourTags.intValue; } break;
                }

                soIdleSet.ApplyModifiedProperties();
            }

            //Second Blend Spaces
            SerializedProperty spBlendSpaces = m_soAnimModule.FindProperty("m_blendSpaces");

            if (spBlendSpaces == null)
                return;

            for (int i = 0; i < spBlendSpaces.arraySize; ++i)
            {
                SerializedProperty spBlendSpace = spBlendSpaces.GetArrayElementAtIndex(i);

                if (spBlendSpace == null || spBlendSpace.objectReferenceValue == null)
                    continue;

                MxMBlendSpace blendSpace = spBlendSpace.objectReferenceValue as MxMBlendSpace;

                if (blendSpace == null)
                    return;

                SerializedObject soBlendSpace = new SerializedObject(blendSpace);

                SerializedProperty spRequireTags = soBlendSpace.FindProperty("GlobalTags");
                SerializedProperty spFavourTags = soBlendSpace.FindProperty("GlobalFavourTags");

                switch (a_option)
                {
                    case 0:
                        {
                            spRequireTags.intValue = m_spRequireTags.intValue;
                            spFavourTags.intValue = m_spFavourTags.intValue;
                        }
                        break;
                    case 1: { spRequireTags.intValue = m_spRequireTags.intValue; } break;
                    case 2: { spFavourTags.intValue = m_spFavourTags.intValue; } break;
                }

                soBlendSpace.ApplyModifiedProperties();
            }

            //Finally composite categories
            SerializedProperty spCompositeCategories = m_soAnimModule.FindProperty("m_compositeCategories");

            for (int i = 0; i < spCompositeCategories.arraySize; ++i)
            {
                SerializedProperty spCategory = spCompositeCategories.GetArrayElementAtIndex(i);

                if (spCategory == null)
                    continue;

                //First change the defaults for the category
                SerializedProperty spRequireTags_default = spCategory.FindPropertyRelative("RequireTags_default");
                SerializedProperty spFavourTags_default = spCategory.FindPropertyRelative("FavourTags_default");

                switch (a_option)
                {
                    case 0:
                        {
                            spRequireTags_default.intValue = m_spRequireTags.intValue;
                            spFavourTags_default.intValue = m_spFavourTags.intValue;
                        }
                        break;
                    case 1: { spRequireTags_default.intValue = m_spRequireTags.intValue; } break;
                    case 2: { spFavourTags_default.intValue = m_spFavourTags.intValue; } break;
                }


                //Now change each individual composite within each category to have those tags
                SerializedProperty spCompositeList = spCategory.FindPropertyRelative("Composites");

                if (spCompositeList == null)
                    continue;

                for (int k = 0; k < spCompositeList.arraySize; ++k)
                {
                    SerializedProperty spComposite = spCompositeList.GetArrayElementAtIndex(k);

                    if (spComposite == null)
                        continue;

                    MxMAnimationClipComposite composite = spComposite.objectReferenceValue as MxMAnimationClipComposite;

                    if (composite == null)
                        continue;

                    SerializedObject soClipComposite = new SerializedObject(composite);

                    SerializedProperty spRequireTags = soClipComposite.FindProperty("GlobalTags");
                    SerializedProperty spFavourTags = soClipComposite.FindProperty("GlobalFavourTags");


                    switch (a_option)
                    {
                        case 0:
                            {
                                spRequireTags.intValue = m_spRequireTags.intValue;
                                spFavourTags.intValue = m_spFavourTags.intValue;
                            }
                            break;
                        case 1: { spRequireTags.intValue = m_spRequireTags.intValue; } break;
                        case 2: { spFavourTags.intValue = m_spFavourTags.intValue; } break;
                    }

                    soClipComposite.ApplyModifiedProperties();
                }
            }
        }
    }
}