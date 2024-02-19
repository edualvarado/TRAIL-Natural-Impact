using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

namespace MxMEditor
{
    public static class MxMFootstepDetector
    {
        public static void DetectFootsteps(IMxMAnim a_mxmAnim, GameObject a_targetModel, MxMPreProcessData a_preProcessData,
            AnimationModule a_animModule, float a_groundingThreshold, float a_minSpacing, float a_minDuration, float a_maxFootSpeed)
        {
            if ((a_preProcessData == null && a_animModule == null) || a_mxmAnim == null)
                return;

            AnimationClip targetClip = a_mxmAnim.TargetClip;

            if (targetClip == null)
                return;

            if(a_targetModel == null)
            {
                if (a_preProcessData == null)
                {
                    a_targetModel = a_preProcessData.Prefab;
                }
                else if(a_animModule == null)
                {
                    a_targetModel = a_animModule.Prefab;
                }

                if (a_targetModel == null)
                {
                    Debug.LogError("MxM Footstep Detection - The MxMAnim you are trying to detect footsteps for has no target" +
                        "model. This could occur if your target model is not set on the pre-processor, or your animation module" +
                        "doesn't have a MotionMatch Config referenced.");
                    return;
                }
            }

            List<FootStepData> leftFootStepData = new List<FootStepData>();
            List<Vector2> leftFootStepPositions = new List<Vector2>();

            List<FootStepData> rightFootStepData = new List<FootStepData>();
            List<Vector2> rightFootStepPositions = new List<Vector2>();

            GameObject model = GameObject.Instantiate(a_targetModel);
            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            Animator animator = model.GetComponent<Animator>();

            if(animator == null)
                animator = model.AddComponent<Animator>();

            animator.applyRootMotion = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Transform leftToeJoint = null;
            Transform rightToeJoint = null;

            bool getBonesByName = false;

            if(a_preProcessData != null)
            {
                getBonesByName = a_preProcessData.GetBonesByName;
            }
            else if(a_animModule != null)
            {
                getBonesByName = a_animModule.GetBonesByName;
            }

            if(!getBonesByName)
            {
                leftToeJoint = animator.GetBoneTransform(HumanBodyBones.LeftToes);
                rightToeJoint = animator.GetBoneTransform(HumanBodyBones.RightToes);

                if (leftToeJoint == null)
                    leftToeJoint = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

                if (rightToeJoint == null)
                    rightToeJoint = animator.GetBoneTransform(HumanBodyBones.RightFoot);

                if (leftToeJoint == null || rightToeJoint == null)
                {
                    Debug.LogError("Could not find toe or foot joint on humanoid body rig. " +
                                   "Aborting automatic footstep detection");
                    return;
                }
            }
            else
            {
                //Get generic joints?
                Debug.LogWarning("Automatic detection of footsteps is not currently supported for generic rigs");
                return;
            }

            PlayableGraph playableGraph = PlayableGraph.Create();
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            var animationMixer = AnimationMixerPlayable.Create(playableGraph, 1);
            playableOutput.SetSourcePlayable(animationMixer);
            
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, targetClip);
            clipPlayable.SetApplyFootIK(true);

            animationMixer.ConnectInput(0, clipPlayable, 0);
            animationMixer.SetInputWeight(0, 1f);

            clipPlayable.SetTime(0.0);
            clipPlayable.SetTime(0.0);
            playableGraph.Evaluate(0f);
            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            const float SixtyHz = 1f / 60f;

            float leftFootLowestY = leftToeJoint.position.y;
            float rightFootLowestY = rightToeJoint.position.y;
            for(float time = 0f; time <= targetClip.length; time += SixtyHz)
            {
                float leftFootY = leftToeJoint.position.y;
                float rightFootY = rightToeJoint.position.y;

                if (leftFootY < leftFootLowestY)
                    leftFootLowestY = leftFootY;

                if (rightFootY < rightFootLowestY)
                    rightFootLowestY = rightFootY;

                playableGraph.Evaluate(SixtyHz);
            }

            clipPlayable.SetTime(0.0);
            clipPlayable.SetTime(0.0);
            playableGraph.Evaluate(0f);
            bool leftFootGrounded = false;
            bool rightFootGrounded = false;

            float leftStepStartTime = 0f;
            float leftStepEndTime = 0f;

