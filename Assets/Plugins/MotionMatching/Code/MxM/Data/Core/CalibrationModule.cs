using UnityEngine;
using System;

namespace MxM
{
    [CreateAssetMenu(fileName = "MxMCalibrationModule", menuName = "MxM/Utility/MxMCalibrationModule", order = 0)]
    public class CalibrationModule : ScriptableObject
    {
        [SerializeField] private MxMAnimData m_targetAnimData = null;

        [SerializeField] private CalibrationData[] m_calibrationSets = null;
        
        public int CalibrationSetCount { get { return m_calibrationSets.Length; } }

        public bool IsValid()
        {
            if (m_targetAnimData == null)
                return false;

            foreach (CalibrationData calibration in m_calibrationSets)
            {
                if(!calibration.IsValid(m_targetAnimData))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsCompatibleWith(MxMAnimData a_animData)
        {
            if (a_animData == null)
                return false;

            return a_animData.MatchBones.Length == m_targetAnimData.MatchBones.Length;
        }

        public void Initialize(MxMAnimData a_targetAnimData)
        {
            if (a_targetAnimData == null)
                return;

            m_targetAnimData = a_targetAnimData;

            if(m_calibrationSets == null || m_calibrationSets.Length == 0)
            {
                m_calibrationSets = new CalibrationData[1];
                CalibrationData newCalibration = new CalibrationData();
                newCalibration.Initialize("Calibration 0", m_targetAnimData);
                m_calibrationSets[0] = newCalibration;
            }
            else
            {
                ValidateCalibrationSets();
            }
        }

        public void InitializeCalibration(CalibrationData[] a_sourceCalibration = null)
        {
            if (a_sourceCalibration == null || a_sourceCalibration.Length == 0)
            {
                CalibrationData newCalibData = new CalibrationData();
                newCalibData.Initialize("Calibration 0", m_targetAnimData);

                m_calibrationSets = new CalibrationData[1];
                m_calibrationSets[0] = newCalibData;
            }
            else
            {
                m_calibrationSets = new CalibrationData[a_sourceCalibration.Length];

                for (int i = 0; i < a_sourceCalibration.Length; ++i)
                {
                    CalibrationData newCalibData = new CalibrationData(a_sourceCalibration[i]);
                    newCalibData.Validate(m_targetAnimData);

                    m_calibrationSets[i] = newCalibData;
                }
            }
        }

        public void ValidateCalibrationSets()
        {
            if (m_calibrationSets == null)
            {
                Debug.LogWarning("No calibration sets added to calibration data: " + this.name);
                return;
            }
            
            foreach (CalibrationData calibration in m_calibrationSets)
            {
                if (calibration != null)
                {
                    calibration.Validate(m_targetAnimData);
                }
                else
                {
                    Debug.LogWarning("Null calibration set found in calibration set module during initialization");
                }
            }
        }

        public int GetCalibrationHandle(string a_calibrationName)
        {
            for(int i = 0; i < m_calibrationSets.Length; ++i)
            {
                if(a_calibrationName == m_calibrationSets[i].CalibrationName)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetCalibrationHandle(CalibrationData a_calibData)
        {
            if (a_calibData == null)
                return -1;

            for(int i = 0; i < m_calibrationSets.Length; ++i)
            {
                if (m_calibrationSets[i] == a_calibData)
                    return i;
            }

            return -1;
        }

        public CalibrationData GetCalibrationSet(string a_calibrationName)
        {
            foreach (CalibrationData calibration in m_calibrationSets)
            {
                if(calibration.CalibrationName == a_calibrationName)
                {
                    return calibration;
                }
            }

            return null;
        }

        public CalibrationData GetCalibrationSet(int a_calibrationId)
        {
            if (a_calibrationId < 0 || a_calibrationId > m_calibrationSets.Length - 1)
                return null;

            return m_calibrationSets[a_calibrationId];
        }

        public bool CheckUpdateAnimData(MxMAnimData animData)
        {
            if(animData == m_targetAnimData)
            {
                ValidateCalibrationSets();
                return true;
            }
            else
            {
                //The anim data is different
                return false;
            }
        }

    }//End of class: CalibrationModule
} //End of namespace: MxM