using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Collections;
using UnityEngine.Animations;

#if UNITY_2018_4 || UNITY_2019_1 || UNITY_2019_2
using UnityEngine.Experimental.Animations;
#endif

namespace MxM
{
    public class InertialBlendModule
    {
        private AnimationScriptPlayable m_intertialPlayable;

        private bool m_blending;
        private float m_remainingTime;

        private Animator m_animator;
        private AvatarMask m_avatarMask;

        private Transform[] m_transforms;

        //Job Data
        private NativeArray<TransformStreamHandle> m_targetAnimationTransforms;
        private NativeArray<TransformSceneHandle> m_currentAnimationTransforms;
        private NativeArray<TransformData> m_previousAnimationTransforms;

        public ref AnimationScriptPlayable Initialize(Animator a_animator, AvatarMask a_avatarMask, PlayableGraph a_playableGraph, AnimationLayerMixerPlayable a_layerMixer)
        {
            m_animator = a_animator;
            m_avatarMask = a_avatarMask;

            CollectAndBindAnimationTransforms();

            var inertializerJob = new InertializerJob()
            {
                BlendActive = false,
                DeltaTime = Time.deltaTime,
                RemainingTime = 0f,
                TargetAnimationTransforms = m_targetAnimationTransforms,
                CurrentAnimationTransforms = m_currentAnimationTransforms,
                PreviousAnimationTransforms = m_previousAnimationTransforms
            };

            m_intertialPlayable = AnimationScriptPlayable.Create(a_playableGraph, inertializerJob);
            m_intertialPlayable.SetTraversalMode(PlayableTraversalMode.Mix);
            m_intertialPlayable.SetInputCount(1);
            m_intertialPlayable.SetInputWeight(0, 1f);
            m_intertialPlayable.SetProcessInputs(false);

            m_intertialPlayable.ConnectInput(0, a_layerMixer, 0);

            return ref m_intertialPlayable;
        }

        public void UpdateTransition()
        {
            if(m_blending)
            {
                var inertializerJob = m_intertialPlayable.GetJobData<InertializerJob>();

                m_remainingTime -= Time.deltaTime;

                if(m_remainingTime <= 0f)
                {
                    inertializerJob.BlendActive = false;
                }
                else
                {
                    inertializerJob.BlendActive = true;
                    inertializerJob.DeltaTime = Time.deltaTime;
                    inertializerJob.RemainingTime = Mathf.Max(0.0001f, m_remainingTime);
                }

                m_intertialPlayable.SetJobData(inertializerJob);
            }
        }

        public void BeginTransition(float a_blendDuration)
        {
            m_remainingTime = a_blendDuration;
            m_blending = true;
        }

        private void CollectAndBindAnimationTransforms()
        {
            List<Transform> transforms = new List<Transform>(60);

            if (m_animator.isHuman)
            {
                for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
                {
                    var boneTransform = m_animator.GetBoneTransform((HumanBodyBones)i);

                    if (boneTransform != null)
                    {
                        transforms.Add(boneTransform);
                    }
                }
            }

            if (m_avatarMask != null)
            {
                //All non human bones
                for (int i = 0; i < m_avatarMask.transformCount; ++i)
                {
                    var jointTransformPath = m_avatarMask.GetTransformPath(i);
                    var jointTransform = m_animator.transform.Find(jointTransformPath);

                    if (jointTransform != null)
                    {
                        transforms.Add(jointTransform);
                    }
                }
            }

            m_transforms = transforms.ToArray();

            m_targetAnimationTransforms = new NativeArray<TransformStreamHandle>(m_transforms.Length, Allocator.Persistent);
            m_currentAnimationTransforms = new NativeArray<TransformSceneHandle>(m_transforms.Length, Allocator.Persistent);
            m_previousAnimationTransforms = new NativeArray<TransformData>(m_transforms.Length, Allocator.Persistent);

            for (int i = 0; i < m_transforms.Length; ++i)
            {
                Transform tform = m_transforms[i];

                m_targetAnimationTransforms[i] = m_animator.BindStreamTransform(tform);
                m_currentAnimationTransforms[i] = m_animator.BindSceneTransform(tform);
                m_previousAnimationTransforms[i] = new TransformData(tform.position, tform.rotation);
            }
        }    

        public void DisposeNativeData()
        {
            if (m_targetAnimationTransforms.IsCreated)
                m_targetAnimationTransforms.Dispose();

            if (m_currentAnimationTransforms.IsCreated)
                m_currentAnimationTransforms.Dispose();

            if (m_previousAnimationTransforms.IsCreated)
                m_previousAnimationTransforms.Dispose();
        }

    }//End of class: InertialBlendModule
}//End of namespace: MxM
