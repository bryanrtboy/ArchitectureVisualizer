//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    //
    // PhononEffectInspector
    // Custom inspector for PhononEffect components.
    //

    [CustomEditor(typeof(PhononEffect))]
    [CanEditMultipleObjects]
    public class PhononEffectInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Direct Sound UX
            PhononGUI.SectionHeader("Direct Sound");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("directBinauralEnabled"));
            if (serializedObject.FindProperty("directBinauralEnabled").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hrtfInterpolation"), new GUIContent("HRTF Interpolation"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("directOcclusionOption"));
            if (serializedObject.FindProperty("directOcclusionOption").enumValueIndex == (int) OcclusionOption.Partial)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("partialOcclusionRadius"), new GUIContent("Source Radius (meters)"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("physicsBasedAttenuation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("directMixFraction"));

            // Indirect Sound UX
            PhononGUI.SectionHeader("Reflected Sound");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableReflections"));

            if (serializedObject.FindProperty("enableReflections").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("simulationMode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectMixFraction"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("diffractionEnabled"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectBinauralEnabled"));

                if (serializedObject.FindProperty("indirectBinauralEnabled").boolValue)
                    EditorGUILayout.HelpBox("The binaural setting is ignored if Phonon Mixer component is attached to Audio Listener.", MessageType.Info);

                EditorGUILayout.HelpBox("Go to Windows > Phonon > Simulation to update the global baking simulation settings.", MessageType.Info);
                PhononEffect phononEffect = serializedObject.targetObject as PhononEffect;

                BakedSource bakedSource = phononEffect.GetComponent<BakedSource>();
                BakedReverb bakedReverb = phononEffect.GetComponent<BakedReverb>();
                bool bakeSimulationMode = (phononEffect.simulationMode == SimulationType.Baked);
                if (bakeSimulationMode && bakedSource == null && bakedReverb == null)
                    EditorGUILayout.HelpBox("Make sure to add a Baked Source or a Baked Reverb component when using bake simulation for reflection sound.", MessageType.Error);
            }

            // Save changes.
            serializedObject.ApplyModifiedProperties();
        }
    }
}