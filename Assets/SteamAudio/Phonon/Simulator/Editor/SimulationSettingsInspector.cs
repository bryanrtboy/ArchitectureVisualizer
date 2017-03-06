//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;
using UnityEditor;
using System;

namespace Phonon
{

    //
    // SimulationSettingsInspector
    // Custom inspector for a SimulationSettings component.
    //

    [CustomEditor(typeof(SimulationSettings))]
    public class SimulationSettingsInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PhononGUI.SectionHeader("Quality Preset");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Preset"));

            if (serializedObject.FindProperty("Preset").enumValueIndex < 3)
            {
                SimulationSettingsValue actualValue = ((SimulationSettings)target).Value;
                actualValue.CopyFrom(SimulationSettingsPresetList.PresetValue(serializedObject.FindProperty("Preset").enumValueIndex));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Value"));

                if (Application.isEditor && EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    SimulationSettingsValue actualValue = ((SimulationSettings)target).Value;
                    EnvironmentComponent editorRuntimeEnvComponent = FindObjectOfType<EnvironmentComponent>();
                    if (editorRuntimeEnvComponent != null)
                    {
                        IntPtr environment = editorRuntimeEnvComponent.Environment().GetEnvironment();
                        PhononCore.iplSetNumBounces(environment, actualValue.RealtimeBounces);
                    }
                }
            }

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }
    }
}