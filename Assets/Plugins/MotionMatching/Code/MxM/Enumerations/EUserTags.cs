namespace MxM
{
    [System.Flags]
    public enum EUserTags
    {
        None = 0,
        UserTag1 = 1 << 0,
        UserTag2 = 1 << 1,
        UserTag3 = 1 << 2,
        UserTag4 = 1 << 3,
        UserTag5 = 1 << 4,
        UserTag6 = 1 << 5,
        UserTag7 = 1 << 6,
        UserTag8 = 1 << 7,
        UserTag9 = 1 << 8,
        UserTag10 = 1 << 9,
        UserTag11 = 1 << 10,
        UserTag12 = 1 << 11,
        UserTag13 = 1 << 12,
        UserTag14 = 1 << 13,
        UserTag15 = 1 << 14,
        UserTag16 = 1 << 15,
        UserTag17 = 1 << 16,
        UserTag18 = 1 << 17,
        UserTag19 = 1 << 18,
        UserTag20 = 1 << 19,
        UserTag21 = 1 << 20,
        UserTag22 = 1 << 21,
        UserTag23 = 1 << 22,
        UserTag24 = 1 << 23,
        UserTag25 = 1 << 24,
        UserTag26 = 1 << 25,
        UserTag27 = 1 << 26,
        UserTag28 = 1 << 27,
        UserTag29 = 1 << 28,
        UserTag30 = 1 << 29,
        UserTag31 = 1 << 30,
        UserTag32 = 1 << 31

    }//End of enum: EUserTags
    
    public enum EUserTagQueryMethod
    {
        /** User tags will be queried from the dominant pose. I.e. the pose with the most animation weighting*/
        DominantPose,
    
        /** User tags will be queried from the chosen pose. I.e. the pose that was last 'chosen' by the animation system*/
        ChosenPose,

    }//End of enum: EUserTagQueryMethod
}//End of namespace: MxM