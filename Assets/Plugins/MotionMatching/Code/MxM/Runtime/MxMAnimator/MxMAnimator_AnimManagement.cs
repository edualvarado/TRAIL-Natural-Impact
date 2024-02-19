// ================================================================================================
// File: MxMAnimator_AnimManagement.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UTIL;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This is partial implementation of the MxMAnimator. This particular partiSal class 
    *  handles all animation management (including playables) of the MxMAnimator, more specifically
    *  the changing of clips in the motion matchin mixer.
    *  
    *  For initial setup of the PlayableGraph, see MxMAnimator.cs.
    *         
    *********************************************************************************************/
    public partial class MxMAnimator : MonoBehaviour
    {
        private List<int> m_activeSlots = null; //A list of inputs to the animation mixer that are active
        private MxMPlayableState m_inertializationAnimState;
        
        //============================================================================================
        /**
        *  @brief Sets up a pose in a specific input on the Motion Matching mixer playable
        *  
        *  @param [ref PoseData] a_pose - the pose to extract the animation data from
        *  @param [int] a_slotId - the Id of the slot (or in put) to use
        *  @param [float] a_speedMod - any speed modification for animation playback (default 1f)
        *         
        *********************************************************************************************/
        private void SetupPoseInSlot(ref PoseData a_pose, int a_slotId, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
#if UNITY_2019_1_OR_NEWER
            if (p_riggingIntegration != null)
                p_riggingIntegration.FixRigTransforms();
#endif
            ClearPlayablesInSlot(a_slotId);

            switch (a_pose.AnimType)
            {
                case EMxMAnimtype.Composite: { SetupCompositeInSlot(ref a_pose, a_slotId, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.IdleSet: { SetupIdleClipInSlot(ref a_pose, a_slotId, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.BlendSpace: { SetupBlendSpaceInSlot(ref a_pose, a_slotId, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.Clip: { SetupClipInSlot(ref a_pose, a_slotId, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.BlendClip: { SetupBlendClipInSlot(ref a_pose, a_slotId, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.Sequence: { } break;
                case EMxMAnimtype.BlendSpace1D: { } break;
            }
        }

        //============================================================================================
        /**
        *  @brief Sets up a pose for inertial blending method (Experimental)
        *  
        *  In the case of inertial blending, playable clips are never destroyed. Rather each clip is
        *  setup at load time as a playable in a slot id which is the same as their clip Id. Therefore,
        *  no slot id is required
        *  
        *  @param [ref PoseData] a_pose - the pose to setup
        *  @param [float] a_speedMod - the speed modification to the clip playabck (default 1f)
        *         
        *********************************************************************************************/
        private void SetupPose(ref PoseData a_pose, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
            ClearActiveSlotWeights();

            switch (a_pose.AnimType)
            {
                case EMxMAnimtype.Composite: { SetupComposite (ref a_pose, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.IdleSet: { SetupIdleClip (ref a_pose, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.BlendSpace: { SetupBlendSpace (ref a_pose, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.Clip: { SetupClip (ref a_pose, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.BlendClip: { SetupBlendClip (ref a_pose, a_speedMod, a_timeOffset); } break;
                case EMxMAnimtype.Sequence: { } break;
                case EMxMAnimtype.BlendSpace1D: { } break;
            }
        }

        //============================================================================================
        /**
        *  @brief Recursively disconnects and destroys playables in the motion matching mixer input
        *  slot specified.
        *  
        *  @param [int] a_slotId - the slot or input Id to of the motion matching mixer to clear
        *         
        *********************************************************************************************/
        private void ClearPlayablesInSlot(int a_slotId)
        {
#if UNITY_2019_1_OR_NEWER
            if (p_riggingIntegration != null)
                p_riggingIntegration.FixRigTransforms();
#endif
            
            var playable = m_animationMixer.GetInput(a_slotId);

            if(playable.IsValid())
            {
                //m_animationMixer.DisconnectInput(a_slotId);
                PlayableUtils.DestroyPlayableRecursive(ref playable);
            }
        }

        //============================================================================================
        /**
        *  @brief Clears the weights of all input to the motion matching mixer to zero.
        *  
        *  This is only used with inertial blending (experimental) where playable clips are never 
        *  disconnected or destroyed. Instead of iterating through every single playable. The system
        *  keeps track of which inputId's currently have weight and only they are modified.
        *         
        *********************************************************************************************/
        private void ClearActiveSlotWeights()
        {
            for(int i = 0; i < m_activeSlots.Count; ++i)
            {
                int activeSlotId = m_activeSlots[i];

               // m_animationMixer.GetInput(activeSlotId).Pause(); //Todo: Double check if this even helps
                m_animationMixer.SetInputWeight(activeSlotId, 0f);
            }

            m_activeSlots.Clear();
        }
        
        //============================================================================================
        /**
        *  @brief Sets up a single clip in an input slot on the motion matching mixer.
        *  
        *  @param [ref PoseData] a_pose - a reference to the pose data containing the information on the clip
        *  @param [int] a_slotId - the input slot id to set the clip up in.
        *  @param [float] a_speedMod - the speed modification on the clip playback (default 1f)
        *         
        *********************************************************************************************/
        private void SetupIdleClipInSlot(ref PoseData a_pose, int a_slotId, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
            AnimationClip clip = CurrentAnimData.Clips[a_pose.PrimaryClipId];

            var clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
            clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
            clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
            
            float startTime = a_pose.Time + (m_timeSinceMotionChosen + a_timeOffset) * a_speedMod;
            clipPlayable.SetTime(startTime);
            clipPlayable.SetTime(startTime);
            
            clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod);
            
            //Todo: The weight here might be incorrect
            m_animationMixer.ConnectInput(a_slotId, clipPlayable, 0, p_currentDeltaTime + Mathf.Epsilon);
            
            ref MxMPlayableState playableState = ref m_animationStates[a_slotId];
            playableState.SetAsChosenWithPose(ref a_pose, m_timeSinceMotionChosen);
            playableState.TargetPlayable = clipPlayable;
        }
        
        //============================================================================================
        /**
        *  @brief The inertial blending (experimental) counterpart to SetupClipInSlot(...). 
        *  
        *  Instead of creating and destroying the appropriate playables, this function simply gives 
        *  the correct clip a correct weight.
        *         
        *********************************************************************************************/
        private void SetupIdleClip(ref PoseData a_pose, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
            m_animationMixer.SetInputWeight(a_pose.PrimaryClipId, 1f);
            var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(a_pose.PrimaryClipId);
            clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
            clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
            
            float startTime = a_pose.Time + (a_timeOffset * a_speedMod);
            clipPlayable.SetTime(startTime);
            clipPlayable.SetTime(startTime);
            clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod);
            //clipPlayable.Play();

            m_activeSlots.Add(a_pose.PrimaryClipId);
            
            m_inertializationAnimState.SetAsChosenWithPoseNoBlend(ref a_pose, m_timeSinceMotionChosen);
            m_inertializationAnimState.TargetPlayable = clipPlayable;
        }
        
        //============================================================================================
        /**
        *  @brief Sets up a single clip in an input slot on the motion matching mixer.
        *  
        *  @param [ref PoseData] a_pose - a reference to the pose data containing the information on the clip
        *  @param [int] a_slotId - the input slot id to set the clip up in.
        *  @param [float] a_speedMod - the speed modification on the clip playback (default 1f)
        *         
        *********************************************************************************************/
        private void SetupClipInSlot(ref PoseData a_pose, int a_slotId, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
            AnimationClip clip = CurrentAnimData.Clips[a_pose.PrimaryClipId];
            
            var clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
            clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
            clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
            
            m_clipSpeedMod = CurrentAnimData.ClipsData[a_pose.AnimId].PlaybackSpeed;
            float startTime = a_pose.Time + (m_timeSinceMotionChosen + a_timeOffset) * m_clipSpeedMod;
            clipPlayable.SetTime(startTime);
            clipPlayable.SetTime(startTime);
            
            clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);
            
            //Todo: The weight here might be incorrect
            m_animationMixer.ConnectInput(a_slotId, clipPlayable, 0, p_currentDeltaTime + Mathf.Epsilon);
            
            ref MxMPlayableState playableState = ref m_animationStates[a_slotId];
            playableState.SetAsChosenWithPose(ref a_pose, m_timeSinceMotionChosen);
            playableState.TargetPlayable = clipPlayable;
        }

        //============================================================================================
        /**
        *  @brief The inertial blending (experimental) counterpart to SetupClipInSlot(...). 
        *  
        *  Instead of creating and destroying the appropriate playables, this function simply gives 
        *  the correct clip a correct weight.
        *         
        *********************************************************************************************/
        private void SetupClip(ref PoseData a_pose, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
            m_animationMixer.SetInputWeight(a_pose.PrimaryClipId, 1f);
            
            var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(a_pose.PrimaryClipId);
            clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
            clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
            
            m_clipSpeedMod = CurrentAnimData.ClipsData[a_pose.AnimId].PlaybackSpeed;
            float startTime = a_pose.Time + (a_timeOffset * a_speedMod * m_clipSpeedMod);
            clipPlayable.SetTime(startTime);
            clipPlayable.SetTime(startTime);
            
            clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);
            //clipPlayable.Play();

            m_activeSlots.Add(a_pose.PrimaryClipId);
            
            m_inertializationAnimState.SetAsChosenWithPoseNoBlend(ref a_pose, m_timeSinceMotionChosen);
            m_inertializationAnimState.TargetPlayable = clipPlayable;
        }

        //============================================================================================
        /**
        *  @brief Sets up a composite (complex anim) in an input slot on the motion matching mixer.
        *  
        *  @param [ref PoseData] a_pose - a reference to the pose data containing the information on the composite
        *  @param [int] a_slotId - the input slot id to set the composite up in.
        *  @param [float] a_speedMod - the speed modification on the composite playback (default 1f)
        *         
        *********************************************************************************************/
        private void SetupCompositeInSlot(ref PoseData a_pose, int a_slotId, float a_speedMod, float a_timeOffset = 0f)
        {
            ref MxMPlayableState playableState = ref m_animationStates[a_slotId];
            playableState.SetAsChosenWithPose(ref a_pose, m_timeSinceMotionChosen);

            ref readonly CompositeData composite = ref CurrentAnimData.Composites[a_pose.AnimId];
            m_clipSpeedMod = composite.PlaybackSpeed;

            if (a_pose.Time < composite.ClipALength)
            {
                AnimationClip clipA = CurrentAnimData.Clips[composite.ClipIdA];
                AnimationClip clipB = CurrentAnimData.Clips[composite.ClipIdB];

                playableState.TargetPlayable = AnimationMixerPlayable.Create(MxMPlayableGraph, 2);
                var clipPlayableA = AnimationClipPlayable.Create(MxMPlayableGraph, clipA);
                var clipPlayableB = AnimationClipPlayable.Create(MxMPlayableGraph, clipB);
                clipPlayableA.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayableA.SetApplyPlayableIK(m_applyPlayableIK);
                clipPlayableB.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayableB.SetApplyPlayableIK(m_applyPlayableIK);
                

                float startTime = a_pose.Time + (m_timeSinceMotionChosen  + a_timeOffset) * a_speedMod * m_clipSpeedMod;
                clipPlayableA.SetTime(startTime);
                clipPlayableA.SetTime(startTime);
                clipPlayableA.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);

                playableState.TargetPlayable.ConnectInput(0, clipPlayableA, 0, 1f);
                playableState.TargetPlayable.ConnectInput(1, clipPlayableB, 0, 0f);

                m_animationMixer.ConnectInput(a_slotId, playableState.TargetPlayable, 0, p_currentDeltaTime + Mathf.Epsilon);
            }
            else
            {
                AnimationClip clipB = CurrentAnimData.Clips[composite.ClipIdB];

                var clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clipB);
                clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                
                float startTime = a_pose.Time + (m_timeSinceMotionChosen  + a_timeOffset) * a_speedMod * m_clipSpeedMod;
                clipPlayable.SetTime(startTime - composite.ClipALength);
                clipPlayable.SetTime(startTime - composite.ClipALength);
                clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);

                m_animationMixer.ConnectInput(a_slotId, clipPlayable, 0, p_currentDeltaTime + Mathf.Epsilon);
                
                playableState.TargetPlayable = clipPlayable;
            }
        }

        //============================================================================================
        /**
        *  @brief The inertial blending (experimental) counterpart to SetupCompositeInSlot(...). 
        *  
        *  Instead of creating and destroying the appropriate playables, this function simply gives 
        *  the correct clips the correct weight.
        *         
        *********************************************************************************************/
        private void SetupComposite(ref PoseData a_pose, float a_speedMod, float a_timeOffset = 0f)
        {
            m_inertializationAnimState.SetAsChosenWithPoseNoBlend(ref a_pose, m_timeSinceMotionChosen);
            
            ref readonly CompositeData composite = ref CurrentAnimData.Composites[a_pose.AnimId];
            m_clipSpeedMod = composite.PlaybackSpeed;

            if (a_pose.Time < composite.ClipALength)
            {
                m_animationMixer.SetInputWeight(composite.ClipIdA, 1f);
                var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(composite.ClipIdA);
                clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                
                float startTime = a_pose.Time + (a_timeOffset * a_speedMod * m_clipSpeedMod);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);
                //clipPlayable.Play();
                
                m_activeSlots.Add(composite.ClipIdA);
                m_inertializationAnimState.TargetPlayable = clipPlayable;
            }
            else
            {
                m_animationMixer.SetInputWeight(composite.ClipIdB, 1f);
                var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(composite.ClipIdB);
                clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                
                float startTime = a_pose.Time + (a_timeOffset * a_speedMod * m_clipSpeedMod);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);
                //clipPlayable.Play();
                
                m_activeSlots.Add(composite.ClipIdB);
                m_inertializationAnimState.TargetPlayable = clipPlayable;
            }
        }

        //============================================================================================
        /**
        *  @brief Sets up a blend space (complex anim) in an input slot on the motion matching mixer.
        *  
        *  @param [ref PoseData] a_pose - a reference to the pose data containing the information on the blend space
        *  @param [int] a_slotId - the input slot id to set the blend space up in.
        *  @param [float] a_speedMod - the speed modification on the blend space playback (default 1f)
        *         
        *********************************************************************************************/
        private void SetupBlendSpaceInSlot(ref PoseData a_pose, int a_slotId, float a_speedMod, float a_timeOffset = 0f)
        {
            ref MxMPlayableState playableState = ref m_animationStates[a_slotId];
            playableState.SetAsChosenWithPose(ref a_pose, m_timeSinceMotionChosen);

            ref readonly BlendSpaceData blendSpace = ref CurrentAnimData.BlendSpaces[a_pose.AnimId];

            playableState.TargetPlayable = AnimationMixerPlayable.Create(
                MxMPlayableGraph, blendSpace.ClipIds.Length);

            float blendSpaceLength = CurrentAnimData.Clips[blendSpace.ClipIds[0]].length;
            float normalizedClipSpeed = 1f;
            m_clipSpeedMod = blendSpace.PlaybackSpeed;
            
            for(int i = 0; i < blendSpace.ClipIds.Length; ++i)
            {
                AnimationClip clip = CurrentAnimData.Clips[blendSpace.ClipIds[i]];

                if (blendSpace.NormalizeTime)
                {
                    normalizedClipSpeed = clip.length / blendSpaceLength;
                }
                
                var blendClipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
                blendClipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                blendClipPlayable.SetApplyPlayableIK(m_applyPlayableIK);

                float startTime = a_pose.Time + (m_timeSinceMotionChosen  + a_timeOffset) * a_speedMod * normalizedClipSpeed * m_clipSpeedMod;
                blendClipPlayable.SetTime(startTime);
                blendClipPlayable.SetTime(startTime);
                blendClipPlayable.SetSpeed(m_playbackSpeed * a_speedMod  * m_clipSpeedMod * normalizedClipSpeed);
 
                playableState.TargetPlayable.ConnectInput(i, blendClipPlayable, 0, (i == 0 ? 1f : 0f));
            }

            m_curBlendSpaceChannel = a_slotId;

            m_animationMixer.ConnectInput(a_slotId, playableState.TargetPlayable, 0,
                p_currentDeltaTime + Mathf.Epsilon);
        }

        //============================================================================================
        /**
        *  @brief The inertial blending (experimental) counterpart to SetupBlendSpaceInSlot(...). 
        *  
        *  Instead of creating and destroying the appropriate playables, this function simply gives 
        *  the correct clips the correct weight.
        *         
        *********************************************************************************************/
        private void SetupBlendSpace(ref PoseData a_pose, float a_speedMod, float a_timeOffset = 0f)
        {
            ref readonly BlendSpaceData blendSpace = ref CurrentAnimData.BlendSpaces[a_pose.AnimId];
            m_inertializationAnimState.SetAsChosenWithPoseNoBlend(ref a_pose, m_timeSinceMotionChosen);
            
            float blendSpaceLength = CurrentAnimData.Clips[blendSpace.ClipIds[0]].length;
            float normalizedClipSpeed = 1f;
            m_clipSpeedMod = blendSpace.PlaybackSpeed;
            
            
            
            for (int i = 0; i < blendSpace.ClipIds.Length; ++i)
            {
                int clipId = blendSpace.ClipIds[i];

                if (i == 0)
                {
                    m_animationMixer.SetInputWeight(clipId, 1f);
                }
                else
                {
                    m_animationMixer.SetInputWeight(clipId, 0f);
                }

                var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(clipId);
                clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);

                if (blendSpace.NormalizeTime)
                {
                    AnimationClip clip = CurrentAnimData.Clips[blendSpace.ClipIds[0]];
                    normalizedClipSpeed = clip.length / blendSpaceLength;
                }

                float startTime = a_pose.Time + (a_timeOffset * a_speedMod * normalizedClipSpeed * m_clipSpeedMod);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * normalizedClipSpeed * m_clipSpeedMod);
                //clipPlayable.Play();
                
                m_activeSlots.Add(clipId);
                if (i == 0)
                {
                    m_inertializationAnimState.TargetPlayable = clipPlayable;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Sets up a blend clip (complex anim) in an input slot on the motion matching mixer.
        *  
        *  @param [ref PoseData] a_pose - a reference to the pose data containing the information on the blend clip
        *  @param [int] a_slotId - the input slot id to set the blend clip up in.
        *  @param [float] a_speedMod - the speed modification on the blend clip playback (default 1f)
        *         
        *********************************************************************************************/
        private void SetupBlendClipInSlot(ref PoseData a_pose, int a_slotId, float a_speedMod, float a_timeOffset = 0f)
        {
            ref MxMPlayableState playableState = ref m_animationStates[a_slotId];
            playableState.SetAsChosenWithPose(ref a_pose, m_timeSinceMotionChosen);

            ref readonly BlendClipData blendClip = ref CurrentAnimData.BlendClips[a_pose.AnimId];
            m_clipSpeedMod = blendClip.PlaybackSpeed;
            
            if (blendClip.ClipIds.Length == 1)
            {
                //Only one clip so don't need a mixer
                AnimationClip clip = CurrentAnimData.Clips[blendClip.ClipIds[0]];
                var clipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
                clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                
                float startTime = a_pose.Time + (m_timeSinceMotionChosen  + a_timeOffset) * a_speedMod * m_clipSpeedMod;
                clipPlayable.SetTime(startTime);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod);

                playableState.TargetPlayable = clipPlayable;
            }
            else
            {
                //The blend clip has multiple clips so a mixer is required
                playableState.TargetPlayable = AnimationMixerPlayable.Create(
                    MxMPlayableGraph, blendClip.ClipIds.Length);
                
                float normalizedClipSpeed = 1f;
                
                for (int i = 0; i < blendClip.ClipIds.Length; ++i)
                {
                    AnimationClip clip = CurrentAnimData.Clips[blendClip.ClipIds[i]];

                    if (blendClip.NormalizeTime)
                    {
                        normalizedClipSpeed = clip.length / blendClip.Length;
                    }

                    var blendClipPlayable = AnimationClipPlayable.Create(MxMPlayableGraph, clip);
                    blendClipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                    blendClipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                    float startTime = a_pose.Time + (m_timeSinceMotionChosen  + a_timeOffset) * m_playbackSpeed * a_speedMod * m_clipSpeedMod;
                    blendClipPlayable.SetTime(startTime * normalizedClipSpeed);
                    blendClipPlayable.SetTime(startTime * normalizedClipSpeed);
                    blendClipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * m_clipSpeedMod * normalizedClipSpeed);

                    playableState.TargetPlayable.ConnectInput(i, blendClipPlayable, 0, blendClip.Weightings[i]);
                }
            }

            m_curBlendSpaceChannel = a_slotId;
            
            m_animationMixer.ConnectInput(a_slotId, playableState.TargetPlayable, 0, 
                p_currentDeltaTime + Mathf.Epsilon);
        }

        //============================================================================================
        /**
        *  @brief The inertial blending (experimental) counterpart to SetupBlendClipInSlot(...). 
        *  
        *  Instead of creating and destroying the appropriate playables, this function simply gives 
        *  the correct clips the correct weight.
        *         
        *********************************************************************************************/
        private void SetupBlendClip(ref PoseData a_pose, float a_speedMod = 1f, float a_timeOffset = 0f)
        {
            ref readonly BlendClipData blendClip = ref CurrentAnimData.BlendClips[a_pose.AnimId];
            m_inertializationAnimState.SetAsChosenWithPoseNoBlend(ref a_pose, a_timeOffset);

            if (blendClip.ClipIds.Length == 1)
            {
                //Only one clip so don't need a mixer
                int clipId = blendClip.ClipIds[0];
                
                AnimationClip clip = CurrentAnimData.Clips[clipId];
                var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(clipId);
                clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                
                float startTime = a_pose.Time + m_timeSinceMotionChosen  + a_timeOffset;
                clipPlayable.SetTime(startTime);
                clipPlayable.SetTime(startTime);
                clipPlayable.SetSpeed(m_playbackSpeed * a_speedMod * blendClip.PlaybackSpeed);
               // clipPlayable.Play();

                m_activeSlots.Add(clipId);
                m_inertializationAnimState.TargetPlayable = clipPlayable;
            }
            else
            {
                float blendSpaceLength = CurrentAnimData.Clips[blendClip.ClipIds[0]].length;
                float normalizedClipSpeed = 1f;
                
                float unNormalizedPlaybackSpeed = m_playbackSpeed * a_speedMod * blendClip.PlaybackSpeed;
                for (int i = 0; i < blendClip.ClipIds.Length; ++i)
                {
                    int clipId = blendClip.ClipIds[i];
                    
                    if (blendClip.NormalizeTime)
                    {
                        AnimationClip clip = CurrentAnimData.Clips[clipId];
                        normalizedClipSpeed = clip.length / blendSpaceLength;
                    }

                    m_animationMixer.SetInputWeight(clipId, blendClip.Weightings[i]);
                    
                    var clipPlayable = (AnimationClipPlayable)m_animationMixer.GetInput(clipId);
                    clipPlayable.SetApplyFootIK(m_applyHumanoidFootIK);
                    clipPlayable.SetApplyPlayableIK(m_applyPlayableIK);
                    
                    float startTime = a_pose.Time + m_timeSinceMotionChosen + a_timeOffset;
                    clipPlayable.SetTime(startTime);
                    clipPlayable.SetTime(startTime);
                    clipPlayable.SetSpeed(unNormalizedPlaybackSpeed * normalizedClipSpeed);
                    //clipPlayable.Play();

                    m_activeSlots.Add(clipId);
                    m_inertializationAnimState.TargetPlayable = clipPlayable;
                }
            }
        }
    }//End of partial class: MxMAnimator
}//End of namespace: MxM