using System.Collections.Generic;
using UnityEngine;
using MxM;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    [System.Serializable]
    public class MotionModifyData
    {
        [SerializeField]
        private IMxMAnim m_targetMxMAnim;

        [SerializeField]
        public List<MotionSection> MotionSections = new List<MotionSection>(); //Name of the variable for a section

        public MotionModifyData() { }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public MotionModifyData(MotionModifyData a_copy, IMxMAnim a_targetMxMAnim)
        {
            m_targetMxMAnim = a_targetMxMAnim;

            MotionSections = new List<MotionSection>();
            foreach(MotionSection motionSection in a_copy.MotionSections)
            {
                MotionSections.Add(new MotionSection(motionSection));
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public bool OnEnable(IMxMAnim m_targetMxMAnim)
        {
            if (MotionSections == null)
                MotionSections = new List<MotionSection>();

            if (m_targetMxMAnim == null)
                return false;

            AnimationClip primaryClip = m_targetMxMAnim.TargetClip;

            if (MotionSections.Count == 0 && m_targetMxMAnim != null && primaryClip != null)
            {
                MotionSections.Add(new MotionSection(0, 0, 0, primaryClip.length));
                return true;
            }

            return false;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void NewClipSet()
        {
            if (MotionSections == null)
                MotionSections = new List<MotionSection>();

            MotionSections.Clear();

            if (m_targetMxMAnim != null)
            {
                AnimationClip primaryClip = m_targetMxMAnim.TargetClip;

                if (primaryClip != null)
                {
                    MotionSections.Add(new MotionSection(0, 0, EMotionModSmooth.Linear, primaryClip.length));
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void AddPOI(float a_time)
        {
#if UNITY_EDITOR
            Undo.RecordObject(m_targetMxMAnim as ScriptableObject, "Section added");
#endif

            for (int i=0; i < MotionSections.Count; ++i)
            {
                if(a_time < MotionSections[i].EndTime)
                {
                    MotionSection newSection = new MotionSection(i, MotionSections[i], a_time);

                    //MotionSection newSection = new MotionSection(i, 0, EMotionModSmooth.Linear, a_time);
                    MotionSections.Insert(i, newSection);

                    for(int n=i+1; n < MotionSections.Count; ++n)
                    {
                        MotionSections[n].MotionSectionId = n;
                    }

                    break;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public float GetSectionSpeedAtTime(float a_time, MotionTimingPresets a_presets)
        {
            float speed = 1f;
            float lastTime = 0f;
            for(int i=0; i < MotionSections.Count; ++i)
            {
                if(a_time > lastTime && a_time < MotionSections[i].EndTime)
                {
                    speed = 1f / MotionSections[i].GetSpeedMod(lastTime, a_presets, m_targetMxMAnim);
                    break;
                }

                lastTime = MotionSections[i].EndTime;
            }

            return speed;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void VerifyData()
        {

            if (m_targetMxMAnim == null)
                return;

            AnimationClip primaryClip = m_targetMxMAnim.TargetClip;

            if (primaryClip == null)
                return;

            for (int i = MotionSections.Count - 1; i >= 0; --i)
            {
                MotionSection section = MotionSections[i];

                if (i - 1 >= 0)
                {
                    if (MotionSections[i - 1].EndTime > primaryClip.length)
                    {
                        MotionSections.RemoveAt(i);
                        continue;
                    }
                }
                else if (section.EndTime > primaryClip.length)
                {
                    section.EndTime = primaryClip.length;
                }
            }

            if (MotionSections.Count > 0)
            {
                MotionSections[MotionSections.Count - 1].EndTime = primaryClip.length;
            }

        }

    }//End of class: SpeedModData
}//End of namepsace: MxM