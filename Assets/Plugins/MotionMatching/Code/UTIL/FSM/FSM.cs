// ================================================================================================
// File: FSM.cs
// 
// Authors:  Kenneth Claassen
// Date:     2016-06-30: Created this file.
// 
//     Contains a part of the 'MxM' namespace for use with 'Unity Engine'.
// ================================================================================================
using System.Collections.Generic;

namespace MxM
{

    //=============================================================================================
    /**
    *  @brief This class is the basic Finite Staet Machine class used for managing simple state.s
    *         
    *  This class is used to manage states of being within an object. It was originally designed
    *  to be inherited by any object that would have states of being. However, it has been adapted
    *  here to behave as a component of a class using composition rather than inheritatnce. This 
    *  class will automatically manage the transitioning between states of being for an object. 
    *  The specific ins and outs of the states need to be derived from the FsmState class and
    *  added to the FSM as an ID'd state.
    *********************************************************************************************/
    public class FSM
    {
        private uint m_nextState;
        private uint m_curStateId;
        private readonly Dictionary<uint, FsmState> m_states;
        private FsmState m_currentState;

        public uint CurrentStateId { get { return m_curStateId ;} }

        //=========================================================================================
        /**
         *  @brief Constructor for the FSM class. It initialises the FSM with initial values.
         * 
         *****************************************************************************************/
        public FSM()
        {
            m_curStateId = m_nextState = 0;
            m_states = new Dictionary<uint, FsmState>();
        }

        public void Update_Phase2()
        {
            if (m_nextState == 0)
                m_currentState.Update_Phase2();
        }

        //=============================================================================================
        /**
        *  @brief Updates the FSM, updating the state and managing transitions.
        *  
        *  This class updates the FSM, which will inherantly update the current state of being and 
        *  manage the transitioning between states. As this class has been modified to work in 
        *  instead of by inheritance, this function must be called manually for the class that owns it.
        *********************************************************************************************/
        public void Update_Phase1()
        {
            if (m_nextState == 0)
            {
                m_currentState.Update_Phase1();
            }
            else
            {
                m_currentState.DoExit();
                m_currentState = m_states[m_nextState];
                m_currentState.DoEnter();
                m_currentState.Update_Phase1();
                m_curStateId = m_nextState;
                m_nextState = 0;
            }
        }

        //============================================================================================
        /**
        *  @brief Forces any transitions made in the last update to occur immediately.
        *         
        *********************************************************************************************/
        public void ForceTransitionNow()
        {
            if (m_nextState != 0)
            {
                m_currentState.DoExit();
                m_currentState = m_states[m_nextState];
                m_currentState.DoEnter();
                m_curStateId = m_nextState;
                m_nextState = 0;
            }
        }

        //============================================================================================
        /**
        *  @brief Called when a state is transitioned into. It calls that states DoEnter function
        *         
        *********************************************************************************************/
        public void DoEnter()
        {
            if(m_curStateId != 0)
                m_currentState.DoEnter();
        }

        //============================================================================================
        /**
        *  @brief Called when a state is transitioned out of. It calls that states DoExit function
        *         
        *********************************************************************************************/
        public void DoExit()
        {
            if (m_curStateId != 0)
                m_currentState.DoExit();
        }

        //=============================================================================================
        /**
        *  @brief Changes the state of being of a FSM.
        *  
        *  This function changes the state of being of the FSM by telling it to change to the ID'd
        *  state. The state change will not occur until the next frame. The state change will occur 
        *  within the FSM update function.
        *  
        *  @param _stateId - the state to which the FSM must transition to.
        *********************************************************************************************/
        public void GoToState(uint _stateId, bool _forceNow=false)
        {
            m_nextState = _stateId;

            if (_forceNow)
                ForceTransitionNow();
        }

        //=============================================================================================
        /**
        *  @brief Returns the Id'd state/
        *  
        *  This function returnes the FsmState that is Id'd as a parameter and is for use by the user
        *  at their discretion.
        *  
        *  @param _stateId - the id of the state to return
        *  
        *  @return the state that is Id'd
        *********************************************************************************************/
        public FsmState GetState(uint _stateId)
        {
            return m_states[_stateId];
        }

        //=============================================================================================
        /**
        *  @brief Checks whether a state key exists in the states list
        *  
        *  @param _stateId - the id of the state check
        *  
        *  @return true - the state does exist in the FSM
        *  @return false - the state does not exist in the FSM
        *********************************************************************************************/
        public bool StateExists(uint _stateId)
        {
            return m_states.ContainsKey(_stateId);
        }

        //=============================================================================================
        /**
        *  @brief Adds a staste of being to the finite state machine.
        *  
        *  This function adds a state of being to the state machine, give's it an id and makes it
        *  the current state if the user chooses. This of course will work even if the state of being
        *  is derived.
        *  
        *  @param _state - the state to be added to the FSM
        *  @param _stateId - the uint ID of the state to be added
        *  @param _makeCurrent - whether or not to make the state the currently active state or not.
        *********************************************************************************************/
        public void AddState(FsmState _state, uint _stateId, bool _makeCurrent=false)
        {
            m_states.Add(_stateId, _state);
            _state.SetParent(this);

            if (_makeCurrent)
            {
                m_curStateId = _stateId;
                m_currentState = _state;
                //m_currentState.DoEnter();
            }
        }

    } //End of class: FSM
} //End of namespace: MxM

