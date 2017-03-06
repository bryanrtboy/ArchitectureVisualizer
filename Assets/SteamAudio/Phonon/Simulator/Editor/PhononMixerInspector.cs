//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;

namespace Phonon
{

    //
    // PhononMixerInspector
    // Custom inspector for the PhononMixer component.
    //

    [CustomEditor(typeof(PhononMixer))]
    public class PhononMixerInspector : Editor
    {

        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectBinauralEnabled"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}