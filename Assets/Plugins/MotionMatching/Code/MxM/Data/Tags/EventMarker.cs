using System.Collections.Generic;
using UnityEngine;
using MxM;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief Structure for holding data on events.
    *         
    *********************************************************************************************/
    [System.Serializable]
    public class EventMarker
    {
        [SerializeField] public int EventId;
        [SerializeField] public string EventName;
        [SerializeField] public float EventTime;
        [SerializeField] public float Windup;
        [SerializeField] public List<float> Actions;
        [SerializeField] public float FollowThrough;
        [SerializeField] public float Recovery;
        [SerializeField] public List<EventContact> Contacts;

        public bool Selected { get; set; }
        public bool Dragging { get; set; }

        //============================================================================================
        /**
        *  @brief constructor for event marker struct
        *         
        *********************************************************************************************/
        public EventMarker(int _eventId, float _eventTime,
            EventContact[] _contacts = null, float[] _actions = null, float _windup = 0.2f,
            float _followThrough = 0.2f, float _recovery = 0.2f)
        {
            EventId = _eventId;
            EventTime = _eventTime;
            Windup = _windup;
            Actions = new List<float>();
            FollowThrough = _followThrough;
            Recovery = _recovery;
            Contacts = new List<EventContact>();

            if (_contacts != null)
            {
                for (int i = 0; i < _contacts.Length; ++i)
                {
                    Contacts.Add(_contacts[i]);
                }
            }

            if (Contacts.Count == 0)
                Contacts.Add(new EventContact());

            if (_actions != null)
            {               
                for (int i = 0; i < _actions.Length; ++i)
                { 
                    Actions.Add(_actions[i]);
                }
            }

            if (Actions.Count == 0)
                Actions.Add(0.2f);
        }

        //============================================================================================
        /**
        *  @brief Copy constructor
        *         
        *********************************************************************************************/
        public EventMarker(EventMarker _copy)
        {
            EventId = _copy.EventId;
            EventTime = _copy.EventTime;
            Windup = _copy.Windup;
            Actions = new List<float>(_copy.Actions);
            FollowThrough = _copy.FollowThrough;
            Recovery = _copy.Recovery;
            Contacts = new List<EventContact>(_copy.Contacts);
        }

        //============================================================================================
        /**
        *  @brief Deselects the event marker
        *         
        *********************************************************************************************/
        public void Deselect()
        {
            Selected = false;
        }

        //============================================================================================
        /**
        *  @brief Validates that the event Ids and event name match up with Event definitions
        *         
        *********************************************************************************************/
        public void Validate(string[] a_eventNames)
        {
            if (a_eventNames == null)
                return;

            if (EventName == null || EventName == "")
            {
                if(EventId < 0 || EventId >= a_eventNames.Length)
                {
                    EventId = -1;
                    EventName = "";
                }
                else
                {
                    EventName = a_eventNames[EventId];
                }
            }
            else
            {
                //Find the Id from the name
                bool found = false;
                for(int i = 0; i < a_eventNames.Length; ++i)
                {
                    if(a_eventNames[i] == EventName)
                    {
                        found = true;
                        EventId = i;
                        break;
                    }
                }

                //If no id of that name is found reset the event to null
                if(!found)
                {
                    Debug.LogWarning("Event marker name: '" + EventName + "' does not exist. Have you deleted and event Id that was being used by an event marker?" +
                        "The marker event Id and name has been set to null.");
                    EventId = -1;
                }
            }
        }

    }//End of class: EventMarker
}//End of namespace: MxM
