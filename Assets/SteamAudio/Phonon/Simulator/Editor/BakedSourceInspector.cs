//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    //
    // BakedSourceInspector
    // Custom inspector for BakedSource.
    //

    [CustomEditor(typeof(BakedSource))]
    public class BakedSourceInspector : Editor
    {
        //
        // Draws the inspector GUI.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useBakedStaticListener"));
            if (serializedObject.FindProperty("useBakedStaticListener").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bakedListenerIdentifier"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("uniqueIdentifier"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeConvolution"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeParameteric"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bakingRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));

                if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

                BakedSource bakedSource = serializedObject.targetObject as BakedSource;
                GUI.enabled = !oneBakeActive && !BakedReverbInspector.oneBakeActive && !BakedStaticListenerInspector.oneBakeActive;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(" ");
                if (GUILayout.Button("Bake Effect"))
                {
                    Debug.Log("START: Baking effect for \"" + bakedSource.uniqueIdentifier + "\" with influence radius of " + bakedSource.bakingRadius + " meters.");
                    oneBakeActive = true;
                    bakedSource.BeginBake();
                }
                EditorGUILayout.EndHorizontal();

                GUI.enabled = true;

                DisplayProgressBarAndCancel();

                if (bakedSource.bakeStatus == BakeStatus.Complete)
                {
                    bakedSource.EndBake();
                    oneBakeActive = false;
                    Repaint();
                    Debug.Log("COMPLETED: Baking effect for \"" + bakedSource.uniqueIdentifier + "\" with influence radius of " + bakedSource.bakingRadius + " meters.");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DisplayProgressBarAndCancel()
        {
            BakedSource bakedSource = serializedObject.targetObject as BakedSource;
            if (bakedSource.bakeStatus != BakeStatus.InProgress)
                return;

            float progress = bakedSource.probeBoxBakingProgress + .01f; //Adding an offset because progress bar when it is exact 0 has some non-zero progress.
            int progressPercent = Mathf.FloorToInt(Mathf.Min(progress * 100.0f, 100.0f));
            string progressString = "Baking " + bakedSource.probeBoxBakingCurrently + "/" + bakedSource.totalProbeBoxes + " Probe Box (" + progressPercent.ToString() + "% complete)";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, progressString);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Cancel Bake"))
            {
                bakedSource.cancelBake = true;          // Ensures partial baked data is not serialized and that bake is properly cancelled for multiple probe boxes.
                PhononCore.iplCancelBake();
                bakedSource.EndBake();
                oneBakeActive = false;
                bakedSource.cancelBake = false;
                Debug.Log("CANCELLED: Baking effect for \"" + bakedSource.uniqueIdentifier + "\" with influence radius of " + bakedSource.bakingRadius + " meters.");
            }
            EditorGUILayout.EndHorizontal();
            Repaint();
        }

        static public bool oneBakeActive = false;
    }
}