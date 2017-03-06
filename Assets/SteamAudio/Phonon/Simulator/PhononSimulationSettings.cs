//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;

namespace Phonon
{
    //
    // PhononSimulationSettings
    // The global GameObject that controls simulation settings for Phonon.
    //

    public static class PhononSimulationSettings
    {
        //
        // Returns the settings object, creating it if necessary.
        //
        public static GameObject GetObject()
        {
            // The name of the object.
            string name = "Phonon Simulation Settings";

            // If the reference is null, we need to point it to the object.
            if (settingsObject == null)
            {
                // Try finding the object.
                GameObject existingObject = GameObject.Find(name);

                // If the object couldn't be found, we need to create it.
                if (existingObject == null)
                {
                    // Create the object.
                    existingObject = new GameObject(name);

                    // Add a simulation bake settings component.
                    existingObject.AddComponent<SimulationSettings>();
                    SimulationSettings simulationSettings = existingObject.GetComponent<SimulationSettings>();
                    simulationSettings.Value  = new SimulationSettingsValue();
                    simulationSettings.Value.CopyFrom(SimulationSettingsPresetList.PresetValue((int)simulationSettings.Preset));
                }

                // Point our reference to the object.
                settingsObject = existingObject;
            }

            // Return the object.
            return settingsObject;
        }

        //
        // Returns the default settings component.
        //
        public static SimulationSettings GetSimulationSettings()
        {
            return GetObject().GetComponent<SimulationSettings>();
        }

        //
        // Data members.
        //

        // The GameObject in which the Phonon simulation settings are stored.
        static GameObject settingsObject;
    }
}