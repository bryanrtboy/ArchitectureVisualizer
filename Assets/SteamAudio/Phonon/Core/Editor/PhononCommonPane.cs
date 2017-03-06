//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    public static class PhononCommonPane
    {
        public static void DrawPane()
        {
            if (targetObject == null || editor == null)
            {
                targetObject = AudioEngineComponent.GetObject();
                editor = Editor.CreateEditor(targetObject.GetComponent<AudioEngineComponent>());
            }

            editor.OnInspectorGUI();
        }

        static GameObject targetObject = null;
        static Editor editor = null;
    }
}