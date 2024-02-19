using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMEditor
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
    public class FootStepTagTrack : TagTrackBase
    {
        [SerializeField]
        private List<FootStepData> m_footStepData = new List<FootStepData>();

        public int StepCount { get { return m_footStepData.Count; } }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public (Vector2 range, FootStepData step) GetFootStepData(int a_index)
        {
            if (a_index < 0 || a_index > m_footStepData.Count - 1)
                return (Vector2.zero, new FootStepData());

            return (m_tagPositions[a_index], m_footStepData[a_index]);
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public FootStepTagTrack(int a_tagId, string a_name, float a_clipLength)
            : base(a_tagId, a_name, a_clipLength)
        {

        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public FootStepTagTrack(FootStepTagTrack a_copy) : base(a_copy)
        {
           for(int i=0; i < a_copy.m_footStepData.Count; ++i)
            {
                m_footStepData.Add(a_copy.m_footStepData[i]);
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetStepDataToAll(EFootstepPace a_pace, EFootstepType a_type)
        {
            for(int i=0; i < m_footStepData.Count; ++i)
            {
                m_footStepData[i] = new FootStepData(a_pace, a_type);
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetStepData(int a_tagId, EFootstepPace a_pace, EFootstepType a_type)
        {
            if (a_tagId < 0 || a_tagId > m_footStepData.Count)
                return;

            m_footStepData[a_tagId] = new FootStepData(a_pace, a_type);
        }

        //============================================================================================
        /**
        *  @brief Adds a tag to the track at a specific time point. 
        *  
        *  The tag length will either be 0.5f or half the clip length, whichever is smallest
        *  
        *  @param [float] _point - the time point to add the the tag at
        *         
        *********************************************************************************************/
        public override void AddTag(float _point)
        {
            base.AddTag(_point);

            m_footStepData.Add(new FootStepData());
        }

        //============================================================================================
        /**
        *  @brief Adds a tag to the track given a specific range
        *  
        *  @param [float] _start - the start time of the tag
        *  @param [float] _end - the end time of the tag
        *         
        *********************************************************************************************/
        public override void AddTag(float _start, float _end)
        {
            base.AddTag(_start, _end);

            m_footStepData.Add(new FootStepData());
        }

        //============================================================================================
        /**
        *  @brief Removes a tag by its ID if it exists
        *  
        *  @param [int] _id - The id of the tag to remove
        *         
        *********************************************************************************************/
        public override void RemoveTag(int _id)
        {
            if (_id < m_tagPositions.Count && _id >= 0)
            {
                m_tagPositions.RemoveAt(_id);
                m_footStepData.RemoveAt(_id);
            }
        }

        //============================================================================================
        /**
        *  @brief Removes all tags that cover a specific time point
        *  
        *  @param [float] _time - the time point to remove tags from
        *         
        *********************************************************************************************/
        public override void RemoveTags(float _time)
        {
            if (_time >= 0f && _time <= m_clipLength)
            {
                for (int i = 0; i < m_tagPositions.Count; ++i)
                {
                    if (_time > m_tagPositions[i].x && _time < m_tagPositions[i].y)
                    {
                        m_tagPositions.RemoveAt(i);
                        m_footStepData.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Removes all tags that overlap a specific time range
        *  
        *  @param [float] _time - the time point to remove tags from
        *         
        *********************************************************************************************/
        public override void RemoveTags(float _start, float _end)
        {
            _start = Mathf.Clamp(_start, 0f, m_clipLength);
            _end = Mathf.Clamp(_end, 0f, m_clipLength);

            for (int i = 0; i < m_tagPositions.Count; ++i)
            {
                Vector2 tag = m_tagPositions[i];

                if (tag.x > _start && tag.x < _end ||
                    tag.y > _start && tag.y < _end ||
                    _start > tag.x && _start < tag.y)
                {
                    m_tagPositions.RemoveAt(i);
                    m_footStepData.RemoveAt(i);
                    --i;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Removes all tags from the track
        *         
        *********************************************************************************************/
        public override void RemoveAllTags()
        {
            base.RemoveAllTags();
            m_footStepData.Clear();


        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void VerifyData(AnimationClip a_clip)
        {
            base.VerifyData(a_clip);

            if (m_footStepData == null)
                m_footStepData = new List<FootStepData>();

            if(m_footStepData.Count > m_tagPositions.Count)
            {
                int dif = m_footStepData.Count - m_tagPositions.Count;

                for(int i=0; i < dif; ++i)
                {
                    m_footStepData.RemoveAt(m_footStepData.Count - 1);
                }
            }
            else if(m_footStepData.Count < m_tagPositions.Count)
            {
                int dif = m_tagPositions.Count - m_footStepData.Count;

                for(int i=0; i < dif; ++i)
                {
                    m_footStepData.Add(new FootStepData());
                }
            }
        }


    }//End of class: FootStepTagTrack
}//End of namespace: MxMEditor