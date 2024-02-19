// ================================================================================================
// File: MotionMatchingPlayable.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using UnityEngine.Playables;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief A simple playable behaviour that allows MxM access to the animation update to 
    *  separate the scheduling and collection of jobs as much as possible
    *         
    *********************************************************************************************/
    public class MotionMatchingPlayable : PlayableBehaviour
    {
        private MxMAnimator m_mxmAnimator;
        //============================================================================================
        /**
        *  @brief Sets the reference to the MxMAnimator. This is done when MxM is initialized
        *  
        *  @param [MxMAnimator] a_mxmAnimator - a reference to the MxMAnimator that this playable
        *  belongs to.
        *         
        *********************************************************************************************/
        public void SetMxMAnimator(MxMAnimator a_mxmAnimator)
        {
            m_mxmAnimator = a_mxmAnimator;
        }

        //============================================================================================
        /**
        *  @brief Triggers the second phase update of the MxMAniamtor
        *  
        *  @param [Playable] playable - the playable
        *  @param [FrameData] info - info about the current farame
        *         
        *********************************************************************************************/
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if(!m_mxmAnimator.IsPaused)
                m_mxmAnimator.MxMUpdate_Phase2();
        }
    }//End of class: MotionMatchingPlayable
}//End of namespace: MxM