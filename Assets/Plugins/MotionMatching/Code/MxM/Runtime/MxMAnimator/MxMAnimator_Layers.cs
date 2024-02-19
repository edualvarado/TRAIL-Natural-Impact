 // ================================================================================================
// File: MxMAnimator_Layers.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-10-10: Created this file.
// 
//     Contains a part of the 'MxM' namespace for 'Unity Engine'.
// ================================================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief This is partial implementation of the MxMAnimator. This particular partial class 
    *  handles all layer system logic for the MxMAnimator.
    *         
    *********************************************************************************************/
    public partial class MxMAnimator : MonoBehaviour
    {
        //Layers
        private Coroutine m_controllerFadeCoroutine; //Coroutine used for layer fading
        private Dictionary<int, MxMLayer> m_layers; //Map or layers. The index is the input ID on the MxM Layer Mixer
        private List<MxMLayer> m_transitioningLayers; //A list of layers that are currently requiring updates for transitions.

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private void UpdateLayers()
        {
            //Update MxMLayer Transitions
            for (int i = 0; i < m_transitioningLayers.Count; ++i)
            {
                if (m_transitioningLayers[i].UpdateTransition(p_currentDeltaTime))
                {
                    m_transitioningLayers.RemoveAt(i);
                    --i;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetLayerClip(int a_layerId, AnimationClip a_clip)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {

                layer.SetLayerClip(a_clip);
                layer.PrimaryClipTime = 0f;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set a layer clip on an invalid layer. " +
                    "Layer is out of range and does not exist. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the clip for a current layer instantly, removing any previous clip.
        *         
        *********************************************************************************************/
        public void SetLayerClip(int a_layerId, AnimationClip a_clip, AvatarMask a_mask,
            float a_time = 0f, float a_weight = 1f, bool a_applyFootIK = true, float a_playbackSpeed = 1f)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.ApplyHumanoidFootIK = a_applyFootIK;
                layer.SetLayerClip(a_clip, a_time);
                layer.Mask = a_mask;
                layer.Weight = a_weight;
                layer.PlaybackSpeed = a_playbackSpeed;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set a layer clip on an invalid layer. " +
                    "Layer is out of range and does not exist. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief
        *         
        *********************************************************************************************/
        public void SetLayerPlayable(int a_layerId, ref Playable a_playable, AvatarMask a_mask,
            float a_weight = 1f, bool a_applyuFootIK = true, float a_playbackSpeed = 1f)
        {
            MxMLayer layer = null;
            if(m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.ApplyHumanoidFootIK = a_applyuFootIK;
                layer.SetLayerPlayable(ref a_playable);
                layer.Mask = a_mask;
                layer.Weight = a_weight;
                layer.PlaybackSpeed = a_playbackSpeed;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set a layer playableon an invalid layer. " +
                    "Layer is out of range and does not exist. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the layer mask of any existing layer
        *         
        *********************************************************************************************/
        public void SetLayerMask(int a_layerId, AvatarMask a_mask)
        {
            MxMLayer layer = null;
            if (m_animationLayerMixer.IsValid() && m_layers.TryGetValue(a_layerId, out layer))
            {
                m_layers[a_layerId].Mask = a_mask;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set a layer mask before on an invalid layer " +
                    "id (out of range) or before the the playable graph is created. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void TransitionLayerClip(int a_layerId, AnimationClip a_clip, float a_fadeRate, float a_time = 0f)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.TransitionToClip(a_clip, a_fadeRate, a_time);

                if (!m_transitioningLayers.Contains(layer))
                    m_transitioningLayers.Add(layer);
                
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to transition layer clip on an invalid layer Id. Out of range. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void TransitionLayerPlayable(int a_layerId, ref Playable a_playable, float a_fadeRate, float a_time = 0f)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.TransitionToPlayable(ref a_playable, a_fadeRate, a_time);

                if (!m_transitioningLayers.Contains(layer))
                    m_transitioningLayers.Add(layer);
                
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to transition layer clip on an invalid layer Id. Out of range. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendInLayer(int a_layerId, float a_fadeRate, float a_targetWeight = 1f)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                if (layer.FadeCoroutine != null)
                    StopCoroutine(layer.FadeCoroutine);

                layer.FadeCoroutine = StartCoroutine(FadeInLayer(a_layerId, a_fadeRate, Mathf.Clamp01(a_targetWeight)));
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to blend in an invalid layer. Out of range. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendInLayerWithTime(int a_layerId, float a_blendTime, float a_targetWeight = 1f)
        {
            BlendInLayer(a_layerId, 1f / a_blendTime, a_targetWeight);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendOutLayer(int a_layerId, float a_fadeRate, float a_targetWeight = 0f)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                if (layer.FadeCoroutine != null)
                    StopCoroutine(layer.FadeCoroutine);

                layer.FadeCoroutine = StartCoroutine(FadeOutLayer(a_layerId, a_fadeRate, Mathf.Clamp01(a_targetWeight)));
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to blend out an invalid layer. Out of range. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendOutLayerWithTime(int a_layerId, float a_blendTime, float a_targetWeight = 0f)
        {
            BlendOutLayer(a_layerId, 1f / a_blendTime, a_targetWeight);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private IEnumerator FadeInLayer(int a_layerId, float a_fadeRate, float a_targetWeight)
        {
            if (a_layerId < m_animationLayerMixer.GetInputCount())
            {
                float curWeight = m_animationLayerMixer.GetInputWeight(a_layerId);

                while (curWeight < a_targetWeight)
                {
                    curWeight += a_fadeRate * p_currentDeltaTime * m_playbackSpeed;

                    if (curWeight > a_targetWeight)
                        curWeight = a_targetWeight;

                    m_animationLayerMixer.SetInputWeight(a_layerId, curWeight);

                    yield return null;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        private IEnumerator FadeOutLayer(int a_layerId, float a_fadeRate, float a_targetWeight)
        {
            if (a_layerId < m_animationLayerMixer.GetInputCount())
            {
                float curWeight = m_animationLayerMixer.GetInputWeight(a_layerId);

                while (curWeight > a_targetWeight)
                {
                    curWeight -= a_fadeRate * p_currentDeltaTime * m_playbackSpeed;

                    if (curWeight < a_targetWeight)
                        curWeight = a_targetWeight;

                    m_animationLayerMixer.SetInputWeight(a_layerId, curWeight);

                    yield return null;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendOutController(float a_fadeRate)
        {
            if (LayerMixerPlayable.GetInput(m_animControllerLayer).IsValid())
            {
                if (m_controllerFadeCoroutine != null)
                    StopCoroutine(m_controllerFadeCoroutine);

                m_controllerFadeCoroutine = StartCoroutine(FadeOutLayer(m_animControllerLayer, a_fadeRate, 0));
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to blend out animator controller layer but the layer is not valid. Aborting");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendOutControllerWithTime(float a_blendTime)
        {
            BlendOutController(1f / a_blendTime);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendInController(float a_fadeRate)
        {
            if (LayerMixerPlayable.GetInput(m_animControllerLayer).IsValid())
            {
                if (m_controllerFadeCoroutine != null)
                    StopCoroutine(m_controllerFadeCoroutine);

                m_controllerFadeCoroutine = StartCoroutine(FadeInLayer(m_animControllerLayer, a_fadeRate, 1f));
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to blend in animator controller layer but the layer is not valid.");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void BlendInControllerWithTime(float a_blendTime)
        {
            BlendInController(1f / a_blendTime);
        }

        //============================================================================================
        /**
        *  @brief Sets the animation mask to use for the animator controller layer if you are using 
        *  one. This enables control of masked layer animations directly through the Animator Controller
        *  
        *  @param [AvatarMask] a_mask - the avatar mask to use
        *         
        *********************************************************************************************/
        public void SetControllerMask(AvatarMask a_mask)
        {
            if (a_mask == null)
            {
                Debug.LogWarning("MxMAnimator: Cannot set animator controller mask on MxMAnimator with a null mask");
                return;
            }

            MxMLayer layer = null;

            if (m_layers.TryGetValue(m_animControllerLayer, out layer))
            {
                layer.Mask = a_mask;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set the animator controller mask but the layer is not valid.");
            }
        }

        //============================================================================================
        /**
        *  @brief Adds a new layer to the MxM layer stack given an animation clip to use for the layer
        *  playable setup.
        *         
        *********************************************************************************************/
        public int AddLayer(AnimationClip a_clip, float a_weight = 0f, bool a_additive = false, AvatarMask a_mask = null,
             bool a_applyFootIK=true, bool a_beforeController = false, int a_maxClips = 2)
        {
            if (a_clip == null)
            {
                Debug.LogError("MxMAnimator: Trying to add a layer but the passed Animation Clip is null. Aborting.");
                return -1;
            }

            int slotToUse = FindOrCreateLayerSlotToUse(a_beforeController);

            if (slotToUse > 0)
            {
                MxMLayer layer = new MxMLayer(slotToUse, a_maxClips, ref m_animationLayerMixer,
                    a_clip, a_mask, a_weight, a_additive, a_applyFootIK);

                m_layers.Add(slotToUse, layer);
            }

            return slotToUse;
        }

        //============================================================================================
        /**
        *  @brief  Adds a new layer to the MxM layer stack given a generic playable. This allows users
        *  to create their own custom playable setup on a layer and connect it to eh MxM layer stack.
        *         
        *********************************************************************************************/
        public int AddLayer(Playable a_playable, float a_weight = 0f, bool a_additive = false,
            AvatarMask a_mask = null, bool a_applyFootIK = true, bool a_beforeController = false, int a_maxClips = 2)
        {
            if (a_playable.IsNull())
            {
                Debug.LogWarning("MxMAnimator: Trying to add a layer but the passed Animation Clip is null. Aborting.");
                return -1;
            }

           int slotToUse = FindOrCreateLayerSlotToUse(a_beforeController);

            if (slotToUse > 0)
            {
                MxMLayer layer = new MxMLayer(slotToUse, a_maxClips, ref m_animationLayerMixer, a_playable,
                    a_mask, a_weight, a_additive, a_applyFootIK);

                m_layers.Add(slotToUse, layer);
            }

            return slotToUse;
        }

        //============================================================================================
        /**
        *  @brief Finds an empty layer slot to use for an animation layer in the MxMLayer stack. This
        *  is dependent on if the layer is intended to be before or after the controller. If a slot 
        *  cannot be found, one will be created if possible.
        *  
        *  @param [bool] a_beforeController - whether the slot needs to be before or after the Animator
        *  controller layer
        *         
        *********************************************************************************************/
        public int FindOrCreateLayerSlotToUse(bool a_beforeController)
        {
            int inputCount = m_animationLayerMixer.GetInputCount();
            int startIndex = a_beforeController ? 1 : m_animControllerLayer + 1;

            for (int i = startIndex; i < inputCount; ++i)
            {
                if (i == m_animControllerLayer)
                {
                    if (a_beforeController)
                        break;

                    continue;
                }

                Playable layerPlayable = m_animationLayerMixer.GetInput(i);

                if (layerPlayable.IsNull())
                {
                    return i;
                }
            }

            //If we manage to get here then it means no slot was found
            if (a_beforeController)
            {//There aren't enough slots before the controller slot so a slot cannot be made
                Debug.LogWarning("MxMAnimator: Trying to add a layer before the Animator " +
                    "Controller but there are no slots available. Consider increasing the " +
                    "Animator Controller starting layer. Aborting.");

                return -1;
            }
            else
            {//Here we create a new slot
                m_animationLayerMixer.SetInputCount(inputCount + 1);
                return inputCount;
            }
        }

        //============================================================================================
        /**
        *  @brief Returns the requested layer if it exists.
        *  
        *  @param [int] a_layerId - the id of the layer turn return (must be 2 or greater)
        *  
        *  @return [MxMLayer] - a reference to the requested layer or null if it doesn't exist
        *         
        *********************************************************************************************/
        public MxMLayer GetLayer(int a_layerId)
        {
            MxMLayer layer = null;

            if(!m_layers.TryGetValue(a_layerId, out layer))
            {
                Debug.LogWarning("MxMAnimator: Trying to get layer but the passed layerId is not valid. Returned layer will be null.");
            }

            return layer;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public int SetLayer(int a_layerId, AnimationClip a_clip, float a_weight = 0f, 
            bool a_additive = false, AvatarMask a_mask = null, bool a_applyFootIK=true, float a_playbackSpeed = 1f)
        {
            if (a_clip == null)
            {
                Debug.LogError("MxMAnimator: Trying to set a layer with a null AnimationClip. Aborting.");
                return -1;
            }

            MxMLayer layer = null;

            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.ApplyHumanoidFootIK = a_applyFootIK;
                layer.SetLayerClip(a_clip);
                layer.Mask = a_mask;
                layer.PlaybackSpeed = a_playbackSpeed;

                m_animationLayerMixer.SetInputWeight(a_layerId, a_weight);
                m_animationLayerMixer.SetLayerAdditive((uint)a_layerId, a_additive);

                return a_layerId;
            }
            else
            {
                Debug.LogError("MxMAnimator: Trying to set a layer clip and data but the layerId is invalid. Aborting.");
                return -1;
            }

        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public int SetLayer(int a_layerId, ref Playable a_playable, float a_weight = 0, 
            bool a_additive = false, AvatarMask a_mask = null, bool a_applyFootIK=true, float a_playbackSpeed=1f)
        {
            if (!a_playable.IsValid())
            {
                Debug.LogError("MxMAnimator: Trying to set a layer with a invalid playable. Aborting.");
                return -1;
            }

            MxMLayer layer = null;

            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.ApplyHumanoidFootIK = true;
                layer.SetLayerPlayable(ref a_playable);
                layer.PlaybackSpeed = a_playbackSpeed;

                m_animationLayerMixer.SetInputWeight(a_layerId, a_weight);
                m_animationLayerMixer.SetLayerAdditive((uint)a_layerId, a_additive);
                m_animationLayerMixer.SetLayerMaskFromAvatarMask((uint)a_layerId, a_mask);

                return a_layerId;
            }
            else
            {
                Debug.LogError("MxMAnimator: Trying to set a layer playable and data but the layerId is invalid. Aborting.");
                return -1;
            }
        }

        //============================================================================================
        /**
        *  @brief Removes an MxM layer from the playable graph
        *         
        *********************************************************************************************/
        public bool RemoveLayer(int a_layerId, bool a_destroyPlayable = true)
        {
            MxMLayer layer = null;

            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                m_animationLayerMixer.SetInputWeight(a_layerId, 0f);
                layer.ClearLayer();

                m_layers.Remove(a_layerId);
                return true;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to remove a layer but the layerId is invalid. Aborting");
                return false;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void RemoveAllLayers(bool a_destroyPlayable = true)
        {
            foreach (KeyValuePair<int, MxMLayer> pair in m_layers)
            {
                if (pair.Value != null)
                {
                    pair.Value.ClearLayer();
                }

                m_animationLayerMixer.SetInputWeight(pair.Key, 0f);
            }
        }

        //============================================================================================
        /**
        *  @brief Sets the playback speed of a specified layer
        *
        * @param [int] a_layerId - the id of the layer to set the speed of
        * @param [float] a_playbackSpeed - the playbackSpeed for the layer
        *         
        *********************************************************************************************/
        public void SetLayerSpeed(int a_layerId, float a_playbackSpeed)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.PlaybackSpeed = a_playbackSpeed;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set a layer playback speed by the layerId is invalid. Aborting.");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetLayerWeight(int a_layerId, float a_weight)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.Weight = a_weight;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set a layer weight by the layerId is invalid. Aborting.");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetControllerLayerWeight(float a_weight)
        {
            if (LayerMixerPlayable.GetInput(m_animControllerLayer).IsValid())
            {
                m_animationLayerMixer.SetInputWeight(m_animControllerLayer, Mathf.Clamp01(a_weight));
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set the layer weight of the animator controller " +
                    "but the controller layer is invalid. Aborting.");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetLayerAdditive(int a_layerId, bool a_additive)
        {
            MxMLayer layer = null;
            if (m_layers.TryGetValue(a_layerId, out layer))
            {
                layer.Additive = a_additive;
            }
            else
            {
                Debug.LogWarning("MxMAnimator: Trying to set layer additive but the layerId is invalid. Aborting.");
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void ResetLayerWeights()
        {
            if (!m_animationLayerMixer.IsNull())
            {
                m_animationLayerMixer.SetInputWeight(0, 1f);

                int inputCount = m_animationLayerMixer.GetInputCount();
                for (int i = 1; i < inputCount; ++i)
                {
                    m_animationLayerMixer.SetInputWeight(i, 0f);
                }
            }
        }

    }//End of partical class: MxMAnimator
}//End of namespace: MxM
