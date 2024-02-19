using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
   // [CreateAssetMenu(fileName = "MxMMotionTimingPresets", menuName = "MxM/Utility/MxMMotionTimingPresets", order = 4)]
    public class MotionTimingPresets : ScriptableObject
    {
        [SerializeField]
        private List<MotionPreset> m_defenitions = new List<MotionPreset>();

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void AddDefenition(string a_name, float a_timing, EMotionModType a_motionType)
        {
            MotionPreset newDefenition = new MotionPreset(a_name, a_timing, a_motionType);
            m_defenitions.Add(newDefenition);

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void RemoveDefenition(MotionPreset a_defenition)
        {
            m_defenitions.Remove(a_defenition);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void RemoveDefenition(int a_index)
        {
            if(a_index > 0 && a_index < m_defenitions.Count)
            {
                m_defenitions.RemoveAt(a_index);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public MotionPreset GetDefenition(int a_index)
        {
            if (a_index >= 0 && a_index < m_defenitions.Count)
            {
                return m_defenitions[a_index];
            }

            return null;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public MotionPreset GetDefenition(string a_name)
        {
            for(int i=0; i < m_defenitions.Count; ++i)
            {
                if(a_name == m_defenitions[i].MotionName)
                {
                    return m_defenitions[i];
                }
            }

            return null;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public string[] GetDefenitionNames()
        {
            string[] names = new string[m_defenitions.Count];

            for(int i = 0; i < m_defenitions.Count; ++i)
            {
                names[i] = m_defenitions[i].MotionName;
            }

            return names;
        }


    }//End of class AnimSpeedBlackboard
}//End of namespace: MxM
