//
// Copyright (C) Valve Corporation. All rights reserved.
//

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Phonon
{
    enum SettingsTab
    {
        General,
        SceneExport,
        Simulator,
        Support
    }

    public class PhononSettingsWindow : EditorWindow
    {

        //
        // Mapping from tabs to tab pane classes.
        //
        static Dictionary<SettingsTab, string> 	SettingsTabNames;
        static Dictionary<SettingsTab, string> 	SettingsTabClasses;
        static string[]							SettingsTabNamesValues;

        static PhononSettingsWindow()
        {
            SettingsTabNames = new Dictionary<SettingsTab, string>();
            SettingsTabNames.Add(SettingsTab.General, "General");
            SettingsTabNames.Add(SettingsTab.SceneExport, "Scene");
            SettingsTabNames.Add(SettingsTab.Simulator, "Simulation");
            SettingsTabNames.Add(SettingsTab.Support, "Help");

            SettingsTabClasses = new Dictionary<SettingsTab, string>();
            SettingsTabClasses.Add(SettingsTab.General, "Phonon.PhononCommonPane");
            SettingsTabClasses.Add(SettingsTab.SceneExport, "Phonon.PhononScenePane");
            SettingsTabClasses.Add(SettingsTab.Simulator, "Phonon.PhononSimulatorPane");
            SettingsTabClasses.Add(SettingsTab.Support, "Phonon.PhononSupportPane");

            SettingsTabNamesValues = new string[SettingsTabNames.Count];
            SettingsTabNames.Values.CopyTo(SettingsTabNamesValues, 0);
        }

        [MenuItem("Window/Phonon")]
        static void Init()
        {
#pragma warning disable 618
            PhononSettingsWindow window = EditorWindow.GetWindow<PhononSettingsWindow>();
            window.title = "Phonon";
            window.Show();
#pragma warning restore 618
        }
        
        void OnEnable()
        {
            autoRepaintOnSceneChange = true;
        }
        
        void OnInspectorUpdate()
        {
            Repaint();
        }
        
        void OnGUI()
        {
            EditorGUILayout.Space();
            selectedTab = (SettingsTab) GUILayout.Toolbar((int) selectedTab, SettingsTabNamesValues);		
            EditorGUILayout.Space();

            if (SettingsTabClasses.ContainsKey(selectedTab))
                Type.GetType(SettingsTabClasses[selectedTab]).GetMethod("DrawPane").Invoke(null, null);
        }

        SettingsTab selectedTab	= SettingsTab.General;
    }
}