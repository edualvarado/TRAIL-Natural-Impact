// ================================================================================================
// File: FSMState.cs
// 
// Authors:  Kenneth Claassen
// Date:     2016-06-30: Created this file.
// 
//     Contains a part of the 'MxM' namespace for use with 'Unity Engine'.
// ================================================================================================

namespace MxM
{

    //=============================================================================================
    /**
    *  @brief This is the FsmState class which is used as the base class for any state of being
    *  of a Finite State Machine.
    *         
    * This class should be used as the base class for any state of being of an object with a 
    * finite state machine. The derived state should represent a single behaviour type (e.g. idle,
    * or running). By adding a state to an finite state machine (FSM) the states will be
    * automatically managed, so long as the DoEnter(), DoExit() and Update() virtual functions
    * are overriden in an appropriate way to represent a behaviour and transition in and out
    * of a state.
    *********************************************************************************************/
    public abstract class FsmState
    {
        [System.NonSerialized]
        protected FSM p_parent;

        //=============================================================================================
        /**
        *  @brief A virtual function that when overriden should do all the code for transitioning into
        *  the state.
        *  
        *  This function is called immediately after transitioning into a state of being. In general
        *  it will be called immediately after a DoExit() function of another state. It simply sets
        *  up the state just before it takes control as the primary state. Overriding this function
        *  may not be necessary if no setup is required.
        *********************************************************************************************/
        public abstract void DoEnter();

        //=============================================================================================
        /**
        *  @brief A virtual function that when overriden should do all the code for transitioning out
        *  of the state.
        *  
        *  This function is automatically called by the FSM when the state is current and the state
        *  is changed to another state. It should be overriden (if necessary) with any code required
        *  to finish a state of being.
        *********************************************************************************************/
        public abstract void DoExit();

        //=============================================================================================
        /**
        *  @brief Updates the state every frame that it is the active state.
        *  
        *  This function updates the state of being everytime is is the current, active state within
        *  an FSM. It will be automatically called from the parent FSM's update routine and it should 
        *  hold all the logic required to control a state of being throughout every frame.
        *********************************************************************************************/
        public abstract void Update_Phase1();

        //=============================================================================================
        /**
        *  @brief 
        *  
        *********************************************************************************************/
        public abstract void Update_Phase2();

        //=============================================================================================
        /**
        *  @brief Sets the parent FSM for the state of being.
        *  
        *  This function is called from an FSM when the state is added to it. It basically gives the
        *  state a reference back to the FSM so that it can request a change in state within itself
        *  if requried.
        *  
        *  @param FSM - a reference to the parent FSM to set.
        *  
        *********************************************************************************************/
        public void SetParent(FSM _parent)
        {
            p_parent = _parent;
        }

    } //End of Class: FSMState
} //End of namespace: MxM

