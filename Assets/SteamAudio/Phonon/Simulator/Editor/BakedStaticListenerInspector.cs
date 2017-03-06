//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{
    //
    // BakedStaticListenerInspector
    // Custom inspector for BakedStaticListener.
    //

    [CustomEditor(typeof(BakedStaticListener))]
    public class BakedStaticListenerInspector : Editor
    {
        //
        // Draws the inspector GUI.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uniqueIdentifier"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeConvolution"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("bakeParameteric"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bakingRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));

            if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

            BakedStaticListener bakedStaticListener = serializedObject.targetObject as BakedStaticListener;
            GUI.enabled = !oneBakeActive && !BakedReverbInspector.oneBakeActive && !BakedSourceInspector.oneBakeActive;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Bake Effect"))
            {
                Debug.Log("START: Baking effect for \"" + bakedStaticListener.uniqueIdentifier + "\".");
                oneBakeActive = true;
                bakedStaticListener.BeginBake();
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            DisplayProgressBarAndCancel();

            if (bakedStaticListener.bakeStatus == BakeStatus.Complete)
            {
                bakedStaticListener.EndBake();
                oneBakeActive = false;
                Repaint();
                Debug.Log("COMPLETED: Baking effect for \"" + bakedStaticListener.uniqueIdentifier + "\".");
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DisplayProgressBarAndCancel()
        {
            BakedStaticListener bakedStaticListener = serializedObject.targetObject as BakedStaticListener;
            if (bakedStaticListener.bakeStatus != BakeStatus.InProgress)
                return;

            float progress = bakedStaticListener.probeBoxBakingProgress + .01f; //Adding an offset because progress bar when it is exact 0 has some non-zero progress.
            int progressPercent = Mathf.FloorToInt(Mathf.Min(progress * 100.0f, 100.0f));
            string progressString = "Baking " + bakedStaticListener.probeBoxBakingCurrently + "/" + bakedStaticListener.totalProbeBoxes + " Probe Box (" + progressPercent.ToString() + "% complete)";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, progressString);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Cancel Bake"))
            {
                bakedStaticListener.cancelBake = true;          // Ensures partial baked data is not serialized and that bake is properly cancelled for multiple probe boxes.
                PhononCore.iplCancelBake();
                bakedStaticListener.EndBake();
                oneBakeActive = false;
                bakedStaticListener.cancelBake = false;
                Debug.Log("CANCELLED: Baking effect for \"" + bakedStaticListener.uniqueIdentifier + "\".");
            }
            EditorGUILayout.EndHorizontal();
            Repaint();
        }

        static public bool oneBakeActive = false;
    }
}