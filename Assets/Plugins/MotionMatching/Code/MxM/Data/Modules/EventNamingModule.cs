using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    [CreateAssetMenu(fileName = "MxMEventModule", menuName = "MxM/Core/Modules/MxMEventModule", order = 3)]
    public class EventNamingModule : ScriptableObject
    {
        [SerializeField] private List<string> m_eventNames;

        public List<string> EventNames {  get { return m_eventNames; } }

        public void OnEnable()
        {
            if (m_eventNames == null)
                m_eventNames = new List<string>();
        }

    }//End of class: EventNamingModule
}//End of namespace: MxMEditor