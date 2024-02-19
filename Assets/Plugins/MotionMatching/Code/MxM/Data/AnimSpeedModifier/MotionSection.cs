using UnityEngine;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    [System.Serializable]
    public class MotionSection
    {
        [SerializeField]
        public int MotionSectionId;

        [SerializeField]
        public int MotionPresetId = 0;

        [SerializeField]
        public EMotionModType ModType = EMotionModType.PlaybackSpeed;

        [SerializeField]
        public EMotionModSmooth SmoothType = EMotionModSmooth.Linear;

        [SerializeField]
        public float RawModValue = 1f;

        [SerializeField]
        public bool UsePresets = true;

        [SerializeField]
        public float EndTime;

        [System.NonSerialized]
        public bool Selected;

        [System.NonSerialized]
        public bool Dragging;

        public MotionSection() { }

        public MotionSection(MotionSection a_copy)
        {
            MotionSectionId = a_copy.MotionSectionId;
            MotionPresetId = a_copy.MotionPresetId;
            ModType = a_copy.ModType;
            SmoothType = a_copy.SmoothType;
            RawModValue = a_copy.RawModValue;
            UsePresets = a_copy.UsePresets;
            EndTime = a_copy.EndTime;
            Selected = false;
            Dragging = false;
        }

        public float GetSpeedMod(float a_startTime, MotionTimingPresets a_presets, IMxMAnim a_mxmAnim)
        {
            MotionPreset motionDef = null;

            if (a_presets != null)
                motionDef = a_presets.GetDefenition(MotionPresetId);

            if (motionDef == null || !UsePresets) //Raw
            {
                switch (ModType)
                {
                    case EMotionModType.PlaybackSpeed:
                        {
                            return 1f / RawModValue;
                        }
                    case EMotionModType.Duration:
                        {
                            return RawModValue / ((EndTime - a_startTime));
                        }
                    case EMotionModType.LinearSpeed:
                        {
                            if (a_mxmAnim != null)
                            {
                                float averageSpeedOriginal = a_mxmAnim.GetAverageRootSpeed(a_startTime, EndTime);

                                return averageSpeedOriginal / RawModValue;
                            }

                            return 1f / RawModValue;
                        }
                }
            }
            else //from Database
            {
                switch (motionDef.MotionType)
                {
                    case EMotionModType.PlaybackSpeed:
                        {
                            return 1f / motionDef.MotionTiming;
                        }
                    case EMotionModType.Duration:
                        {
                            return motionDef.MotionTiming / (EndTime - a_startTime);
                        }
                    case EMotionModType.LinearSpeed:
                        {
                            if (a_mxmAnim != null)
                            {
                                float averageSpeedOriginal = a_mxmAnim.GetAverageRootSpeed(a_startTime, EndTime);

                                return averageSpeedOriginal / motionDef.MotionTiming;
                            }

                            return 1f / motionDef.MotionTiming;
                        }
                }
            }
            return RawModValue;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public MotionSection(int a_id, int a_defId, EMotionModSmooth a_smoothType, float a_endTime)
        {
            MotionSectionId = a_id;
            MotionPresetId = a_defId;
            SmoothType = a_smoothType;
            EndTime = a_endTime;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public MotionSection(int a_id, MotionSection a_copy, float a_endTime)
        {

            MotionSectionId = a_id;
            MotionPresetId = a_copy.MotionPresetId;
            ModType = a_copy.ModType;
            SmoothType = a_copy.SmoothType;
            RawModValue = a_copy.RawModValue;
            UsePresets = a_copy.UsePresets;
            EndTime = a_endTime;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void Deselect()
        {
            Selected = false;
        }
    }//End of class: MotionSection
}//End of namespace: MxM