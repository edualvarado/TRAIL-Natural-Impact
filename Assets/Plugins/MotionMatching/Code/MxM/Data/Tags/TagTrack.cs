using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMEditor
{
    //===========================================================================================
    /**
    *  @brief Class used to hold data on a tag track.
    *  
    *  This is used specifically for the editor tagging system. Each animation node in 
    *         
    *********************************************************************************************/
    [System.Serializable]
    public class TagTrack
    {
        [SerializeField] private ETags m_tagId;
        [SerializeField] private string m_tagName;
        [SerializeField] private float m_clipLength;
        [SerializeField] private List<Vector2> m_tags = new List<Vector2>();

        public ETags TagId { get { return (ETags)m_tagId; } }
        public string TagName { get { return m_tagName; } set { m_tagName = value; } }
        public int SelectId { get; set; }
        public TagSelectType SelectType { get; set; }
        public List<Vector2> Tags { get { return m_tags; } }
        public float ClipLength { get { return m_clipLength; } set { m_clipLength = value; } }

        public bool DraggingSelected { get; set; }

        //============================================================================================
        /**
        *  @brief Constructor for the TagTrack class
        *  
        *  @param [int] _tagId - the id of the tag (relates to the tags in the animator
        *  @param [float] _clipLength -  the length of the animation clip the track belongs to
        *         
        *********************************************************************************************/
        public TagTrack(ETags _tagId, float _clipLength)
        {
            m_tagId = _tagId;
            m_clipLength = _clipLength;
            m_tags = new List<Vector2>();
        }

        //============================================================================================
        /**
        *  @brief Copy constructor for the TagTrack class
        *  
        *  @param [TagTrack] _copy - the track to copy from 
        *         
        *********************************************************************************************/
        public TagTrack(TagTrack _copy)
        {
            m_tagId = _copy.m_tagId;
            m_clipLength = _copy.m_clipLength;
            m_tags = new List<Vector2>(_copy.m_tags);
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
        public void AddTag(float a_point)
        {
            a_point = Mathf.Clamp(a_point, 0f, m_clipLength - Mathf.Min(m_clipLength / 2f, 0.05f));

            m_tags.Add(new Vector2(a_point, a_point + Mathf.Min(m_clipLength - a_point, 0.25f)));
        }

        //============================================================================================
        /**
        *  @brief Adds a tag to the track given a specific range
        *  
        *  @param [float] _start - the start time of the tag
        *  @param [float] _end - the end time of the tag
        *         
        *********************************************************************************************/
        public void AddTag(float a_start, float a_end)
        {
            m_tags.Add(new Vector2(a_start, a_end));
        }

        //============================================================================================
        /**
        *  @brief Removes a tag by its ID if it exists
        *  
        *  @param [int] _id - The id of the tag to remove
        *         
        *********************************************************************************************/
        public void RemoveTag(int a_id)
        {
            if(a_id < m_tags.Count && a_id >= 0)
                m_tags.RemoveAt(a_id);
        }

        //============================================================================================
        /**
        *  @brief Removes all tags that cover a specific time point
        *  
        *  @param [float] _time - the time point to remove tags from
        *         
        *********************************************************************************************/
        public void RemoveTags(float _time)
        {
            if(_time >=0f && _time <= m_clipLength)
            {
                for(int i=0; i < m_tags.Count; ++i)
                {
                    if(_time > m_tags[i].x && _time < m_tags[i].y)
                    {
                        m_tags.RemoveAt(i);
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
        public void RemoveTags(float _start, float _end)
        {
            _start = Mathf.Clamp(_start, 0f, m_clipLength);
            _end = Mathf.Clamp(_end, 0f, m_clipLength);

            for (int i = 0; i < m_tags.Count; ++i)
            {
                Vector2 tag = m_tags[i];

                if (tag.x > _start && tag.x < _end ||
                    tag.y > _start && tag.y < _end ||
                    _start > tag.x && _start < tag.y)
                {
                    m_tags.RemoveAt(i);
                    --i;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Returns a tag by its id
        *  
        *  @param [int] _id - The id of the tag to return
        *  
        *  @return Vector2 - the tag bounds of the id'd tag
        *         
        *********************************************************************************************/
        public Vector2 GetTag(int _id)
        {
            if (_id < m_tags.Count && _id >= 0)
                return m_tags[_id];

            return Vector2.zero;
        }

        //============================================================================================
        /**
        *  @brief Returns a tag that envelops a specific time
        *  
        *  @param [float] _time - the time to sample
        *  
        *  @return Vector2 - the tag bounds of the id'd tag
        *         
        *********************************************************************************************/
        public Vector2 GetTag(float a_time)
        {
            if (a_time > 0f && a_time <= m_clipLength)
            {
                foreach (Vector2 tagRange in m_tags)
                {
                    if (a_time >= tagRange.x && a_time <= tagRange.y)
                        return tagRange;
                }
            }

            return Vector2.zero;
        }

        //============================================================================================
        /**
        *  @brief Returns a tag id based on a specific time
        *  
        *  @param [float] _time - the time to sample
        *  
        *  @return int - the id of the tag at that specific time
        *         
        *********************************************************************************************/
        public int GetTagId(float a_time)
        {
            if (a_time > 0f && a_time <= m_clipLength)
            {
                for(int i=0; i < m_tags.Count; ++i)
                {
                    if (a_time >= m_tags[i].x && a_time <= m_tags[i].y)
                        return i;
                }
            }

            return -1;
        }

        //============================================================================================
        /**
        *  @brief Checks if a specific time on the track is tagged
        *  
        *  @param [float] _time - the time to check
        *  
        *  @return bool - whether or not the time has been tagged
        *         
        *********************************************************************************************/
        public bool IsTimeTagged(float a_time)
        {
            for (int i = 0; i < m_tags.Count; ++i)
            {
                if (a_time > (m_tags[i].x - 0.0001f) && a_time < (m_tags[i].y + 0.0001f))
                    return true;
            }

            return false;
        }

        //============================================================================================
        /**
        *  @brief Deselects all tags on the track
        *         
        *********************************************************************************************/
        public void Deselect()
        {
            SelectId = -1;
            SelectType = TagSelectType.None;
        }

        //============================================================================================
        /**
        *  @brief Deselects all tags on the track
        *         
        *********************************************************************************************/
        public void DeleteSelectedTag()
        {
            if(SelectId > -1)
            {
                m_tags.RemoveAt(SelectId);
                Deselect();
            }
        }

        //============================================================================================
        /**
        *  @brief Verifies the tag data compared to the passed clip. The passed clip should be the
        *  clip that this tag track is placed on
        *  
        *  @param [AnimationClip] a_clip - the clip to base verification on.
        *         
        *********************************************************************************************/
        public void VerifyData(AnimationClip a_clip)
        {
            if (!a_clip)
                return;

            m_clipLength = a_clip.length;
            
            for(int i=0; i < m_tags.Count; ++i)
            {
                Vector2 tag = m_tags[i];

                if(tag.x > a_clip.length)
                {
                    tag.x = a_clip.length - 0.01f;
                }

                if(tag.y > a_clip.length)
                {
                    tag.y = a_clip.length;
                }

                if(tag.x > tag.y)
                {
                    tag.x = tag.y - 0.01f;
                }

                m_tags[i] = tag;
            }
        }

        public void OnDeleteTag(object a_eventObj)
        {
            int tagId = (int)a_eventObj;

            if (tagId > -1 && tagId < Tags.Count)
            {
                Tags.RemoveAt(tagId);
            }
        }

    }//End of class: TagTrack
}//End of namespace: MxM
