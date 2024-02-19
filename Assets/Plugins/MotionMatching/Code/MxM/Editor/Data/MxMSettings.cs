using UnityEngine;
using UnityEditor;
using MxM;

namespace MxMEditor
{

    public class MxMSettings : ScriptableObject
    {
        const string k_mxmSettingsPath = "Assets/Plugins/MxMSettings.asset";

        [SerializeField]
        public MxMAnimationClipComposite ActiveComposite;

        [SerializeField]
        public MxMAnimationIdleSet ActiveIdleSet;

        [SerializeField]
        public MxMBlendSpace ActiveBlendSpace;

        [SerializeField]
        public bool HelpActive;

        internal static MxMSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MxMSettings>(k_mxmSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MxMSettings>();
                settings.ActiveComposite = null;
                settings.ActiveIdleSet = null;
                settings.ActiveBlendSpace = null;
                settings.HelpActive = false;

                AssetDatabase.CreateAsset(settings, k_mxmSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        public static MxMSettings Instance()
        {
            return GetOrCreateSettings();
        }
    }
}