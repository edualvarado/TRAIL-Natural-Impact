﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using MxM;

namespace MxMEditor
{

    public class MxMAssetHandler
    {
        [OnOpenAsset(1)]
        public static bool OpenAsset(int a_instanceId, int a_line)
        {
            Object asset = EditorUtility.InstanceIDToObject(a_instanceId);

            MxMAnimationIdleSet idleSet = asset as MxMAnimationIdleSet;
            if (idleSet)
            {
                MxMAnimConfigWindow.ShowWindow();
                MxMAnimConfigWindow.SetData(idleSet);
                return true;
            }

            MxMAnimationClipComposite composite = asset as MxMAnimationClipComposite;
            if(composite)
            {
                MxMAnimConfigWindow.ShowWindow();
                MxMAnimConfigWindow.SetData(composite);
                return true;
            }

            MxMBlendSpace blendSpace = asset as MxMBlendSpace;
            if(blendSpace)
            {
                MxMAnimConfigWindow.ShowWindow();
                MxMAnimConfigWindow.SetData(blendSpace);
                return true;
            }

            return false;
        }
    }
}