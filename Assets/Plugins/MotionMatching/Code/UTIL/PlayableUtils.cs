using UnityEngine.Playables;

namespace UTIL
{
    public static class PlayableUtils
    {
        public static void DestroyPlayableRecursive(ref Playable a_playable)
        {
            int count = a_playable.GetInputCount();

            for (int i = 0; i < count; ++i)
            {
                var playable = a_playable.GetInput(i);

                if (playable.IsValid())
                {
                    a_playable.DisconnectInput(i);
                    DestroyPlayableRecursive(ref playable); //Recursion
                }
            }

            a_playable.Destroy();
        }

        public static void DestroyChildPlayables(Playable a_playable)
        {
            int count = a_playable.GetInputCount();

            for(int i = 0; i < count; ++i)
            {
                var playable = a_playable.GetInput(i);

                if(playable.IsValid())
                {
                    DestroyPlayableRecursive(ref playable);
                }
            }
        }
    }
}
