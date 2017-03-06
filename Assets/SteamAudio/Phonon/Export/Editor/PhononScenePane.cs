//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Phonon
{
    public static class PhononScenePane
    {
        public static void DrawPane()
        {
            if (targetObject == null || editor == null)
            {
                targetObject = PhononMaterialSettings.GetObject();
                editor = Editor.CreateEditor(targetObject.GetComponent<PhononMaterial>());
            }

            editor.OnInspectorGUI();

            PhononGUI.SectionHeader("Export Phonon Geometry");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");

            if (GUILayout.Button("Export to OBJ"))
                EnvironmentComponent.DumpScene();

            if (GUILayout.Button("Pre-Export Scene"))
                EnvironmentComponent.ExportScene();

            EditorGUILayout.EndHorizontal();
        }

        static GameObject targetObject = null;
        static Editor editor = null;
    }
}