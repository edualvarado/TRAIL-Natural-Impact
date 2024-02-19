using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MxM
{
    public class MxMBlendSpaceLayers : MonoBehaviour, IMxMExtension
    {
        [SerializeField]
        private AvatarMask m_mask = null;

        [SerializeField]
        private int m_maxTransitionBlends = 2;

        [SerializeField]
        private bool m_useLayerBelowController = false;

        [SerializeField]
        private bool m_applyFootIk = false;

        [SerializeField]
        private bool m_additive = false;

        [SerializeField]
        private MxMBlendSpace[] m_blendSpaces = null;

        private Dictionary<MxMBlendSpace, MxMBlendSpaceState> m_blendSpaceStates;
        private MxMAnimator m_mxmAnimator;
        private AnimationMixerPlayable Mixer { get; set; }

        public MxMBlendSpace CurrentBlendSpace { get; private set; }
        public bool IsEnabled { get { return enabled; } }
        public bool DoUpdatePhase1 { get { return true; } }
        public bool DoUpdatePhase2 { get { return false; } }
        public bool DoUpdatePost { get { return false; } }

        public MxMLayer Layer { get; private set; }
        public int LayerId { get; private set; }

        private void OnDestroy()
        {
            foreach(KeyValuePair<MxMBlendSpace, MxMBlendSpaceState> pair in m_blendSpaceStates)
            {
                pair.Value.DisposeNativeData();
            }
        }

        public void Initialize()
        {
            m_mxmAnimator = GetComponent<MxMAnimator>();

            if (m_mxmAnimator == null)
            {
                Debug.LogError("Could not find MxMAnimator component, MxMBlendSpaceLayers component disabled");
                enabled = false;
                return;
            }

            m_blendSpaceStates = new Dictionary<MxMBlendSpace, MxMBlendSpaceState>(m_blendSpaces.Length + 1);
            CurrentBlendSpace = null;

            //Independent blend space assets
            for (int i = 0; i < m_blendSpaces.Length; ++i)
            {
                MxMBlendSpace blendSpace = m_blendSpaces[i];

                AddBlendSpace(blendSpace);
            }
        }

        public void AddBlendSpace(MxMBlendSpace a_blendSpace)
        {
            if (a_blendSpace == null)
                return;

            AnimationMixerPlayable BSMixer = m_mxmAnimator.CreateBlendSpacePlayable(a_blendSpace);

            if(Layer == null)
            {
                LayerId = m_mxmAnimator.AddLayer(BSMixer, 0f, m_additive, m_mask,
                           m_applyFootIk, m_useLayerBelowController, m_maxTransitionBlends);

                Layer = m_mxmAnimator.GetLayer(LayerId);
                CurrentBlendSpace = a_blendSpace;
            }

            m_blendSpaceStates.Add(a_blendSpace, new MxMBlendSpaceState(a_blendSpace, ref BSMixer));
            MxMBlendSpaceState bsState = m_blendSpaceStates[a_blendSpace];
            bsState.SetPosition(Vector2.zero);
            bsState.CalculateWeightings();
            bsState.ApplyWeightings();
        }

        public void SetBlendSpace(MxMBlendSpace a_blendSpace, float a_time = 0f)
        {
            if (a_blendSpace == null)
                return;

            MxMBlendSpaceState bsState;
            if(m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                CurrentBlendSpace = a_blendSpace;

                Playable bsMixer = m_mxmAnimator.CreateBlendSpacePlayable(a_blendSpace);
                
                //bsMixer.SetTime(a_time);

                bsState.Mixer = (AnimationMixerPlayable)bsMixer;

                bsState.CalculateWeightings();
                bsState.ApplyWeightings();
                bsState.SetTime(a_time);
                m_mxmAnimator.SetLayerPlayable(LayerId, ref bsMixer, m_mask, 1.0f, m_applyFootIk);
            }
            else
            {
                AddBlendSpace(a_blendSpace);
                SetBlendSpace(a_blendSpace, a_time);
            }
        }

        public void SetBlendSpace(string a_blendSpaceName, float a_time = 0f)
        {
            MxMBlendSpace blendSpace = GetBlendSpaceFromName(a_blendSpaceName);

            if (blendSpace == null)
                return;

            SetBlendSpace(blendSpace, a_time);
        }

        public void TransitionToBlendSpace(MxMBlendSpace a_blendSpace, float a_fadeRate = 0.4f, float a_time = 0f)
        {
            if (a_blendSpace == null)
                return;

            MxMBlendSpaceState bsState;

            if (m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                CurrentBlendSpace = a_blendSpace;

                Playable bsMixer = m_mxmAnimator.CreateBlendSpacePlayable(a_blendSpace);

                bsState.Mixer = (AnimationMixerPlayable)bsMixer;
                bsState.CalculateWeightings();
                bsState.ApplyWeightings();
                bsState.SetTime(a_time);
                m_mxmAnimator.TransitionLayerPlayable(LayerId, ref bsMixer, a_fadeRate, a_time);
            }
            else
            {
                AddBlendSpace(a_blendSpace);
                TransitionToBlendSpace(a_blendSpace, a_fadeRate, a_time);
            }
        }

        public void TransitionToBlendSpace(string a_blendSpaceName, float a_fadeRate = 0.4f, float a_time = 0f)
        {
            MxMBlendSpace blendSpace = GetBlendSpaceFromName(a_blendSpaceName);

            if (blendSpace == null)
                return;

            TransitionToBlendSpace(blendSpace, a_fadeRate, a_time);
        }

        public void UpdatePhase1()
        {
            if (CurrentBlendSpace == null)
                return;

            MxMBlendSpaceState bsState;
            if (m_blendSpaceStates.TryGetValue(CurrentBlendSpace, out bsState))
            {
                bsState.Update(m_mxmAnimator.CurrentDeltaTime, m_mxmAnimator.PlaybackSpeed);
            }
        }

        public void SetBlendSpacePosition(Vector2 a_position)
        {
            if (CurrentBlendSpace == null)
                return;
            
            MxMBlendSpaceState bsState;
            if (m_blendSpaceStates.TryGetValue(CurrentBlendSpace, out bsState))
            {
                a_position /= CurrentBlendSpace.Magnitude;
                bsState.SetPosition(a_position);
            }

        }

        public void SetBlendSpacePosition(Vector2 a_position, MxMBlendSpace a_blendSpace)
        {
            if (a_blendSpace == null)
                return;

            MxMBlendSpaceState bsState;
            if(m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                a_position /= a_blendSpace.Magnitude;
                bsState.SetPosition(a_position);
            }
        }

        public void SetBlendSpacePositionX(float a_positionX)
        {
            if (CurrentBlendSpace == null)
                return;
                
            MxMBlendSpaceState bsState;
            if (m_blendSpaceStates.TryGetValue(CurrentBlendSpace, out bsState))
            {
                a_positionX /= CurrentBlendSpace.Magnitude.x;
                bsState.SetPositionX(a_positionX);
            }
        }

        public void SetBlendSpacePositionX(float a_positionX, MxMBlendSpace a_blendSpace)
        {
            if (a_blendSpace == null)
                return;

            MxMBlendSpaceState bsState;

            if (m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                a_positionX /= a_blendSpace.Magnitude.x;
                bsState.SetPositionX(a_positionX);
            }
        }

        public void SetBlendSpacePositionY(float a_positionY)
        {
            

            if (CurrentBlendSpace == null)
                return;
            
            MxMBlendSpaceState bsState;
            if (m_blendSpaceStates.TryGetValue(CurrentBlendSpace, out bsState))
            {
                a_positionY /= CurrentBlendSpace.Magnitude.y;
                bsState.SetPositionY(a_positionY);
            }
        }

        public void SetBlendSpacePositionY(float a_positionY, MxMBlendSpace a_blendSpace)
        {
            if (a_blendSpace == null)
                return;

            MxMBlendSpaceState bsState;

            if (m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                a_positionY /= a_blendSpace.Magnitude.y;
                bsState.SetPositionY(a_positionY);
            }
        }

        public Vector2 GetPosition()
        {
            if (CurrentBlendSpace != null)
            {
                return m_blendSpaceStates[CurrentBlendSpace].Position * CurrentBlendSpace.Magnitude;
            }
            
            return Vector2.zero;
        }

        public Vector2 GetPosition(MxMBlendSpace a_blendSpace)
        {
            MxMBlendSpaceState bsState;
            if(m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                return bsState.Position * a_blendSpace.Magnitude;
            }

            return Vector2.zero;
        }

        public float GetPositionX()
        {
            if (CurrentBlendSpace == null)
                return 0f;
            
            return m_blendSpaceStates[CurrentBlendSpace].Position.x * CurrentBlendSpace.Magnitude.x;
        }

        public float GetPositionX(MxMBlendSpace a_blendSpace)
        {
            MxMBlendSpaceState bsState;
            if (m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                return bsState.Position.x * a_blendSpace.Magnitude.x;
            }

            return 0f;
        }

        public float GetPositionY()
        {
            if (CurrentBlendSpace != null)
            {
                return m_blendSpaceStates[CurrentBlendSpace].Position.y * CurrentBlendSpace.Magnitude.y;
            }

            return 0f;
        }

        public float GetPositionY(MxMBlendSpace a_blendSpace)
        {
            MxMBlendSpaceState bsState;

            if (m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                return bsState.Position.y * a_blendSpace.Magnitude.y;
            }

            return 0f;
        }

        public MxMBlendSpaceState GetBlendSpaceState(MxMBlendSpace a_blendSpace)
        {
            MxMBlendSpaceState bsState;

            if (m_blendSpaceStates.TryGetValue(a_blendSpace, out bsState))
            {
                return bsState;
            }

            return null;
        }

        public MxMBlendSpace GetBlendSpaceFromName(string a_blendSpaceName)
        {
            if(a_blendSpaceName == null)
            {
                Debug.LogWarning("MxMBlendSpaceLayers: Trying to get blendspace by name " +
                    "but the passed name is null. Returning null.");

                return null;
            }

            foreach(MxMBlendSpace blendSpace in m_blendSpaces)
            {
                if(a_blendSpaceName == blendSpace.BlendSpaceName)
                {
                    return blendSpace;
                }
            }

            Debug.LogWarning("MxMBlendSpaceLayers: Trying to get blend space by name but" +
                " the name does not match any listed blend spaces. Returning null.");

            return null;
        }

        public void Terminate() { }

        public void UpdatePhase2() { }
        public void UpdatePost() { }
    }

}//End of namespace: MxM