//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    //
    // BakedReverbInspector
    // Custom inspector for BakedReverb.
    //

    [CustomEditor(typeof(BakedReverb))]
    public class BakedReverbInspector : Editor
    {
        //
        // Draws the inspector GUI.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeConvolution"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeParameteric"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));

            if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

            BakedReverb bakedReverb = serializedObject.targetObject as BakedReverb;
            GUI.enabled = !oneBakeActive && !BakedSourceInspector.oneBakeActive && !BakedStaticListenerInspector.oneBakeActive;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Bake Effect"))
            {
                Debug.Log("START: Baking reverb effect.");
                oneBakeActive = true;
                bakedReverb.BeginBake();
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            DisplayProgressBarAndCancel();

            if (bakedReverb.bakeStatus == BakeStatus.Complete)
            {
                bakedReverb.EndBake();
                oneBakeActive = false;
                Repaint();
                Debug.Log("COMPLETED: Baking reverb effect.");
            }

            serializedObject.ApplyModifiedProperties();
        }


        void DisplayProgressBarAndCancel()
        {
            BakedReverb bakedReverb = serializedObject.targetObject as BakedReverb;
            if (bakedReverb.bakeStatus != BakeStatus.InProgress)
                return;

            float progress = bakedReverb.probeBoxBakingProgress + .01f; //Adding an offset because progress bar when it is exact 0 has some non-zero progress.
            int progressPercent = Mathf.FloorToInt(Mathf.Min(progress * 100.0f, 100.0f));
            string progressString = "Baking " + bakedReverb.probeBoxBakingCurrently + "/" + bakedReverb.totalProbeBoxes + " Probe Box (" + progressPercent.ToString() + "% complete)";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, progressString);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Cancel Bake"))
            {
                bakedReverb.cancelBake = true;          // Ensures partial baked data is not serialized and that bake is properly cancelled for multiple probe boxes.
                PhononCore.iplCancelBake();
                bakedReverb.EndBake();
                oneBakeActive = false;
                bakedReverb.cancelBake = false;
                Debug.Log("CANCELLED: Baking reverb effect.");
            }
            EditorGUILayout.EndHorizontal();
            Repaint();
        }

        static public bool oneBakeActive = false;
    }
}