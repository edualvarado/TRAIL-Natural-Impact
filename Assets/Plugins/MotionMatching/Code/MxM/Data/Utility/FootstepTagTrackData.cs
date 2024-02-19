using UnityEngine;

namespace MxM
{
    [System.Serializable]
    public struct FootStepData
    {
        public EFootstepPace Pace;
        public EFootstepType Type;

        public FootStepData(EFootstepPace a_pace, EFootstepType a_type)
        {
            Pace = a_pace;
            Type = a_type;
        }
    }

    [System.Serializable]
    public class FootstepTagTrackData : GenericTagTrackData
    {
        public FootStepData[] FootSteps;

        public FootstepTagTrackData(int a_id, int a_numSteps) : base(a_id, a_numSteps)
        {
            FootSteps = new FootStepData[a_numSteps];
        }

        public void SetFootStep(int a_index, Vector2 a_range,
            EFootstepPace a_pace, EFootstepType a_type)
        {
            SetTag(a_index, a_range);

            if(a_index >= 0 && a_index < Tags.Length)
            {
                FootSteps[a_index] = new FootStepData(a_pace, a_type);
            }
        }

        public int GetStepStart(Vector2 a_range, ref int a_cachedLastStep)
        {
            for(int i=a_cachedLastStep; i < Tags.Length; ++i)
            {
                float start = Tags[i].x;

                if (start > a_range.y)
                {
                    a_cachedLastStep = i;
                    return -1;
                }
                else if(a_range.x <= start)
                {
                    a_cachedLastStep = i;
                    return i;
                }
            }

            a_cachedLastStep = Tags.Length;
            return -1;
        }

        public int IsGrounded(float a_time, ref int a_cachedLastStep)
        {
            for (int i = a_cachedLastStep; i < Tags.Length; ++i)
            {
                Vector2 tag = Tags[i];

                if (a_time < tag.x)
                {
                    a_cachedLastStep = i;
                    return i;
                }
                else if (a_time > tag.x && a_time < tag.y)
                {
                    a_cachedLastStep = i;
                    return i;
                }
            }

            a_cachedLastStep = Tags.Length;
            return -1;
        }

    }//End of class: FootstepTagTrackData
}//End of namespace: MxM