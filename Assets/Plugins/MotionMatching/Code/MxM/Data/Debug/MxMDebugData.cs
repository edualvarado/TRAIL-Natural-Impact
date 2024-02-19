#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This class is used to record the State of MxMAnimator over a series of frames. It 
    *  records frame data in a circular buffer continuous (when activated) so that the it can
    *  be re-simulated and played back for debugging purposes.
    *         
    *********************************************************************************************/
    public class MxMDebugger
    {
        public int DebugFrameCount = 600;

        private MxMDebugFrame[] m_debugFrameBuffer;
        private int m_currentIndex = -1;

        private Dictionary<int, int> m_usedPoses;

        private MxMAnimator m_targetAnimator;

        public MxMDebugFrame[] FrameData { get { return m_debugFrameBuffer; } }

        public int LastRecordedFrame { get { return m_currentIndex; } }

        //============================================================================================
        /**
        *  @brief Constructor for MxMDebugger.
        *  
        *  @param [MxMAnimator] a_animator - reference to the target MxMAnimator that this debug Data
        *  is based on.
        *         
        *********************************************************************************************/
        public MxMDebugger(MxMAnimator a_animator, int a_maxMixCount, int a_trajPointCount)
        {
            if(a_animator == null)
            {
                Debug.LogError("Trying to create MxMDebugger, but the MxMAnimator is null. Aborting operation");
                return;
            }

            m_targetAnimator = a_animator;
            m_debugFrameBuffer = new MxMDebugFrame[DebugFrameCount];

            for(int i=0; i < m_debugFrameBuffer.Length; ++i)
            {
                m_debugFrameBuffer[i].SetChannelCount(a_maxMixCount);
                m_debugFrameBuffer[i].SetTrajectoryCount(a_trajPointCount);
            }

            m_currentIndex = 0;
            m_usedPoses = new Dictionary<int, int>();
        }

        //============================================================================================
        /**
        *  @brief Finds and returns a reference to the next MxMDebugFrame in the circular buffer.
        *  This also increments the current index so that the buffer remains circular. Any modifications
        *  performaed on the retrieved state will always be the next item in the buffer even if the
        *  end of the fixed buffer is reached.
        *         
        *********************************************************************************************/
        public ref MxMDebugFrame GetNexDebugState()
        {
            ++m_currentIndex;
            
            if(m_currentIndex >= m_debugFrameBuffer.Length)
                m_currentIndex = 0;

            return ref m_debugFrameBuffer[m_currentIndex];
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public ref MxMDebugFrame GetDebugFrame(int a_frameId)
        {
            if (a_frameId < 0 || a_frameId >= m_debugFrameBuffer.Length)
                return ref m_debugFrameBuffer[m_currentIndex];

            return ref m_debugFrameBuffer[a_frameId];
        }

        //============================================================================================
        /**
        *  @brief In order to keep track of how often poses are being used, this function is called
        *  by the MxM animator to notify the debug data that a pose has been used. That poses use
        *  gets recorded in a dictionary so that analytics of pose use can be viewed at a later stage.
        *  
        *  @param [int] a_poseUsed - the pose Id of the pose used.
        *         
        *********************************************************************************************/
        public void UsePose(int a_poseUsed)
        {
            int timesUsed;
            if (m_usedPoses.TryGetValue(a_poseUsed, out timesUsed))
            {
                m_usedPoses[a_poseUsed] = timesUsed + 1;
            }
            else
            {
                m_usedPoses[a_poseUsed] = 1;
            }
        }

        //============================================================================================
        /**
        *  @brief Resets all used pose data
        *         
        *********************************************************************************************/
        public void ResetUsedPoseData()
        {
            if(m_usedPoses == null)
            {
                m_usedPoses = new Dictionary<int, int>();
            }
            else
            {
                m_usedPoses.Clear();
            }
        }

        //============================================================================================
        /**
        *  @brief Prints all used pose analytics data to the console
        *         
        *********************************************************************************************/
        public void DumpUsedPoseAnalytics()
        {
            if (m_usedPoses != null)
            {
                AnimationClip currentClip = null;

                int maxUseCount = 0;

                foreach (KeyValuePair<int, int> kvp in m_usedPoses)
                {
                    if (kvp.Value > maxUseCount)
                    {
                        maxUseCount = kvp.Value;
                    }
                }

                StringBuilder clipStats = new StringBuilder();

                int uniqueUsedPoses = 0;
                int clipTotalUsedPoses = 0;

                MxMAnimData curAnimData = m_targetAnimator.CurrentAnimData;

                for (int index = 0; index < curAnimData.Poses.Length; index++)
                {
                    PoseData animDataPose = curAnimData.Poses[index];

                    int clipId = 0;
                    switch(animDataPose.AnimType)
                    {
                        case EMxMAnimtype.Composite: { clipId = curAnimData.Composites[animDataPose.AnimId].ClipIdA; } break;
                        case EMxMAnimtype.BlendSpace: { clipId = curAnimData.BlendSpaces[animDataPose.AnimId].ClipIds[0]; } break;
                        case EMxMAnimtype.Clip: { clipId = curAnimData.ClipsData[animDataPose.AnimId].ClipId; } break;
                    }

                    var newClip = curAnimData.Clips[clipId] != currentClip; 

                    if (newClip)
                    {
                        if (currentClip)
                        {
                            clipStats.Append(clipTotalUsedPoses.ToString());

                            Debug.Log(clipStats.ToString(), currentClip);

                            clipTotalUsedPoses = 0;

                            clipStats.Clear();

                        }

                        currentClip = curAnimData.Clips[clipId];

                        clipStats.Append(currentClip.name);
                    }

                    int usedCount;

                    if (m_usedPoses.TryGetValue(index, out usedCount))
                    {
                        uniqueUsedPoses++;
                        clipTotalUsedPoses++;

                        int usedDigit = usedCount * 9 / maxUseCount;

                        clipStats.Append(usedDigit);
                    }
                    else
                    {
                        clipStats.Append('_');
                    }
                }

                if (currentClip)
                {
                    clipStats.Append(clipTotalUsedPoses.ToString());

                    Debug.Log(clipStats.ToString(), currentClip);

                    clipTotalUsedPoses = 0;

                    clipStats.Clear();

                }

                Debug.Log(string.Format("used {0}/{1} poses ", uniqueUsedPoses, curAnimData.Poses.Length));
            }
        }

        //============================================================================================
        /**
        *  @brief Generates a pose mask asset based on the used pose data analytics
        *         
        *********************************************************************************************/
        public void GeneratePoseMask()
        {
            if (m_usedPoses == null)
            {
                Debug.LogError("Trying to generate a pose mask but m_usedPoses Dictionary is null. Operation aborted");
                return;
            }

            MxMAnimData curAnimData = m_targetAnimator.CurrentAnimData;

            if (curAnimData == null)
            {
                Debug.LogError("Trying to generate a pose mask but the current MxMAnimData is null. Operation aborted");
                return;
            }

            PoseMask poseMask = ScriptableObject.CreateInstance<PoseMask>();
            poseMask.name = "PoseMask_" + curAnimData.name;
            poseMask.Initialize(curAnimData);
            poseMask.SetMask(m_usedPoses);

            curAnimData.StripPoseMask();
            curAnimData.BindPoseMask(poseMask);
            EditorUtility.SetDirty(curAnimData);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void ClearData(int a_maxMixCount, int a_trajPointCount)
        {
            m_debugFrameBuffer = new MxMDebugFrame[DebugFrameCount];

            for (int i = 0; i < m_debugFrameBuffer.Length; ++i)
            {
                m_debugFrameBuffer[i].SetChannelCount(a_maxMixCount);
                m_debugFrameBuffer[i].SetTrajectoryCount(a_trajPointCount);
            }

            m_usedPoses = new Dictionary<int, int>();

            m_currentIndex = -1;
        }

    }//End of class: MxMDebugger
}//End of namespace: MxM
#endif