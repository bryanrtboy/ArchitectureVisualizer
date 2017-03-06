//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    public static class PhononSimulatorPane
    {
        public static void DrawPane()
        {
            if (targetObject == null || editor == null)
            {
                targetObject = PhononSimulationSettings.GetObject();
                editor = Editor.CreateEditor(targetObject.GetComponent<SimulationSettings>());
            }

            editor.OnInspectorGUI();
        }

        static GameObject targetObject = null;
        static Editor editor = null;
    }
}
