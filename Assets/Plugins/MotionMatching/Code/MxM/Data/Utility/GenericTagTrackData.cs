using UnityEngine;

namespace MxM
{
    [System.Serializable]
    public class GenericTagTrackData
    {
        public int TrackId;
        public Vector2[] Tags;

        public GenericTagTrackData(int a_id, int a_numTags)
        {
            TrackId = a_id;
            Tags = new Vector2[a_numTags];
        }

        public void SetTag(int a_index, Vector2 a_range)
        {
            if (a_index >= 0 && a_index < Tags.Length)
            {
                Tags[a_index] = a_range;
            }
        }

        public int DoesRangeTriggerStart(float a_lastTime, float a_thisTime)
        {
            //TODO_KC: Don't cycle through all tags
            for(int i = 0; i < Tags.Length; ++i)
            {
                Vector2 tag = Tags[i];

                if(a_lastTime < tag.x && a_thisTime >= tag.x)
                {
                    return i;
                }
            }

            return -1;
        }

        public int DoesRangeTriggerEnd(float a_lastTime, float a_thisTime)
        {
            //TODO_KC: Don't cycle through all tags
            for (int i = 0; i < Tags.Length; ++i)
            {
                Vector2 tag = Tags[i];

                if (a_lastTime < tag.y && a_thisTime >= tag.y)
                {
                    return i;
                }
            }

            return -1;
        }

    }//End of class: GenericTagTrackData
}//End of namespace: MxM