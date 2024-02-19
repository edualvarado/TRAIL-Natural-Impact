using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    [System.Serializable]
    public class BoolTagTrack : TagTrackBase
    {
        [SerializeField]
        private List<bool> m_values = new List<bool>();

        public List<bool> Values { get { return m_values; } }

        public BoolTagTrack(int a_tagId, string a_name, float a_clipLength)
            : base(a_tagId, a_name, a_clipLength)
        {

        }

        public BoolTagTrack(BoolTagTrack a_copy) : base(a_copy)
        {
            m_values = new List<bool>(a_copy.m_values);
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

            m_values.Add(false);
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

            m_values.Add(false);
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
                m_values.RemoveAt(_id);
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
                        m_values.RemoveAt(i);
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
                    m_values.RemoveAt(i);
                    --i;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void VerifyData(AnimationClip a_clip)
        {
            base.VerifyData(a_clip);

            if (m_values == null)
                m_values = new List<bool>();

            if (m_values.Count > m_tagPositions.Count)
            {
                int dif = m_values.Count - m_tagPositions.Count;

                for (int i = 0; i < dif; ++i)
                {
                    m_values.RemoveAt(m_values.Count - 1);
                }
            }
            else if (m_values.Count < m_tagPositions.Count)
            {
                int dif = m_tagPositions.Count - m_values.Count;

                for (int i = 0; i < dif; ++i)
                {
                    m_values.Add(false);
                }
            }
        }

    }//End of class: BoolTagTrack
}//End of namespace: MxMEditor