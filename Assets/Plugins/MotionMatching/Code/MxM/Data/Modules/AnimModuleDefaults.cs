using UnityEngine;
using MxM;

namespace MxMEditor
{
    [System.Serializable]
    public class AnimModuleDefaults
    {
        //Composites
        public bool IgnoreEdges;
        public bool Extrapolate = true;
        public bool FlattenTrajectory;
        public bool RuntimeSplicing;

        //Blend Spaces
        public EBlendSpaceType BlendSpaceType = EBlendSpaceType.ScatterX;
        public Vector2 ScatterSpacing = new Vector2(0.05f, 0.05f);
        public bool NormalizeBlendSpace = true;
        public Vector2 BlendSpaceMagnitude = Vector2.one;
        public Vector2 BlendSpaceSmoothing = new Vector2(0.25f, 0.25f);

        //Idle Sets
        public int MinLoops = 1;
        public int MaxLoops = 2;

        //General
        public ETags RequireTags;
        public ETags FavourTags;
    }
}