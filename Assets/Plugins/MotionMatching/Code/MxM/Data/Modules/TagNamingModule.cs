using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    [CreateAssetMenu(fileName = "MxMTagModule", menuName = "MxM/Core/Modules/MxMTagModule", order = 2)]
    public class TagNamingModule : ScriptableObject
    {
        [SerializeField] private List<string> m_tagNames;
        [SerializeField] private List<string> m_favourTagNames;
        [SerializeField] private List<string> m_userTagNames;

        public List<string> TagNames { get { return m_tagNames; } }
        public List<string> FavourTagNames { get { return m_favourTagNames; } }
        public List<string> UserTagNames { get { return m_userTagNames; } }

        public void OnEnable()
        {
            if (m_tagNames == null || m_tagNames.Count != 32)
            {
                if (m_tagNames == null)
                    m_tagNames = new List<string>(33);

                m_tagNames.Clear();

                for (int i = 1; i < 31; ++i)
                {
                    m_tagNames.Add("Tag " + i.ToString());
                }

                m_tagNames.Add("DoNotUse");
                m_tagNames.Add("Reserved");
            }

            if(m_favourTagNames == null || m_favourTagNames.Count != 32)
            {
                m_favourTagNames = new List<string>(33);

                m_favourTagNames.Clear();

                for (int i = 1; i < 33; ++i)
                {
                    m_favourTagNames.Add("Favour Tag " + i.ToString());
                }
            }

            if (m_userTagNames == null || m_userTagNames.Count != 32)
            {
                m_userTagNames = new List<string>(33);

                m_userTagNames.Clear();

                for (int i = 1; i < 33; ++i)
                {
                    m_userTagNames.Add("User Tag " + i.ToString());
                }
            }
        }
    }//End of class: TagDefinitionModule
}//End of namespace: MxMEditor