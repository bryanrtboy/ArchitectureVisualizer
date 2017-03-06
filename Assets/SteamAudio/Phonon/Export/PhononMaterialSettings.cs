//
// Copyright (C) Valve Corporation. All rights reserved.
//

using UnityEngine;

namespace Phonon
{

    //
    // PhononMaterialSettings
    // The global GameObject that controls material settings for Phonon.
    //

    public static class PhononMaterialSettings
    {
        //
        // Returns the settings object, creating it if necessary.
        //
        public static GameObject GetObject()
        {
            // The name of the object.
            string name = "Phonon Material Settings";

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

                    // Add a default material component.
                    existingObject.AddComponent<PhononMaterial>();
                    PhononMaterial acousticMaterial = existingObject.GetComponent<PhononMaterial>();
                    acousticMaterial.Preset = PhononMaterialPreset.Generic;
                    acousticMaterial.Value = PhononMaterialPresetList.PresetValue((int)PhononMaterialPreset.Generic);
                }

                // Point our reference to the object.
                settingsObject = existingObject;
            }

            // Return the object.
            return settingsObject;
        }

        //
        // Returns the default material component.
        //
        public static PhononMaterial GetDefaultMaterial()
        {
            return GetObject().GetComponent<PhononMaterial>();
        }

        //
        // Data members.
        //

        // The GameObject in which the phonon settings are stored.
        static GameObject settingsObject;
    }
}