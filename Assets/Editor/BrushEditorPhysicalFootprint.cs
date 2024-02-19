/****************************************************
 * File: BrushEditorPhysicalFootprint.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 07/11/2022
   * Project: RL-terrain-adaptation
   * Last update: 14/11/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BrushPhysicalFootprint), true)]
public class BrushEditorPhysicalFootprint : Editor
{
    #region Read-only & Static Fields

    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    #endregion

    #region Instance Methods

    public override void OnInspectorGUI()
	{
        BrushPhysicalFootprint myBrush = (BrushPhysicalFootprint)target;

        // To start brush by default
        if (!myBrush.IsActive() && Application.isPlaying)
        {
            myBrush.Toggle();
        }

        if (myBrush.IsActive())
            DrawDefaultInspector();

        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        GUIStyle style = myBrush.IsActive() ? ToggleButtonStyleToggled : ToggleButtonStyleNormal;
        if (GUILayout.Button("Use", style))
        {
            myBrush.Toggle();
        }
    }

	#endregion
}
