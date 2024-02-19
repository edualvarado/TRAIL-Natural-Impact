// ================================================================================================
// File: MxMAnimator_BlendSpace.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This is partial implementation of the MxMAnimator. This particular partial class 
    *  handles all blend space logic for the of the MxMAnimator.
    *         
    *********************************************************************************************/
    public partial class MxMAnimator : MonoBehaviour
    {
        //Blend Spaces
        private int m_curBlendSpaceChannel; //The id of the motion matching mixer input slot that the current blend space is using
        private Vector2 m_blendSpacePosition; //Current position in the current blend space
        private List<float> m_blendSpaceWeightings; //A list of blend space weightings for all clips in the blend space
        private List<float> m_blendSpaceClipLengths; //A list of all the clip lengths within the blend space.
        private Vector2 m_desiredBlendSpacePosition; //The desired blend space position (the actual position is smoothly interpolated towards this value)
        private bool m_normalizeBlendSpaceTime; //If true the time / speed of the blend space clips will be normalized.
        private float m_normalizedBlendSpaceLength = 0f; //The length of the blendspace when normalized by weightings and varying clip lengths.
        
        public Vector2 DesiredBlendSpacePosition { get { return m_desiredBlendSpacePosition; } }
        public float DesiredBlendSpacePositionX { get { return m_desiredBlendSpacePosition.x; } }
        public float DesiredBlendSpacePositionY { get { return m_desiredBlendSpacePosition.y; } }
        
        public Vector2 BlendSpacePosition { get { return m_blendSpacePosition; } }
        public float BlendSpacePositionX { get { return m_blendSpacePosition.x; } }
        public float BlendSpacePositionY { get { return m_blendSpacePosition.y; } }

        //=============================================================================================
        /**
        *  @brief Updates the blend spaces position and weighting.
        *  
        *  To avoid sudden snaps in animation, the blend space position needs to be smoothly interpolated
        *  toward the desired position. This update function manages this smooth transition and also
        *  re-calculates the blend space weightings as the positions are smoothly changed.
        *         
        *********************************************************************************************/
        private void UpdateBlendSpaces()
        {
            if (m_chosenPose.AnimId >= CurrentAnimData.BlendSpaces.Length || m_chosenPose.AnimId < 0)
                return;

            ref BlendSpaceData curBlendSpace = ref CurrentAnimData.BlendSpaces[m_chosenPose.AnimId];

            Vector2 lastPosition = m_blendSpacePosition;

            switch (m_blendSpaceSmoothing)
            {
                case EBlendSpaceSmoothing.Lerp:
                    {
                        m_blendSpacePosition = Vector2.Lerp(m_blendSpacePosition,
                            m_desiredBlendSpacePosition, m_blendSpaceSmoothRate.x * p_currentDeltaTime * m_playbackSpeed * 60f);
                    }
                    break;
                case EBlendSpaceSmoothing.Lerp2D:
                    {
                        m_blendSpacePosition.x = Mathf.Lerp(m_blendSpacePosition.x, m_desiredBlendSpacePosition.x,
                            m_blendSpaceSmoothRate.x * p_currentDeltaTime * m_playbackSpeed * 60f);

                        m_blendSpacePosition.y = Mathf.Lerp(m_blendSpacePosition.y, m_desiredBlendSpacePosition.x,
                            m_blendSpaceSmoothRate.y * p_currentDeltaTime * m_playbackSpeed * 60f);
                    }
                    break;
                case EBlendSpaceSmoothing.Unique:
                    {
                        m_blendSpacePosition = Vector2.Lerp(m_blendSpacePosition,
                            m_desiredBlendSpacePosition, curBlendSpace.Smoothing.x * p_currentDeltaTime * m_playbackSpeed * 60f);
                    }
                    break;
                case EBlendSpaceSmoothing.Unique2D:
                    {
                        m_blendSpacePosition.x = Mathf.Lerp(m_blendSpacePosition.x, m_desiredBlendSpacePosition.x,
                            curBlendSpace.Smoothing.x * p_currentDeltaTime * m_playbackSpeed * 60f);

                        m_blendSpacePosition.y = Mathf.Lerp(m_blendSpacePosition.y, m_desiredBlendSpacePosition.y,
                            curBlendSpace.Smoothing.y * p_currentDeltaTime * m_playbackSpeed * 60f);
                    }
                    break;
                default:
                    return;
            }

            if ((m_blendSpacePosition - lastPosition).SqrMagnitude() < 0.00001f)
                return;

            BlendSpacePositionChanged(ref curBlendSpace, m_primaryBlendChannel);
        }

        public void BlendSpacePositionChanged(ref BlendSpaceData a_blendSpace, int a_blendChannel)
        {
            CalculateBlendSpaceWeightings(a_blendSpace.Positions);

            if (m_normalizeBlendSpaceTime)
            {
                m_normalizedBlendSpaceLength = 0f;
                for (int i = 0; i < m_blendSpaceClipLengths.Count; ++i)
                {
                    m_normalizedBlendSpaceLength += m_blendSpaceClipLengths[i] * m_blendSpaceWeightings[i];
                }
            }

            var blendSpacePlayable = m_animationMixer.GetInput(a_blendChannel);

            if (blendSpacePlayable.IsValid())
            {
                int inputCount = blendSpacePlayable.GetInputCount();
                for (int i = 0; i < inputCount; ++i)
                {
                    blendSpacePlayable.SetInputWeight(i, m_blendSpaceWeightings[i]);
    
                    //Blendspace time sync / normalization
                    if (m_normalizeBlendSpaceTime)
                    {
                        var clipPlayable = (AnimationClipPlayable)blendSpacePlayable.GetInput(i);
                        clipPlayable.SetSpeed(m_playbackSpeed * (m_blendSpaceClipLengths[i] / m_normalizedBlendSpaceLength)); //Todo: Custom Blendspace speed support
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Begins a loop blend in the MxMAnimator. LoopBlend animations will continue to play
        *  until they are manually exited through code.
        *  
        *  @param int a_blend
        *         
        *********************************************************************************************/
        public bool BeginLoopBlend(int a_blendSpaceId, float a_x = 0f, float a_y = 0f)
        {
            if (a_blendSpaceId >= 0 && a_blendSpaceId < CurrentAnimData.BlendSpaces.Length)
            {
#if UNITY_2019_1_OR_NEWER
                if (p_riggingIntegration != null)
                    p_riggingIntegration.CacheTransforms();
#endif
                ref BlendSpaceData blendSpaceData = ref CurrentAnimData.BlendSpaces[a_blendSpaceId];

                int bestPoseId = blendSpaceData.StartPoseId;
                float bestCost = float.MaxValue;
                for (int poseId = blendSpaceData.StartPoseId; poseId <= blendSpaceData.EndPoseId; ++poseId)
                {
                    float thisPoseCost = ComputePoseCost(ref CurrentAnimData.Poses[poseId]);

                    if (thisPoseCost < bestCost)
                    {
                        bestCost = thisPoseCost;
                        bestPoseId = poseId;
                    }
                }
                
                m_normalizeBlendSpaceTime = blendSpaceData.NormalizeTime;
                if (m_normalizeBlendSpaceTime)
                {
                    m_blendSpaceClipLengths.Clear();
                    for (int i = 0; i < blendSpaceData.ClipIds.Length; ++i)
                    {
                        m_blendSpaceClipLengths.Add(CurrentAnimData.Clips[blendSpaceData.ClipIds[i]].length);
                    }
                }

                m_blendSpacePosition = new Vector2(a_x, a_y);
                m_desiredBlendSpacePosition = m_blendSpacePosition;

                //ref readonly PoseData bestPose = ref CurrentAnimData.Poses[bestPoseId];
                m_chosenPose = CurrentAnimData.Poses[bestPoseId];
                BlendToPose(ref m_chosenPose);

                m_fsm.GoToState((uint)EMxMStates.LoopBlend, true);

                BlendSpacePositionChanged(ref blendSpaceData, m_primaryBlendChannel);

                return true;
            }

            return false;
        }

        //============================================================================================
        /**
        *  @brief Begins a loop blend in the MxMAnimator. LoopBlend animations will continue to play
        *  until they are manually exited through code.
        *  
        *  @param string - BlendSpaceame
        *         
        *********************************************************************************************/
        public bool BeginLoopBlend(string a_blendSpaceName, float a_x = 0f, float a_y = 0f)
        {
            int blendSpaceId = CurrentAnimData.BlendSpaceIdFromName(a_blendSpaceName);

            if (blendSpaceId == -1)
                return false;

            return BeginLoopBlend(blendSpaceId, a_x, a_y);
        }

        //============================================================================================
        /**
        *  @brief Updates the loop blend state of the MxMAniamtor
        *         
        *********************************************************************************************/
        private void UpdateLoopBlend()
        {
#if UNITY_EDITOR
            if (m_debugCurrentPose)
                ComputeCurrentPose();
#endif
            UpdateBlendSpaces();
        }

        //============================================================================================
        /**
        *  @brief Ends the loop blend state if it currently in one and reverts to standard matching
        *         
        *********************************************************************************************/
        public void EndLoopBlend()
        {
            if (m_fsm.CurrentStateId == (uint)EMxMStates.LoopBlend)
            {
                m_fsm.GoToState((uint)EMxMStates.Matching, true);
            }
        }

        //============================================================================================
        /**
        *  @brief Utility function which calculates the weighting of all clips in the current blend space
        *  based on the 2D position within the space and the position of the clips.
        *  
        *  @param [Vector2[]] a_positions - the position of all the clips in the blend space
        *         
        *********************************************************************************************/
        private void CalculateBlendSpaceWeightings(Vector2[] a_positions)
        {
            if (a_positions.Length != m_blendSpaceWeightings.Count)
            {
                m_blendSpaceWeightings.Clear();
                for (int i = 0; i < a_positions.Length; ++i)
                    m_blendSpaceWeightings.Add(0f);
            }

            float totalWeight = 0f;

            for (int i = 0; i < a_positions.Length; ++i)
            {
                Vector2 positionI = a_positions[i];
                Vector2 iToSample = m_blendSpacePosition - positionI;

                float weight = 1f;

                for (int j = 0; j < a_positions.Length; ++j)
                {
                    if (j == i)
                        continue;

                    Vector2 positionJ = a_positions[j];
                    Vector2 iToJ = positionJ - positionI;

                    //Calc Weight
                    float lensq_ij = Vector2.Dot(iToJ, iToJ);
                    float newWeight = Vector2.Dot(iToSample, iToJ) / lensq_ij;
                    newWeight = 1f - newWeight;
                    newWeight = Mathf.Clamp01(newWeight);

                    weight = Mathf.Min(weight, newWeight);
                }

                m_blendSpaceWeightings[i] = weight;
                totalWeight += weight;
            }

            for (int i = 0; i < m_blendSpaceWeightings.Count; ++i)
            {
                m_blendSpaceWeightings[i] = m_blendSpaceWeightings[i] / totalWeight;
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the blend space position for the currently chosen blend space via float values
        *  
        *  @param [float] a_x - the x position in the blend space
        *  @param [float] a_y - the y position in the blend space
        *         
        *********************************************************************************************/
        public void SetBlendSpacePosition(float a_x, float a_y, bool a_instant = false)
        {
            if (m_chosenPose.AnimType == EMxMAnimtype.BlendSpace)
            {
                m_desiredBlendSpacePosition.x = Mathf.Clamp(a_x, -1f, 1f);
                m_desiredBlendSpacePosition.y = Mathf.Clamp(a_y, -1f, 1f);

                if(a_instant || m_blendSpaceSmoothing == EBlendSpaceSmoothing.None)
                {
                    m_blendSpacePosition = m_desiredBlendSpacePosition;

                    if (m_chosenPose.AnimType != EMxMAnimtype.BlendSpace)
                        return;

                    ref BlendSpaceData curBlendSpace = ref CurrentAnimData.BlendSpaces[m_chosenPose.AnimId];
                    BlendSpacePositionChanged(ref curBlendSpace, m_curBlendSpaceChannel);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the blend space position of the current chosen blend space on the x axis
        *  
        *  @param [float] a_x - the x position in the blend space
        *         
        *********************************************************************************************/
        public void SetBlendSpacePositionX(float a_x, bool a_instant = false)
        {
            if (m_chosenPose.AnimType == EMxMAnimtype.BlendSpace)
                m_desiredBlendSpacePosition.x = Mathf.Clamp(a_x, -1f, 1f);

            if (a_instant || m_blendSpaceSmoothing == EBlendSpaceSmoothing.None)
            {
                m_blendSpacePosition = m_desiredBlendSpacePosition;

                if (m_chosenPose.AnimType != EMxMAnimtype.BlendSpace)
                    return;

                ref BlendSpaceData curBlendSpace = ref CurrentAnimData.BlendSpaces[m_chosenPose.AnimId];
                BlendSpacePositionChanged(ref curBlendSpace, m_curBlendSpaceChannel);
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the blend space position of the current chosen blend space on the y axis
        *  
        *  @param [float] a_y - the y position in the blend space
        *         
        *********************************************************************************************/
        public void SetBlendSpacePositionY(float a_y, bool a_instant = false)
        {
            if (m_chosenPose.AnimType == EMxMAnimtype.BlendSpace)
            {
                m_desiredBlendSpacePosition.y = Mathf.Clamp(a_y, -1f, 1f);
            }

            if (a_instant || m_blendSpaceSmoothing == EBlendSpaceSmoothing.None)
            {
                m_blendSpacePosition = m_desiredBlendSpacePosition;

                if (m_chosenPose.AnimType != EMxMAnimtype.BlendSpace)
                    return;

                ref BlendSpaceData curBlendSpace = ref CurrentAnimData.BlendSpaces[m_chosenPose.AnimId];
                BlendSpacePositionChanged(ref curBlendSpace, m_curBlendSpaceChannel);
            }
        }

        //============================================================================================
        /**
        *  @brief Resets the blend space position to default values at 0, 0
        *         
        *********************************************************************************************/
        public void ResetBlendSpacePositions(bool a_instant = false)
        {
            if (m_chosenPose.AnimType == EMxMAnimtype.BlendSpace)
            {
                m_desiredBlendSpacePosition = new Vector2(0f, 0f);
            }

            if(a_instant || m_blendSpaceSmoothing == EBlendSpaceSmoothing.None)
            {
                m_blendSpacePosition = m_desiredBlendSpacePosition;

                if (m_chosenPose.AnimType != EMxMAnimtype.BlendSpace)
                    return;

                ref BlendSpaceData curBlendSpace = ref CurrentAnimData.BlendSpaces[m_chosenPose.AnimId];
                BlendSpacePositionChanged(ref curBlendSpace, m_primaryBlendChannel);
            }

            
        }

        //============================================================================================
        /**
        *  @brief Creates an animation mixer with clips set up to be used for a blend space
        *         
        *********************************************************************************************/
        public AnimationMixerPlayable CreateBlendSpacePlayable(int a_blendSpaceId)
        {
            if (a_blendSpaceId < 0
                || a_blendSpaceId > CurrentAnimData.BlendSpaces.Length - 1)
            {
                return AnimationMixerPlayable.Null;
            }

            ref readonly BlendSpaceData blendSpaceData = ref CurrentAnimData.BlendSpaces[a_blendSpaceId];

            AnimationMixerPlayable Mixer = AnimationMixerPlayable.Create(MxMPlayableGraph, blendSpaceData.ClipIds.Length);

            for (int i = 0; i < blendSpaceData.ClipIds.Length; ++i)
            {
                int clipId = blendSpaceData.ClipIds[i];

                AnimationClip clip = CurrentAnimData.Clips[clipId];
                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);

                if (clipPlayable.IsValid())
                {
                    Mixer.ConnectInput(i, clipPlayable, 0);
                    Mixer.SetInputWeight(i, 0f);
                }
            }


            Mixer.SetInputWeight(0, 1f);

            return Mixer;
        }

        //============================================================================================
        /**
        *  @brief Creates an animation mixer with clips set up to be used for a blend space
        *         
        *********************************************************************************************/
        public AnimationMixerPlayable CreateBlendSpacePlayable(MxMBlendSpace a_blendSpace)
        {
            if(a_blendSpace == null || a_blendSpace.Clips.Count == 0)
            {
                return AnimationMixerPlayable.Null;
            }

            AnimationMixerPlayable Mixer = AnimationMixerPlayable.Create(MxMPlayableGraph, a_blendSpace.Clips.Count);
            
            float firstClipLength = a_blendSpace.Clips[0].length;

            for (int i = 0; i < a_blendSpace.Clips.Count; ++i)
            {
                AnimationClip clip = a_blendSpace.Clips[i];

                if (clip == null)
                    continue;

                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
                
                if (clipPlayable.IsValid())
                {
                    Mixer.ConnectInput(i, clipPlayable, 0);
                    Mixer.SetInputWeight(i, 0f);

                    float clipSpeed = m_playbackSpeed;

                    if (a_blendSpace.NormalizeTime)
                    {
                        clipSpeed *= clip.length / firstClipLength;
                    }

                    clipPlayable.SetSpeed(clipSpeed);
                }
            }


            Mixer.SetInputWeight(0, 1f);

            return Mixer;
        }

    }//End of partial class: MxMAnimator
}//End of namespace: MxM
