//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;

namespace Phonon
{

    //
    // AudioEngineInspector
    // Custom inspector for a AudioEngineComponent component.
    //

    [CustomEditor(typeof(AudioEngineComponent))]
    public class AudioEngineInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PhononGUI.SectionHeader("Audio Engine Integration");

            string[] engines = { "Unity Audio" };
            var audioEngineProperty = serializedObject.FindProperty("audioEngine");
            audioEngineProperty.enumValueIndex = EditorGUILayout.Popup("Audio Engine", audioEngineProperty.enumValueIndex, engines);

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        int EnumValueToPopupIndex(AudioEngine enumValue)
        {
            switch (enumValue)
            {
                case AudioEngine.Unity:
                    return 0;
                case AudioEngine.Wwise:
                    return 1;
                case AudioEngine.FMODStudio:
                    return 2;
                default:
                    return -1;
            }
        }

        AudioEngine PopupIndexToEnumValue(int popupIndex)
        {
            switch (popupIndex)
            {
                case 0:
                    return AudioEngine.Unity;
                case 1:
                    return AudioEngine.Wwise;
                case 2:
                    return AudioEngine.FMODStudio;
                default:
                    return AudioEngine.Unity;
            }
        }
    }
}