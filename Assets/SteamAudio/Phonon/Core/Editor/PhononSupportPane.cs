//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    public static class PhononSupportPane
    {
        public static void DrawPane()
        {
            Phonon.PhononGUI.SectionHeader("Documentation");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Steam Audio comes with extensive documentation. Please download Steam Audio User's Manual by clicking on the button below.", MessageType.Info);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Get Docs"))
                Application.OpenURL("https://valvesoftware.github.io/steam-audio");
        }
    }
}