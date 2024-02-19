// ============================================================================================
// File: ETags.cs
// 
// Authors:  Kenneth Claassen
// Date:     2019-02-18: Created this file.
// 
//     Contains a part of the 'MxMEditor' namespace for 'Unity Engine.
// 
// Copyright (c) 2019 Kenneth Claassen. All rights reserved.
// ============================================================================================

namespace MxM
{
    //============================================================================================
    /**
    *  @brief 64Bit Bitmask for animation pose tags
    *         
    *********************************************************************************************/
    [System.Flags]
    public enum ETags
    {
        //This first value must be 0, DO  NOT CHANGE IT

        //Here are the custom tags that you can use for your game. Change the names to suite tags
        //that you want to use for your gameplay but do not change their values.
        None = 0,
        Tag1 = 1 << 0,
        Tag2 = 1 << 1,
        Tag3 = 1 << 2,
        Tag4 = 1 << 3,
        Tag5 = 1 << 4,
        Tag6 = 1 << 5,
        Tag7 = 1 << 6,
        Tag8 = 1 << 7,
        Tag9 = 1 << 8,
        Tag10 = 1 << 9,
        Tag11 = 1 << 10,
        Tag12 = 1 << 11,
        Tag13 = 1 << 12,
        Tag14 = 1 << 13,
        Tag15 = 1 << 14,
        Tag16 = 1 << 15,
        Tag17 = 1 << 16,
        Tag18 = 1 << 17,
        Tag19 = 1 << 18,
        Tag20 = 1 << 19,
        Tag21 = 1 << 20,
        Tag22 = 1 << 21,
        Tag23 = 1 << 22,
        Tag24 = 1 << 23,
        Tag25 = 1 << 24,
        Tag26 = 1 << 25,
        Tag27 = 1 << 26,
        Tag28 = 1 << 27,
        Tag29 = 1 << 28,
        Tag30 = 1 << 29,
        DoNotUse = 1 << 30,
        Reserved = 1 << 31

    }//End of enum: ETags

    public enum ETagBlendMethod
    {
        HighestWeight,
        Combine,
        AlwaysFormer,
        AlwaysLater
    }

    //============================================================================================
    /**
    *  @brief Static class used to easily manage ETag flags in an easily readable way.
    *         
    *********************************************************************************************/
    public static class Tags
    {
        public static ETags SetTag(ETags _source, ETags _tag)
        {
            return _source | _tag;
        }

        public static ETags UnsetTag(ETags _source, ETags _tag)
        {
            return _source & (~_tag);
        }

        public static bool HasTags(ETags _source, ETags _tag)
        {
            return (_source & _tag) == _tag;
        }

        public static ETags ToggleTag(ETags _source, ETags _tag)
        {
            return _source ^ _tag;
        }

    }//End of static class: Tags
    
    

}//End of namespace: MxM
