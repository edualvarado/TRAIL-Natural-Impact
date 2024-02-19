using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    [System.Serializable]
    public class TagTrackBase
    {
        [SerializeField] private int m_trackId = -1;
        [SerializeField] private string m_trackName = "Invalid";
        [SerializeField] protected float m_clipLength;
        [SerializeField] protected List<Vector2> m_tagPositions = new List<Vector2>();

        public string Name { get { return m_trackName; } set { m_trackName = value; } }
        public int TrackId { get { return m_trackId; } }
        public int SelectId { get; set; }
        public TagSelectType SelectType { get; set; }
        public List<Vector2> TagPositions { get { return m_tagPositions; } }
        public int TagCount { get { return m_tagPositions.Count; } }
        public bool DraggingSelected { get; set; }
        public float ClipLength { get { return m_clipLength; } set { m_clipLength = value; } }

        //============================================================================================
        /**
        *  @brief Constructor for the TagTrack class
        *  
        *  @param [int] _tagId - the id of the tag (relates to the tags in the animator
        *  @param [float] _clipLength -  the length of the animation clip the track belongs to
        *         
        *********************************************************************************************/
        public TagTrackBase(int a_id, string a_name, float a_clipLength)
        {
            m_trackId = a_id;
            m_trackName = a_name;
            m_clipLength = a_clipLength;
            m_tagPositions = new List<Vector2>();
            SelectId = -1;
            SelectType = TagSelectType.None;
        }

        //============================================================================================
        /**
        *  @brief Copy constructor for the TagTrack class
        *  
        *  @param [TagTrack] _copy - the track to copy from 
        *         
        *********************************************************************************************/
        public TagTrackBase(TagTrackBase a_copy)
        {
            m_trackId = a_copy.m_trackId;
            m_trackName = a_copy.m_trackName;
            m_clipLength = a_copy.m_clipLength;
            m_tagPositions = new List<Vector2>(a_copy.m_tagPositions);
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
        public virtual void AddTag(float a_point)
        {
            a_point = Mathf.Clamp(a_point, 0f, m_clipLength - Mathf.Min(m_clipLength / 2f, 0.05f));

            m_tagPositions.Add(new Vector2(a_point, a_point + Mathf.Min(m_clipLength - a_point, 0.25f)));
        }

        //============================================================================================
        /**
        *  @brief Adds a tag to the track given a specific range
        *  
        *  @param [float] _start - the start time of the tag
        *  @param [float] _end - the end time of the tag
        *         
        *********************************************************************************************/
        public virtual void AddTag(float _start, float _end)
        {
            m_tagPositions.Add(new Vector2(_start, _end));
        }

        //============================================================================================
        /**
        *  @brief Removes a tag by its ID if it exists
        *  
        *  @param [int] _id - The id of the tag to remove
        *         
        *********************************************************************************************/
        public virtual void RemoveTag(int _id)
        {
            if (_id < m_tagPositions.Count && _id >= 0)
                m_tagPositions.RemoveAt(_id);
        }

        //============================================================================================
        /**
        *  @brief Removes all tags that cover a specific time point
        *  
        *  @param [float] a_time - the time point to remove tags from
        *         
        *********************************************************************************************/
        public virtual void RemoveTags(float a_time)
        {
            if (a_time >= 0f && a_time <= m_clipLength)
            {
                for (int i = 0; i < m_tagPositions.Count; ++i)
                {
                    if (a_time > m_tagPositions[i].x && a_time < m_tagPositions[i].y)
                    {
                        m_tagPositions.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Removes all tags that overlap a specific time range
        *  
        *  @param [float] a_time - the time point to remove tags from
        *         
        *********************************************************************************************/
        public virtual void RemoveTags(float _start, float _end)
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
                    --i;
                }
            }
        }

        //============================================================================================
        /**
        *  @brief Removes all tags from the track
        *         
        *********************************************************************************************/
        public virtual void RemoveAllTags()
        {
            m_tagPositions.Clear();
            SelectId = -1;
            SelectType = TagSelectType.None;
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
        public Vector2 GetTag(int a_index)
        {
            if (a_index < m_tagPositions.Count && a_index >= 0)
                return m_tagPositions[a_index];

            return Vector2.zero;
        }

        //============================================================================================
        /**
        *  @brief Returns a tag that envelops a specific time
        *  
        *  @param [float] a_time - the time to sample
        *  
        *  @return Vector2 - the tag bounds of the id'd tag
        *         
        *********************************************************************************************/
        public Vector2 GetTag(float a_time)
        {
            if (a_time > 0f && a_time <= m_clipLength)
            {
                foreach (Vector2 tagRange in m_tagPositions)
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
        *  @param [float] a_time - the time to sample
        *  
        *  @return int - the id of the tag at that specific time
        *         
        *********************************************************************************************/
        public int GetTagId(float a_time)
        {
            for (int i = 0; i < m_tagPositions.Count; ++i)
            {
                if (a_time >= m_tagPositions[i].x - 0.0001f && a_time <= m_tagPositions[i].y + 0.0001f)
                    return i;
            }

            return -1;
        }

        //============================================================================================
        /**
        *  @brief Checks if a specific time on the track is tagged
        *  
        *  @param [float] a_time - the time to check
        *  
        *  @return bool - whether or not the time has been tagged
        *         
        *********************************************************************************************/
        public bool IsTimeTagged(float a_time)
        {
            for (int i = 0; i < m_tagPositions.Count; ++i)
            {
                if (a_time > m_tagPositions[i].x - 0.0001f
                    && a_time < m_tagPositions[i].y + 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public virtual void DrawOnTagData(int a_tagId, Rect a_rect)
        {

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
        **********************************************************************************************/
        public void DeleteSelectedTag()
        {
            if (SelectId > -1)
            {
                RemoveTag(SelectId);
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
        public virtual void VerifyData(AnimationClip a_clip)
        {
            if (!a_clip)
                return;

            m_clipLength = a_clip.length;
            
            if (m_tagPositions == null)
                m_tagPositions = new List<Vector2>();
            
            for (int i = 0; i < m_tagPositions.Count; ++i)
            {
                Vector2 tag = m_tagPositions[i];

                if (tag.x > a_clip.length)
                {
                    tag.x = a_clip.length - 0.01f;
                }

                if (tag.y > a_clip.length)
                {
                    tag.y = a_clip.length;
                }

                if (tag.x > tag.y)
                {
                    tag.x = tag.y - 0.01f;
                }

                m_tagPositions[i] = tag;
            }
        }

        public void OnDeleteTag(object a_eventObj)
        {
            int tagId = (int)a_eventObj;

            if (tagId > -1 && tagId < TagPositions.Count)
            {
                TagPositions.RemoveAt(tagId);
            }
        }

    }//End of class: TagTrackBase
}//End of namespace: MxMEditor