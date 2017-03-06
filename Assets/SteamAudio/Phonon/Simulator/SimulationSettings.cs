//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;

namespace Phonon
{
    //
    // SimulationSettings
    // Component for specifying simulation settings as a preset or as custom values.
    //
    public class SimulationSettings : MonoBehaviour
    {
        //
        // Data members.
        //

        // The currently selected preset.
        public SimulationSettingsPreset Preset;

        // The actual value of the simulation settings.
        public SimulationSettingsValue Value;
    }
}