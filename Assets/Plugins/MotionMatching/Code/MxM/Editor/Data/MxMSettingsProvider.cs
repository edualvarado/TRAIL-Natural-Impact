using UnityEditor;
using System.IO;

#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#else
using UnityEngine.UIElements;
#endif

namespace MxMEditor
{
    public class MxMSettingsProvider : SettingsProvider
    {
        private SerializedObject m_mxmSettings;

        const string k_mxmSettingsPath = "Assets/Plugins/MxMSettings.asset";

        public MxMSettingsProvider(string a_path, SettingsScope a_scope = SettingsScope.User) : base(a_path, a_scope) { }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(k_mxmSettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_mxmSettings = MxMSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
        }

        [SettingsProvider]
        public static SettingsProvider CreateMxMSettingsProvider()
        {
            if (IsSettingsAvailable())
            {
                var provider = new MxMSettingsProvider("Project/MxMSettingsProvider", SettingsScope.Project);
                return provider;
            }

            return null;
        }

    }//End of class: MxMSettingsProvider
}//End of namespace: MxMEditor
