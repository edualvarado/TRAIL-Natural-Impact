using System.Collections.Generic;
using UnityEngine;
using MxM;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MxMEditor
{

    [System.Serializable]
    public class CompositeCategory
    {
        public string CatagoryName;
        public List<MxMAnimationClipComposite> Composites;

        public bool IgnoreEdges_default;
        public bool Extrapolate_default = true;
        public bool FlattenTrajectory_default;
        public bool RuntimeSplicing_default;
        public ETags RequireTags_default;
        public ETags FavourTags_default;

        public CompositeCategory(string a_name)
        {
            CatagoryName = a_name;
            Composites = new List<MxMAnimationClipComposite>();
        }


        public CompositeCategory(CompositeCategory a_copy, ScriptableObject a_parentObj, bool a_mirrored=false)
        {
#if UNITY_EDITOR
            CatagoryName = a_copy.CatagoryName;
            Composites = new List<MxMAnimationClipComposite>(a_copy.Composites.Count + 1);

            IgnoreEdges_default = a_copy.IgnoreEdges_default;
            Extrapolate_default = a_copy.Extrapolate_default;
            FlattenTrajectory_default = a_copy.FlattenTrajectory_default;
            RuntimeSplicing_default = a_copy.RuntimeSplicing_default;
            RequireTags_default = a_copy.RequireTags_default;
            FavourTags_default = a_copy.FavourTags_default;

            foreach(MxMAnimationClipComposite sourceComposite in a_copy.Composites)
            {
                MxMAnimationClipComposite newComposite = ScriptableObject.CreateInstance<MxMAnimationClipComposite>();
                newComposite.CopyData(sourceComposite, a_mirrored);
                newComposite.name = sourceComposite.name;
                newComposite.CompositeName = sourceComposite.CompositeName;
                newComposite.hideFlags = HideFlags.HideInHierarchy;

                MxMPreProcessData targetPreProcess = a_parentObj as MxMPreProcessData;
                AnimationModule targetAnimModule = a_parentObj as AnimationModule;

                if(targetPreProcess != null)
                {
                    newComposite.TargetPreProcess = targetPreProcess;
                    newComposite.TargetAnimModule = null;
                }
                else if(targetAnimModule != null)
                {
                    newComposite.TargetPreProcess = null;
                    newComposite.TargetAnimModule = targetAnimModule;
                }

                EditorUtility.SetDirty(newComposite);

                if (a_parentObj != null)
                {
                    AssetDatabase.AddObjectToAsset(newComposite, a_parentObj);
                }
                
                Composites.Add(newComposite);
            }
#endif
        }
    }//End of class: CompositeCategory
}//End of namespace: MxMEditor
