using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{
    [System.Serializable]
    public class FloatTagTrack : TagTrackBase
    {
        [SerializeField]
        private List<float> m_values = new List<float>();

        [SerializeField]
        private float m_defaultValue = 1f;

        public List<float> Values { get { return m_values; } }

        public float DefaultValue { get { return m_defaultValue; } set { m_defaultValue = value; } }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public FloatTagTrack(int a_tagId, string a_name, float a_clipLength, float a_defaultValue = 1f)
            : base(a_tagId, a_name, a_clipLength)
        {
            m_defaultValue = a_defaultValue;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public FloatTagTrack(FloatTagTrack a_copy) : base(a_copy)
        {
            m_values = new List<float>(a_copy.m_values);
            m_defaultValue = a_copy.m_defaultValue;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetTagValue(int a_tagId, float a_value)
        {
            if(a_tagId < m_values.Count && a_tagId >= 0)
            {
                m_values[a_tagId] = a_value;
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public float GetTagValue(int a_tagId)
        {
            if (a_tagId < m_values.Count && a_tagId >= 0)
            {
                return m_values[a_tagId];
            }
            else
            {
                return m_defaultValue;
            }         
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void SetDefaultTagValue(float a_value)
        {
            m_defaultValue = a_value;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public float GetTagValue(float a_time)
        {
            if(a_time > 0f && a_time <= m_clipLength)
            {
                for(int i=0; i < m_tagPositions.Count; ++i)
                {
                    if(a_time >= m_tagPositions[i].x - Mathf.Epsilon && 
                        a_time <= m_tagPositions[i].y + Mathf.Epsilon)
                    {
                        return m_values[i];
                    }
                }
            }

            return m_defaultValue;
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

            m_values.Add(m_defaultValue);
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

            m_values.Add(m_defaultValue);
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
                m_values = new List<float>();

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
                    m_values.Add(0f);
                }
            }
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void DrawOnTagData(int a_tagId, Rect a_rect)
        {
            GUIContent tagDataLabel = new GUIContent(m_values[a_tagId].ToString());
            Vector2 size = GUI.skin.label.CalcSize(tagDataLabel);

            if (a_rect.width < size.x + 4f)
                return;

            float offset = (a_rect.width - size.x) / 2f;

            a_rect.x += offset - 2f;
            a_rect.width = size.x + 4f;

#if UNITY_2019_3_OR_NEWER
            a_rect.height = 18f;
            a_rect.y -= 6f;
#else
            a_rect.height = 20f;
            a_rect.y -= 9f;
#endif

            GUIStyle floatBoxStyle = new GUIStyle(GUI.skin.box);
#if UNITY_EDITOR            
            if (EditorGUIUtility.isProSkin)
            {
                floatBoxStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.5f, 0.5f, 1f));
            }
#endif

            GUI.Box(a_rect, "", floatBoxStyle);

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
            EditorGUI.LabelField(new Rect(a_rect.x + 3f, a_rect.y,
                size.x, 18f), m_values[a_tagId].ToString());
#elif UNITY_EDITOR
            EditorGUI.LabelField(new Rect(a_rect.x + 2f, a_rect.y + 2f,
                size.x, 18f), m_values[a_tagId].ToString());
#endif
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

    }//End of class: FloatTagTrack
}//End of namespace: MxMEditor