            float rightStepStartTime = 0f;
            float rightStepEndTime = 0f;

            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            float leftToeSpeed = 0f;
            float rightToeSpeed = 0f;

            Vector3 leftToeLastPos = leftToeJoint.position;
            Vector3 rightToeLastPos = rightToeJoint.position;

            for (float time = 0f; time <= targetClip.length; time += SixtyHz)
            {
                //Velocities
                leftToeSpeed = Vector3.Distance(leftToeLastPos, leftToeJoint.position) / SixtyHz;
                rightToeSpeed = Vector3.Distance(rightToeLastPos, rightToeJoint.position) / SixtyHz;
                leftToeLastPos = leftToeJoint.position;
                rightToeLastPos = rightToeJoint.position;

                //LEFT FOOT
                float leftFootDif = leftToeJoint.position.y - leftFootLowestY;
                if (leftFootDif < a_groundingThreshold && leftToeSpeed < a_maxFootSpeed)
                {
                    if (leftFootGrounded == false)
                    {
                        leftStepStartTime = time;
                    }

                    leftFootGrounded = true;
                }
                else
                {
                    if(leftFootGrounded == true)
                    {
                        leftStepEndTime = time;

                        leftFootStepData.Add(new FootStepData());
                        leftFootStepPositions.Add(new Vector2(leftStepStartTime, leftStepEndTime));
                    }

                    leftFootGrounded = false;
                }

                //RIGHT FOOT
                float rightFootDif = rightToeJoint.position.y - rightFootLowestY;

                if (rightFootDif < a_groundingThreshold && rightToeSpeed < a_maxFootSpeed)
                {
                    if(rightFootGrounded == false)
                    {
                        rightStepStartTime = time;
                    }

                    rightFootGrounded = true;
                }
                else 
                {
                    if(rightFootGrounded == true)
                    {
                        rightStepEndTime = time;

                        rightFootStepData.Add(new FootStepData());
                        rightFootStepPositions.Add(new Vector2(rightStepStartTime, rightStepEndTime));
                    }

                    rightFootGrounded = false;
                }

                playableGraph.Evaluate(SixtyHz);
            }

            List<TagTrackBase> genericTagTracks = a_mxmAnim.GenericTagTracks;
            FootStepTagTrack leftFootTagTrack = genericTagTracks[0] as FootStepTagTrack;
            leftFootTagTrack.RemoveAllTags();

            for (int i = 0; i < leftFootStepPositions.Count; ++i)
            {
                Vector2 footStepPosition = leftFootStepPositions[i];

                //Combine footsteps that are too close to be real (This should be recursive)
                if (i + 1 < leftFootStepPositions.Count)
                {
                    for (int k = i + 1; k < leftFootStepPositions.Count; ++k)
                    {
                        Vector2 nextFootStepPos = leftFootStepPositions[k];

                        if (nextFootStepPos.x - footStepPosition.y < a_minSpacing)
                        {
                            footStepPosition.y = nextFootStepPos.y;
                            leftFootStepPositions.RemoveAt(k);
                            --k;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //Ignore footsteps that are too short to be real
                if (footStepPosition.y - footStepPosition.x < a_minDuration)
                {
                    continue;
                }

                leftFootTagTrack.AddTag(footStepPosition.x, footStepPosition.y);
            }

            FootStepTagTrack rightFootTagTrack = genericTagTracks[1] as FootStepTagTrack;
            rightFootTagTrack.RemoveAllTags();

            for (int i=0; i < rightFootStepPositions.Count; ++i)
            {
                Vector3 footStepPosition = rightFootStepPositions[i];

                //Combine footsteps that are too close to be real (This should be recursive)
                if (i + 1 < rightFootStepPositions.Count)
                {
                    for (int k = i + 1; k < rightFootStepPositions.Count; ++k)
                    {
                        Vector2 nextFootStepPos = rightFootStepPositions[k];

                        if (nextFootStepPos.x - footStepPosition.y < a_minSpacing)
                        {
                            footStepPosition.y = nextFootStepPos.y;
                            rightFootStepPositions.RemoveAt(k);
                            --k;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //Ignore footsteps that are too short to be real
                if(footStepPosition.y - footStepPosition.x < a_minDuration)
                {
                    continue;
                }

                rightFootTagTrack.AddTag(footStepPosition.x, footStepPosition.y);
            }

            GameObject.DestroyImmediate(model);
        }
    }
}