using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

#if UNITY_2018_4 || UNITY_2019_1 || UNITY_2019_2
using UnityEngine.Experimental.Animations;
#elif UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#endif

namespace MxM
{
    [BurstCompile]
    public struct InertializerJob : IAnimationJob
    {
        public bool BlendActive; //If the blend is active or not
        public float DeltaTime; //The time delta for this frame
        public float RemainingTime; //The remaining blend time

        public NativeArray<TransformStreamHandle> TargetAnimationTransforms;
        public NativeArray<TransformSceneHandle> CurrentAnimationTransforms;
        public NativeArray<TransformData> PreviousAnimationTransforms;

        public void ProcessRootMotion(AnimationStream stream)
        {
            AnimationStream inputStream = stream.GetInputStream(0);

            stream.velocity = inputStream.velocity;
            stream.angularVelocity = inputStream.angularVelocity;
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            AnimationStream inputStream = stream.GetInputStream(0);

            if (BlendActive)
            {
                for (int i = 0; i < TargetAnimationTransforms.Length; ++i)
                {
                    TransformStreamHandle TargetTransform = TargetAnimationTransforms[i];
                    TransformSceneHandle CurrentTransform = CurrentAnimationTransforms[i];
                    TransformData PreviousTransform = PreviousAnimationTransforms[i];

                    float3 currentPos = float3.zero;
                    if (i == 0)
                    {
                        float3 targetPos = TargetTransform.GetLocalPosition(inputStream);
                        currentPos = CurrentTransform.GetLocalPosition(stream);

                        float3 pos = Inertialize(PreviousTransform.Position,
                           currentPos, targetPos, DeltaTime, RemainingTime, DeltaTime);

                        TargetTransform.SetLocalPosition(stream, pos);
                    }

                    quaternion targetRot = TargetTransform.GetLocalRotation(inputStream);
                    quaternion currentRot = CurrentTransform.GetLocalRotation(stream);

                    quaternion rot = Inertialize(PreviousTransform.Rotation,
                        currentRot, targetRot, DeltaTime, RemainingTime, DeltaTime);

                    TargetTransform.SetLocalRotation(stream, rot);

                    PreviousAnimationTransforms[i] = new TransformData(currentPos,
                        currentRot);
                }
            }
            else
            {
                for (int i = 0; i < TargetAnimationTransforms.Length; ++i)
                {
                    TransformStreamHandle TargetTransform = TargetAnimationTransforms[i];
                    TransformSceneHandle CurrentTransform = CurrentAnimationTransforms[i];

                    if (i == 0)
                        TargetTransform.SetLocalPosition(stream, TargetTransform.GetLocalPosition(inputStream));

                    TargetTransform.SetLocalRotation(stream, TargetTransform.GetLocalRotation(inputStream));

                    PreviousAnimationTransforms[i] = new TransformData(CurrentTransform.GetLocalPosition(stream),
                        CurrentTransform.GetLocalRotation(stream));
                }
            }
        }

        public static float3 Inertialize(float3 a_previous, float3 a_current, float3 a_target, float dt, float tf, float t)
        {
            float3 vx0 = a_current - a_target;
            float3 vxn1 = a_previous - a_target;

            float x0 = math.length(vx0);

            float3 vx0_dir = x0 > 0.00001f ? (vx0 / x0) : math.length(vxn1) > 0.00001f ? math.normalize(vxn1) : math.float3(1, 0, 0);

            float xn1 = math.dot(vxn1, vx0_dir);
            float v0 = (x0 - xn1) / dt;

            float xt = Inertialize(x0, v0, dt, tf, t);

            float3 vxt = xt * vx0_dir + a_target;

            return vxt;
        }

        public static quaternion Inertialize(quaternion a_previous, quaternion a_current, quaternion a_target, float dt, float tf, float t)
        {
            if (math.length(a_target) < 0.0001f)
            {
                a_target = quaternion.identity;
            }
            if (math.length(a_current) < 0.0001f)
            {
                a_current = quaternion.identity;
            }
            if (math.length(a_previous) < 0.0001f)
            {
                a_previous = quaternion.identity;
            }

            quaternion q0 = math.normalize(math.mul(a_current, math.inverse(a_target)));
            quaternion qn1 = math.normalize(math.mul(a_previous, math.inverse(a_target)));

            float4 q0_aa = ToAxisAngle(q0);

            float3 vx0 = q0_aa.xyz;
            float x0 = q0_aa.w;

            float xn1 = 2 * math.atan(math.dot(qn1.value.xyz, vx0) / qn1.value.w);

            float v0 = (x0 - xn1) / dt;

            float xt = Inertialize(x0, v0, dt, tf, t);
            quaternion qt = math.mul(quaternion.AxisAngle(vx0, xt), a_target);

            return math.normalize(qt);
        }

        public static float Inertialize(float x0, float v0, float dt, float tf, float t)
        {
            float tf1 = -5 * x0 / v0;

            if (tf1 > 0)
            {
                tf = math.min(tf, tf1);
            }

            if (tf < 0.00001f)
            {
                return 0f;
            }

            t = math.min(t, tf);

            float tf2 = tf * tf;
            float tf3 = tf2 * tf;
            float tf4 = tf3 * tf;
            float tf5 = tf4 * tf;

            float a0 = (-8 * v0 * tf - 20 * x0) / (tf * tf);

            float A = -(a0 * tf2 + 6 * v0 * tf + 12 * x0) / (2 * tf5);
            float B = (3 * a0 * tf2 + 16 * v0 * tf + 30 * x0) / (2 * tf4);
            float C = -(3 * a0 * tf2 + 12 * v0 * tf + 20 * x0) / (2 * tf3);

            float t2 = t * t;
            float t3 = t2 * t;
            float t4 = t3 * t;
            float t5 = t4 * t;

            return A * t5 + B * t4 + C * t3 + (a0 / 2) * t2 + v0 * t + x0;
        }

        public static float4 ToAxisAngle(quaternion a_quat)
        {
            float4 q1 = a_quat.value;

            if (q1.w > 1)
            {
                math.normalize(q1);
            }

            float angle = 2 * math.acos(q1.w);
            float s = math.sqrt(1 - q1.w * q1.w);

            if (s < 0.001f)
            {
                return math.float4(q1.x, q1.y, q1.z, angle);
            }
            else
            {
                return math.float4(q1.x / s, q1.y / s, q1.z / s, angle);
            }
        }
    }
}