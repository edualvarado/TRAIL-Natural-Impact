using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace MxM
{
    public class MxMBlendSpaceState
    {
        private NativeArray<float> m_weightings;
        private NativeArray<float2> m_clipPositions;
        private Vector2 m_position = Vector2.zero;
        public MxMBlendSpace BlendSpace { get; private set; }
        public EBlendSpaceSmoothing Smoothing { get; set; }
        public Vector2 SmoothRate { get; set; }
        public Vector2 DesiredPosition { get; set; }

        public float Time { get; set; }
        public float PlayRate { get; set; }
        public float NormalizedLength { get; set; }
        public AnimationMixerPlayable Mixer { get; set; }

        public Vector2 Position
        {
            get { return m_position; }
            set { SetPosition(value); }
        }

        public MxMBlendSpaceState()
        {
            m_weightings = new NativeArray<float>(4, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            
            BlendSpace = null;
            Smoothing = EBlendSpaceSmoothing.None;
            SmoothRate = new Vector2(5f, 5f);
            DesiredPosition = m_position = Vector2.zero;
            Time = 0f;
            PlayRate = 1.0f;
            NormalizedLength = 0f;
            
        }
        public MxMBlendSpaceState(MxMBlendSpace blendSpace, ref AnimationMixerPlayable a_mixer)
        {
            PlayRate = 1.0f;
            DesiredPosition = m_position = Vector2.zero;
            BlendSpace = blendSpace;
            Mixer = a_mixer;
            Smoothing = EBlendSpaceSmoothing.None;
            Time = 0f;
            

            m_clipPositions = new NativeArray<float2>(blendSpace.Positions.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            for(int i = 0; i < blendSpace.Positions.Count; ++i)
            {
                m_clipPositions[i] = blendSpace.Positions[i];
            }

            m_weightings = new NativeArray<float>(blendSpace.Clips.Count, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            
            for (int i = 0; i < blendSpace.Clips.Count; ++i)
            {
                AnimationClip clip = blendSpace.Clips[i];

                if (clip)
                {
                    m_weightings[i] = (0);
                }
            }

            CalculateWeightings();
        }

        public void DisposeNativeData()
        {
            if(m_weightings.IsCreated)
                m_weightings.Dispose();

            if (m_clipPositions.IsCreated)
                m_clipPositions.Dispose();
        }

        public void SetPosition(Vector2 a_position)
        {
            if (BlendSpace == null
                || (a_position - m_position).SqrMagnitude() < 0.0001f)
            {
                return;
            }


            DesiredPosition = m_position = a_position;
            CalculateWeightings();
            ApplyWeightings();
        }

        public void SetPositionX(float a_positionX)
        {
            if (BlendSpace == null
                || Mathf.Abs(a_positionX - m_position.x) < 0.0001f)
            {
                return;
            }

            m_position.x = a_positionX;
            DesiredPosition = m_position;

            CalculateWeightings();
            ApplyWeightings();
        }

        public void SetPositionY(float a_positionY)
        {
            if (BlendSpace == null
                || Mathf.Abs(a_positionY - m_position.y) < 0.0001f)
            {
                return;
            }

            m_position.y = a_positionY;
            DesiredPosition = m_position;

            CalculateWeightings();
            ApplyWeightings();
        }

        public void Update(float a_deltaTime, float a_playbackSpeed)
        {
            if (BlendSpace == null
                || (m_position - DesiredPosition).SqrMagnitude() < 0.0001f)
            {
                return;
            }

            Vector2 lastPosition = m_position;

            switch (Smoothing)
            {
                case EBlendSpaceSmoothing.Lerp:
                    {
                        m_position = Vector2.Lerp(m_position,
                            DesiredPosition, SmoothRate.x * a_deltaTime * a_playbackSpeed * PlayRate * 60f);
                    }
                    break;
                case EBlendSpaceSmoothing.Lerp2D:
                    {
                        m_position.x = Mathf.Lerp(m_position.x, DesiredPosition.x,
                            SmoothRate.x * a_deltaTime * a_playbackSpeed * PlayRate * 60f);

                        m_position.y = Mathf.Lerp(m_position.y, DesiredPosition.x,
                            SmoothRate.y * a_deltaTime * a_playbackSpeed * PlayRate * 60f);

                    }
                    break;
                case EBlendSpaceSmoothing.Unique:
                    {
                        m_position = Vector2.Lerp(m_position,
                            DesiredPosition, BlendSpace.Smoothing.x * a_deltaTime * a_playbackSpeed * PlayRate * 60f);
                    }
                    break;
                case EBlendSpaceSmoothing.Unique2D:
                    {
                        m_position.x = Mathf.Lerp(m_position.x, DesiredPosition.x,
                            BlendSpace.Smoothing.x * a_deltaTime * a_playbackSpeed * PlayRate * 60f);

                        m_position.y = Mathf.Lerp(m_position.y, DesiredPosition.y,
                            BlendSpace.Smoothing.y * a_deltaTime * a_playbackSpeed * PlayRate * 60f);
                    }
                    break;
                default:
                    return;
            }


            if ((m_position - lastPosition).SqrMagnitude() < 0.00001f)
                return;

            CalculateWeightings();
            ApplyWeightings();
        }

        public void ApplyWeightings()
        {
            if (Mixer.IsValid())
            {
                int inputCount = Mixer.GetInputCount();
                for (int i = 0; i < inputCount; ++i)
                {
                    Mixer.SetInputWeight(i, m_weightings[i]);
                }

                //Modify clip speeds to normalize the blendspace length
                if (BlendSpace.NormalizeTime)
                {
                    for (int i = 0; i < inputCount; ++i)
                    {
                        var clipPlayable = (AnimationClipPlayable)Mixer.GetInput(i);
                        
                        clipPlayable.SetSpeed(PlayRate * (BlendSpace.Clips[i].length / NormalizedLength));
                    }
                }
            }
        }

        public void SetTime(float a_time)
        {
            if (Mixer.IsValid())
            {
                int inputCount = Mixer.GetInputCount();
                float baseClipTime = a_time * PlayRate;
                for (int i = 0; i < inputCount; ++i)
                {
                    var clipPlayable = (AnimationClipPlayable) Mixer.GetInput(i);

                    float clipTime = baseClipTime;
                    if (BlendSpace.NormalizeTime)
                    {
                        clipTime *= (BlendSpace.Clips[i].length / NormalizedLength);
                    }
                    
                    clipPlayable.SetTime(clipTime);
                }
            }
        }

        public void CalculateWeightings()
        {
            var bsJob = new CalculateBlendSpaceWeightingsJob()
            {
                Position = m_position,
                ClipPositions = m_clipPositions,
                ClipWeights = m_weightings
            };

            bsJob.Run();
            
            if(BlendSpace.NormalizeTime)
                CalculateNormalizedLength();
        }

        private void CalculateNormalizedLength()
        {
            NormalizedLength = 0f;
            for (int i = 0; i < BlendSpace.Clips.Count; ++i)
            {
                NormalizedLength += BlendSpace.Clips[i].length * m_weightings[i];
            }
        }

    }//End of class: MxMBlendSpaceState
}//End of namespace: MxM