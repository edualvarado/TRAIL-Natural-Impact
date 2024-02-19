using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    //===========================================================================================
    /**
    *  @brief Enumeration for detecting tag types
    *         
    *********************************************************************************************/
    public enum TagSelectType
    {
        None,
        All,
        Left,
        Right
    }//End of enum TagSelectTypes

    //===========================================================================================
    /**
    *  @brief A class used to describe a tag selection
    *         
    *********************************************************************************************/
    public struct TagSelection
    {
        public int TagTrackId;
        public int TagId;
        public TagSelectType SelectType;
    }//End of class: TagSelection;
}//End of namespace: MxMEditor
